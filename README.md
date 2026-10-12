# Day Clock Pause

A small BepInEx mod for **Supermarket Simulator** that pauses the in-game day clock while customers, staff, movement, and other gameplay keep running.

## Controls

Press **Ctrl+Alt+P** to pause or resume the day clock. A message appears in the upper-right corner. The clock starts paused by default, and the chosen state is saved in `BepInEx/config/vinicius.dayclockpause.cfg` (`Enabled = true/false`).

The mod changes `DayCycleManager.m_GameTimeScale` only. It does not change Unity's `Time.timeScale`, so it works independently of general speed controls such as FastForward Reborn.

## Installation

1. Install [Tobey's BepInEx 6 Pack for Supermarket Simulator](https://www.nexusmods.com/supermarketsimulator/mods/9) for the IL2CPP version of the game.
2. Extract the release ZIP into the game's root directory, beside `Supermarket Simulator.exe`.
3. Start the game through Steam.

The DLL should be at `BepInEx/plugins/DayClockPause/DayClockPause.dll`. To uninstall, remove that DLL.

Tested with Supermarket Simulator v1.7.1. The clock behavior was verified in the BepInEx log with normal and accelerated Unity time scales.

## Build

Requires .NET 6 SDK and a local game installation with BepInEx interop assemblies. Run `dotnet build -c Release` from this directory. Override the game folder with `-p:GameDir=...` if needed.

Licensed under MIT.
