# FX and Polish

Effects are implemented with three purpose-built components plus DOTween.
Everything is created in code at runtime — **no effect prefabs, no material
assets**, so the FX layer can't drift out of sync with the scenes.

## ScreenFX (`ScreenFX.cs`)

Attached to each scene's `Canvas` (one `Instance` per scene).

- Builds a full-screen `FX_Overlay` at runtime on top of the host canvas:
  - **Vignette** — a 128 px radial gradient `Texture2D` generated in code,
    constant subtle darkening at the edges (strength serialized).
  - **Hit flash** — a full-screen image tinted with `hitFlashColor`, hidden by
    default; `FlashHit()` pops it to `hitFlashAlpha` and fades it back out.
- All tweens use `SetUpdate(true)` so feedback still animates while the game
  is paused (slow-mo, pause menu).

```csharp
ScreenFX.Instance?.FlashHit();                          // damage
ScreenFX.Instance?.Flash(Color.green, 0.3f, 0.5f);      // generic flash
```

## ParticleFX (`ParticleFX.cs`)

Attached to the Gameplay `Player` GO (`Instance` static). Spawns
**world-space** particle bursts from a pool (default max 24 systems, growing
on demand):

| Effect | Trigger | Look |
|---|---|---|
| `BurstCoin(pos)` | Coin pickup | 14 gold additive sparks, slight gravity |
| `BurstHit(pos)` | Damage | 22 orange/red additive sparks |
| `BurstDust(pos)` | Landing from a jump | 10 soft dust puffs, float up |
| `BurstConfetti()` | New record on game-over | 60 soft confetti bits in front of the camera |

Implementation notes:

- Particle texture (64 px radial soft dot) and the two materials (soft-alpha
  and additive `Sprites/Default`) are generated in `Awake` and **destroyed in
  `OnDestroy`** (runtime GPU resources don't die with scene unloads otherwise).
- Effects pool by deactivating after their lifetime
  (`WaitForSecondsRealtime`), so a burst never leaks.
- `renderQueue = Transparent + 1`, shadows/light probes off, `maxParticles 128`
  per system — the whole pool is a rounding error for the renderer.

## SceneFader (`SceneFader.cs`)

Persistent (created on first `Get()`, `DontDestroyOnLoad`), ScreenSpaceOverlay
canvas at `sortingOrder 10000` — above all game UI.

`FadeAndLoad(sceneName)`:

1. Fade to black (0.35 s, unscaled).
2. `SceneManager.LoadSceneAsync` (manual wait loop).
3. `WaitForEndOfFrame`, then fade back in (0.5 s, unscaled).

A `_busy` guard ignores re-entrancy (double-clicking Play is harmless).

> **Why manual wait loops?** The bundled DOTween is **1.2.340**, which predates
> `tween.AsyncWaitable()`. Polling `color.a` across `yield return null` is the
> compatible idiom.

## DOTween usage and version caveats

The project ships **DOTween 1.2.340** (`Assets/Plugins/Demigiant/`). A few APIs
that exist in newer DOTween **do not exist here** — code in this repo must use
the 1.2.340 equivalents:

| Do NOT use (newer API) | Use instead (1.2.340) |
|---|---|
| `tween.SetAs("id")` | `tween.SetId("id")` |
| `target.DOKill("id")` | `DOTween.Kill(target, "id")` (static) |
| `target.DOLocalRotate(v, dur, false)` | `target.DOLocalRotate(v, dur, RotateMode.Fast)` |
| `tween.AsyncWaitable()` | manual `while` + `yield return null` |

Other conventions used throughout:

- UI tweens always set `SetUpdate(true)` (unscaled) so panels/countdowns keep
  animating during pause and slow-mo.
- One target, many concurrent tweens → kill by id (`DOTween.Kill(target, id)`),
  never a blanket `DOKill()` that would cancel unrelated tweens
  (see the lane-lean tween in `PlayerController.MoveLane`).
- `DOShakePosition(duration, strength, vibrato, sharpness)` — mind the
  parameter types (`float sharpness`, not a bool).
