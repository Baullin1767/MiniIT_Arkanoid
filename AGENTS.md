# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project Overview

A 2D Arkanoid (brick-breaker) game built in Unity 6 (6000.2.7f2) using URP. Single-scene game (`SampleScene.unity`) targeting Windows Standalone and mobile (Android/iOS). All scripts use namespace `MiniIT.ARKANOID` (except `AudioServiceInstaller` which lacks a namespace).

## Build & Run

This is a Unity project — open in Unity Editor 6000.2.7f2. No CI/CD, build scripts, or test suites exist. The scripting define `DOTWEEN` is set for all platforms.

## Architecture

### Dependency Injection (Extenject/Zenject 9.2.0)

All wiring goes through four `MonoInstaller` scripts in `Assets/Installers/`:
- **SignalsInstaller** — declares all signals on the `SignalBus`
- **GameInstaller** — binds `GameManager`, `LevelManager`, `TweenEffects`, `GameSettings`, `IInputService`
- **UIInstaller** — binds UI components from scene hierarchy
- **AudioServiceInstaller** — binds `AudioService` with AudioSource refs and clip dictionary

MonoBehaviours receive dependencies via `[Inject]`-annotated `Construct()` methods (not constructors).

### Signal Bus (Event System)

All cross-system communication uses Zenject's `SignalBus`. Signals defined in `Assets/Scripts/Core/Signals/GameSignals.cs`:

| Signal | Purpose |
|---|---|
| `BallLostSignal` | Ball fell out of bounds → decrement lives |
| `BrickDestroyedSignal` | Brick destroyed → add score, check level complete |
| `ScoreChangedSignal(int)` | Update score UI |
| `LivesChangedSignal(int)` | Update lives UI |
| `GameOverSignal` | Lives = 0 → show Game Over panel |
| `LevelCompletedSignal` | All bricks gone → show Win panel |
| `LevelResetSignal` | Level restarted → respawn bricks, re-attach ball |

To add a new signal: define it in `GameSignals.cs`, declare in `SignalsInstaller`, subscribe/fire via `signalBus`.

### Game Flow

`GameStarter.Start()` → `GameManager.StartGame()` → `GameManager.RestartGame(resetScore)`. Win restart preserves score (`resetScore: false`), Game Over resets it (`resetScore: true`). Game pauses via `Time.timeScale = 0`.

### Key Patterns

- **`GameManager`** is a plain C# class (Zenject singleton); **`LevelManager`** is a MonoBehaviour (needs SerializeField references to prefabs/transforms)
- **Object pooling**: `LevelManager` pools bricks in `Dictionary<BrickType, Queue<BrickBase>>`, pre-warmed on first reset
- **Brick hierarchy**: `BrickBase` (abstract) → `StandardBrick`, `ReinforcedBrick`. New types need: subclass, enum value in `BrickLayoutAsset.cs`, prefab, and `LevelManager` switch cases
- **Input abstraction**: `IInputService` → `DesktopInputService` (legacy Input API) / `MobileInputService` (touch). Selected at bind time via platform defines in `GameInstaller`
- **UI**: `UIController` mediates between signals and views (`HUDView`, `GameOverPanel`, `WinPanel`). Panels use Doozy UIView for animations
- **Audio**: `AudioService` (plain C# class) wraps two AudioSources (music + SFX), subscribes to signals directly

### Configuration

- `GameSettings` ScriptableObject at `Resources/Configs/Game Settings.asset` — lives, paddle speed, ball speed, boundary limits
- `BrickLayoutAsset` ScriptableObject at `Resources/Configs/BrickLayout.asset` — brick row definitions. Multiple layouts assigned to `LevelManager`; one chosen randomly per level

### Physics

2D physics. Ball velocity maintained explicitly in `Ball.MaintainSpeedAfterCollision()` after non-paddle collisions with a `wallBounceMultiplier`. Paddle uses `Rigidbody2D.MovePosition()` in `FixedUpdate`.

## Third-Party Libraries

| Library | Location | Purpose |
|---|---|---|
| Extenject (Zenject) 9.2.0 | `Assets/Plugins/Zenject/` | DI + Signal Bus |
| DOTween | `Assets/Plugins/Demigiant/DOTween/` | Tween animations |
| Doozy UI | `Assets/Doozy/` | Animated UI panels and buttons |
