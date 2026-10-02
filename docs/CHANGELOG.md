# Changelog

## 2026-10-02 — Production revamp

### Bug fixes

- **Death animation never played.** No `Death` state existed in
  `Character.controller` (only a clip in the library). Added an `isDead` bool
  parameter, a `Death` state (wired to `Aj@Standing Death Backward 01`), and an
  AnyState→Death transition; `PlayerController` now sets `isDead = true` on
  death.
- **Multi-hit drain** — the original `PlayerHealth` had no invulnerability
  window, so a lingering trigger overlap could consume several hearts in one
  pass. 1.2 s of i-frames now make `Hurt()` safe to call repeatedly.
- **Restarting while paused froze the next run** (`Time.timeScale` stayed 0).
  `GameManager.Start` now resets the time scale on scene entry.
- **HUD showed stale placeholder text** from the saved scene until the score
  first changed. HUD caches start at `-1` so the first frame writes real zeros.
- **Coin pickup had no audio or effect** — SFX and sparkle burst are now
  raised from `GameManager.CollectCoin`.
- **Runtime GPU resource leak** — `ParticleFX` materials/texture now destroyed
  in `OnDestroy`; the shared particle texture is created once, not per material.
- **Static event leaks** — all `GameEvents` subscriptions are removed in
  `OnDestroy` (the old code only removed one).
- **Misnamed scripts** — `ObsticleSpawner.cs` → `ObstacleSpawner.cs`,
  `EnvirnomentSpawner.cs` → `EnvironmentSpawner.cs` (GUIDs preserved, so scene
  references stay valid).
- **Spawn distance 1021 m** → 150 m (fog hides the spawn line; far less dead
  space to simulate — see [PERFORMANCE.md](PERFORMANCE.md)).
- Stale serialized fields (`isPaused`, `isOver`) removed from the scene's
  `GameManager` block (the properties still exist on the script).

### New features

- **Audio system** (`AudioDirector` + 10 new synthesized clips in
  `_Audio/`): looping music per scene, pooled SFX (click, coin, jump, slide,
  hit, game-over, countdown, go), music ducking while paused, persisted
  mute toggles (`M`/`N`).
- **Screen FX** (`ScreenFX`): runtime vignette + red damage flash.
- **Particle FX** (`ParticleFX`): coin sparkles, hit bursts, landing dust,
  confetti on a new record — all pooled, all created in code.
- **Scene transitions** (`SceneFader`): fade-to-black between Menu and
  Gameplay (persistent overlay, DOTween-safe for the bundled 1.2.340).
- **Countdown polish**: 3-2-1-GO with beeps, scale/fade pop, "GO!" SFX.
- **Pause panel polish**: animated in/out, `Esc`/`P` hotkey, music duck.
- **Game-over panel**: delayed pop-in after the death animation, results
  (score/coins/time), **best score + NEW RECORD flag**, confetti on a record.
- **Main menu**: floating title, button pop-in, **best score display**
  (hidden until you have one).
- **Damage feedback**: camera shake + slow-mo + invulnerability frames
  (1.2 s) so multi-hits can't chain-drain health.
- **Lane-change lean** on the character model.
- **Coin magnet** (1.7 m pull radius).
- **Touch input**: swipe left/right/up/down (24 px threshold), one action per
  touch.
- **Difficulty curve**: obstacle gap scales with speed (8→24 m); speed ramp
  now caps at 26 m/s (the original ramp was unbounded — it grew forever).
- **Fair coin placement**: coin rows only inside gaps between blockers.
- **Central event hub** (`GameEvents`) replacing ad-hoc static events with a
  documented API.

### Code quality

- All scripts wrapped in `namespace RunnerGame`, XML-doc'd public APIs,
  headers/tooltips on inspector fields, consistent naming (`_camelCase` fields).
- Spawners refactored to growable pools with index stacks (no LINQ, no
  per-frame allocation).
- Null-safe singleton pattern (`Instance` + `Destroy` of duplicates) on every
  scene-level singleton; `AudioDirector`/`ScreenFX`/`ParticleFX` are safe if a
  scene lacks the object.
- `PlayerController` input and actions split into small methods; jump/slide
  tweens capture the start height on the body transform itself (the original
  read the player root's Y at tween-completion time), which is robust to any
  local offset on the body.
- DOTween usage audited against the **bundled 1.2.340** API surface
  (see [FX.md](FX.md) for the compatibility table).

### Documentation

- New `docs/` folder (architecture, gameplay, audio, FX, performance,
  changelog, known issues) and a rewritten `README.md`.

## Before this revamp

Initial assignment version: basic runner (3 lanes, 3 hearts, pause, game
over) with placeholder audio (none), fixed spawn distances, unbounded speed,
and the `ObsticleSpawner`/`EnvirnomentSpawner` naming.
