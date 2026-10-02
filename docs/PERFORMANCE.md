# Performance

## Object pooling (no runtime allocation in hot paths)

| Pooled system | Contents | Recycle rule |
|---|---|---|
| `ObstacleSpawner` | Obstacles + coins (separate pools, size 10 each) | Deactivate when behind the `End` marker (+2 m margin), reset to origin |
| `EnvironmentSpawner` | Ground chunks (size 10) | Deactivate when chunk center passes the `End` marker |
| `ParticleFX` | Particle systems (max 24) | Deactivate after lifetime (real-time) |
| `AudioDirector` | SFX `AudioSource`s (12) | Round-robin reuse; skipped when busy |

Pools **grow on demand** (`Instantiate` once) and never shrink, so there are no
allocations after warm-up. Steady-state active object count is small:
≈ 4–6 chunks, ≈ 4–8 obstacles, ≈ 20–40 coins, a handful of live particles.

## Spawning distances

- Obstacles/coins spawn **150 m** ahead of the player (the `Start` marker is a
  child of the player at local `z = 150` — reduced from the original 1021 m).
- The scene's **Exp2 fog (density 0.01)** already renders ≈ 90 % fogged at
  150 m, so new geometry fades in invisibly — no pop-in at the camera
  far plane (1000 m).

## Rendering

- Built-in render pipeline, single directional light, no post-processing stack.
- Particles: billboard mode, shadows off, light probes off, shared runtime
  materials (2 total), `maxParticles 128` per system.
- ScreenFX overlays are 2 UI draws (vignette + flash image).
- The gameplay skybox is a single panorama-based material
  (`Fantasy Skybox FREE/Panoramics/FS013/FS013_Sunrise.mat`).

## UI

- HUD text only updates when the displayed value changes (no per-frame string
  churn on `score`/`coins`/`time`).
- Panels animate with `CanvasGroup` alpha fades (one batch per canvas).

## Repository size and Git LFS

`Assets/Fantasy Skybox FREE` is the **1.4 GB** Asset Store pack (four cubemap
sets + panoramics + demo scenes). It is kept in the repo for
license-asset integrity and so the project opens as-is, but it dominates
clone size. Recommended workflow:

```bash
git lfs install
git lfs track "Assets/Fantasy Skybox FREE/*"
git add .gitattributes
# then move the tracked files into LFS:
git rm --cached -r "Assets/Fantasy Skybox FREE"
git add "Assets/Fantasy Skybox FREE"
```

If you only need the skybox the game actually uses,
`Panoramics/FS013/FS013_Sunrise.mat` (and its texture) is the one asset
referenced by `Gameplay.unity`; the pack may then be pruned or re-imported
from the Asset Store.
