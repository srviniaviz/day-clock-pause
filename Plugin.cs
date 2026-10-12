using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace DayClockPause;

[BepInPlugin("vinicius.dayclockpause", "Day Clock Pause", "0.1.0")]
public sealed class Plugin : BasePlugin
{
    internal static ConfigEntry<bool> Enabled;
    internal static ManualLogSource Logger;

    public override void Load()
    {
        Logger = Log;
        Enabled = Config.Bind("General", "Enabled", true,
            "Pause only the in-game day clock. Ctrl+Alt+P toggles this during play.");
        ClassInjector.RegisterTypeInIl2Cpp<ClockPauseBehaviour>();
        var obj = new GameObject("Day Clock Pause");
        UnityEngine.Object.DontDestroyOnLoad(obj);
        obj.AddComponent<ClockPauseBehaviour>();
        Log.LogInfo("Loaded. Hotkey: Ctrl+Alt+P. Default paused=" + Enabled.Value);
    }
}

public sealed class ClockPauseBehaviour : MonoBehaviour
{
    private DayCycleManager _manager;
    private float _originalGameTimeScale;
    private bool _hasOriginalScale;
    private float _noticeUntil;
    private string _notice = "";
    private float _nextStatusLog;

    public ClockPauseBehaviour(IntPtr pointer) : base(pointer) { }

    private void Update()
    {
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
            (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) &&
            Input.GetKeyDown(KeyCode.P))
        {
            Plugin.Enabled.Value = !Plugin.Enabled.Value;
            Notify(Plugin.Enabled.Value ? "Relogio do dia: parado" : "Relogio do dia: normal");
            Plugin.Logger.LogInfo("Clock pause=" + Plugin.Enabled.Value);
        }

        try { ApplyClockState(); }
        catch (Exception ex)
        {
            Plugin.Logger.LogError("Clock update failed: " + ex);
            RestoreScale();
        }
    }

    private void ApplyClockState()
    {
        var manager = DayCycleManager.Instance;
        if (manager == null)
        {
            _manager = null;
            _hasOriginalScale = false;
            return;
        }

        if (_manager == null || _manager.Pointer != manager.Pointer)
        {
            _manager = manager;
            _hasOriginalScale = false;
        }

        if (!Plugin.Enabled.Value)
        {
            RestoreScale();
            return;
        }

        // This field belongs to DayCycleManager. Unity's Time.timeScale is never changed.
        if (!_hasOriginalScale && manager.m_GameTimeScale > 0f)
        {
            _originalGameTimeScale = manager.m_GameTimeScale;
            _hasOriginalScale = true;
            Plugin.Logger.LogInfo("Captured game clock scale=" + _originalGameTimeScale);
        }

        if (!_hasOriginalScale) return;
        if (manager.m_GameTimeScale != 0f) manager.m_GameTimeScale = 0f;

        if (Time.unscaledTime >= _nextStatusLog)
        {
            _nextStatusLog = Time.unscaledTime + 15f;
            Plugin.Logger.LogInfo($"Clock paused: day={manager.CurrentDay}, time={manager.CurrentTime:F3}, gameScale={manager.m_GameTimeScale:F3}, unityScale={Time.timeScale:F3}");
        }
    }

    private void RestoreScale()
    {
        if (_hasOriginalScale && _manager != null && _manager.m_GameTimeScale == 0f)
        {
            _manager.m_GameTimeScale = _originalGameTimeScale;
            Plugin.Logger.LogInfo("Restored game clock scale=" + _originalGameTimeScale);
        }
        _hasOriginalScale = false;
    }

    private void OnDestroy() => RestoreScale();

    private void Notify(string message)
    {
        _notice = message;
        _noticeUntil = Time.unscaledTime + 5f;
    }

    private void OnGUI()
    {
        if (Time.unscaledTime < _noticeUntil)
            GUI.Box(new Rect(Screen.width - 290, 152, 275, 34), _notice);
    }
}
