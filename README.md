# RunnerGame

A polished endless 3-lane runner built with **Unity 2022.3.5f1** (Built-in Render Pipeline).
Dodge blockers, collect coins, survive as long as you can.

| | |
|---|---|
| Engine | Unity 2022.3.5f1 (LTS), Built-in RP |
| Input | Keyboard (WASD / arrows) + touch swipes |
| Scenes | `Menu` → `Gameplay` (both in Build Settings) |
| UI | TextMeshPro, DOTween 1.2.340 (bundled) |

## Getting started

1. Open the project root in the Unity 2022.3.5f1 Hub.
2. Press **Play** — the `Menu` scene is the first scene in Build Settings.
3. Or open `Assets/Scenes/Gameplay.unity` directly to jump straight into a run.

> The `Fantasy Skybox FREE` Asset Store pack (≈1.4 GB) is part of the project and
> required for the `FS013_Sunrise` skybox used by the gameplay scene. Keep it in
> place (or track it with Git LFS — see [docs/PERFORMANCE.md](docs/PERFORMANCE.md)).

## Controls

| Action | Keyboard | Touch |
|---|---|---|
| Move left | `A` / `←` | Swipe left |
| Move right | `D` / `→` | Swipe right |
| Jump | `W` / `↑` / `Space` | Swipe up |
| Slide | `S` / `↓` | Swipe down |
| Pause / resume | `Esc` / `P` (or HUD button) | HUD button |
| Toggle music | `M` | — |
| Toggle sound effects | `N` | — |

Gameplay is explained in [docs/GAMEPLAY.md](docs/GAMEPLAY.md).

## Project structure

```
Assets/
├── Scenes/               # Menu.unity, Gameplay.unity
├── _Scripts/             # All game code (namespace RunnerGame)
├── _Prefabs/             # Obstacle, Coin, environment chunk, blockers
├── _Models/              # Source models (coin, blockers)
├── _Audio/               # Music + SFX clips (synthesized, project-owned)
├── _UI/                  # HUD sprites (heart icon)
├── Provided/             # Assignment assets: character, animations, environment
├── TextMesh Pro/         # TMP package assets
├── Plugins/Demigiant/    # DOTween 1.2.340
└── Fantasy Skybox FREE/  # Asset Store skybox pack (see note above)
docs/                     # Project documentation
```

## Documentation

- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — systems, scene layout, code map, event flow
- [docs/GAMEPLAY.md](docs/GAMEPLAY.md) — mechanics, difficulty curve, scoring
- [docs/AUDIO.md](docs/AUDIO.md) — audio system, clip list, mute options
- [docs/FX.md](docs/FX.md) — screen FX, particles, scene fader, DOTween notes
- [docs/PERFORMANCE.md](docs/PERFORMANCE.md) — pooling, budgets, repo size / Git LFS
- [docs/CHANGELOG.md](docs/CHANGELOG.md) — what changed in the production revamp
- [docs/KNOWN_ISSUES.md](docs/KNOWN_ISSUES.md) — editor-side cleanups and limitations

## Credits

- Character & animations: provided assignment assets (`Assets/Provided`)
- Coin + blocker models: provided assignment assets (`Assets/_Models`)
- Skybox: *Fantasy Skybox FREE* (Unity Asset Store)
- DOTween: Demigiant (bundled)
- All music/SFX: synthesized for this project (`Assets/_Audio`)
