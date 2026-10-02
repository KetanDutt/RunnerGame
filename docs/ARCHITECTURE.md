# Architecture

Overview of how the game is put together: scenes, systems, data flow and the
rules each script follows.

## Scenes

| Scene | Contents |
|---|---|
| `Menu` | Main menu UI (title, Play/Quit, best score), menu music, `AudioDirector`, `ScreenFX`, `SceneFader` (created on demand) |
| `Gameplay` | Player (+ child camera, Start/End markers), spawners, `GameManager`, `PlayerHealth`, `GameplayUI` (HUD/panels), `AudioDirector`, `ScreenFX`, `ParticleFX`, environment + skybox + fog |

Both scenes are registered (in this order) in *Edit → Project Settings → Editor
Build Settings*. `SceneFader` persists across loads via `DontDestroyOnLoad`.

### Gameplay scene layout (important quirks)

- The **Main Camera is a child of the Player** (local `0, 5, -5`) — the world
  appears static because the player runs forward under a fixed camera.
- **Start** (spawn marker, local `z = +150`) and **End** (destroy marker, local
  `z = -5`) are also children of the Player, so spawn/recycle distances are
  always relative to the player's current position.
- Obstacles and coins are children of the `ObsticleSpawner` object (they live in
  pools parented to it).

## Systems (code map)

All scripts live in `Assets/_Scripts/` under `namespace RunnerGame`.

| Script | Attached to | Responsibility |
|---|---|---|
| `GameManager` | Gameplay: `GameManager` GO | Run state machine (countdown → running → paused/over), score/coins/time, high score (PlayerPrefs), pause via `Time.timeScale` |
| `PlayerController` | Gameplay: `Player` GO | Movement, lane switching, jump, slide, keyboard + swipe input, speed ramp, death pose |
| `PlayerHealth` | Gameplay: `PlayerHealth` GO | 3 hearts, damage feedback (shake, flash, slow-mo, i-frames), death |
| `CollisionManager` | Gameplay: `Player/Body` | `OnTriggerEnter` for `Obstacle` and `Coin` tags |
| `ObstacleSpawner` | Gameplay: `ObsticleSpawner` GO | Pooled obstacle + coin spawning, gap-based difficulty, coin magnet, recycling |
| `EnvironmentSpawner` | Gameplay: `EnvironmentSpawner` GO | Pooled ground-chunk tiling + recycling (auto chunk width) |
| `Rotator` | Prefabs: `Coin` | Spin effect, gated on run state |
| `GameplayUI` | Gameplay: `Canvas` | HUD text, countdown, pause & game-over panels (DOTween), pause/mute hotkeys, best-score display |
| `MenuUI` | Menu: `Canvas` | Play/Quit, best score, animated title, button pop-in |
| `AudioDirector` | Each scene: `AudioDirector` GO | Music loop + pooled one-shot SFX, ducking, persisted mute options |
| `ScreenFX` | Each scene: `Canvas` | Runtime vignette + hit-flash overlay (built in code) |
| `ParticleFX` | Gameplay: `Player` GO | Pooled world-space particle bursts (coin/hit/dust/confetti), runtime textures + materials |
| `SceneFader` | Persistent (created on demand) | Black fade out → async scene load → fade in |
| `GameEvents` | — (static) | Central event hub (decouples systems) |
| `SceneNames` | — (static) | Build-Settings scene name constants |

## Event flow

`GameEvents` (static) is the single publish/subscribe hub:

```
GameStarted        GameManager ──► PlayerController (CrossFade "Run")
CoinCollected      GameManager ──► (reserved for modding)
PlayerHurt         PlayerHealth  ──► (reserved)
PlayerDied         PlayerHealth  ──► PlayerController (death pose, "isDead" param)
GameOver(score)    GameManager   ──► (reserved)
PauseChanged       GameManager   ──► (reserved)
```

Conventions:

- Systems **subscribe in `OnEnable` / unsubscribe in `OnDestroy`** so static
  events never hold destroyed objects (the old code leaked across scene loads).
- Subscribers must be **null-safe**; publishers use `RaiseX()` helpers.
- Direct calls are still used where two systems are tightly coupled
  (e.g. `GameManager → GameplayUI.ShowGameOver`), keeping the hub optional.

## A typical frame (while running)

1. `PlayerController.Update` — reads input, tweens lane/jump/slide, moves the
   player forward, calls `GameManager.AddScore(distance)`.
2. `ObstacleSpawner.Update` — spawns rows while `Start.z > nextSpawnZ`, recycles
   objects behind `End.z`, applies the coin magnet.
3. `EnvironmentSpawner.Update` — tiles ground chunks the same way.
4. Physics → `CollisionManager.OnTriggerEnter` → coin collect / `PlayerHealth.Hurt`.
5. `GameplayUI.Update` — HUD counters, `Esc`/`M`/`N` hotkeys.

## High-level rules

- **No allocation in hot paths**: all spawned objects come from growable pools
  (see [PERFORMANCE.md](PERFORMANCE.md)).
- **State flows one way**: `GameManager` is the only owner of run state;
  everything else queries `GameManager.Instance` / `GameManager.IsGameRunning()`.
- **Time-safe pausing**: anything that must keep running while paused uses
  `Time.unscaledDeltaTime`, `WaitForSecondsRealtime`, or DOTween
  `SetUpdate(true)`.
- **No async/await in gameplay code**: the bundled DOTween 1.2.340 has no
  `AsyncWaitable`, so `SceneFader` uses explicit wait loops
  (see [FX.md](FX.md)).
