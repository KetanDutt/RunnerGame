# Gameplay

## Objective

Run forward as long as possible. The track never ends — speed ramps up until it
caps, blockers get denser, and you eventually crash. Score = metres travelled;
coins are collected on contact.

## Controls

| Action | Keyboard | Touch |
|---|---|---|
| Move left / right | `A` / `←`, `D` / `→` | Swipe left / right |
| Jump | `W` / `↑` / `Space` | Swipe up |
| Slide | `S` / `↓` | Swipe down |
| Pause / resume | `Esc` / `P` | HUD button |
| Music on/off | `M` | — |
| SFX on/off | `N` | — |

Swipe threshold is 24 px (serialized on `PlayerController`). A single swipe is
consumed per touch so a drag doesn't fire multiple actions.

## Health and damage

- The player has **3 hearts** (HUD, top-left).
- Hitting a blocker costs one heart. After each hit the player gets
  **1.2 s of invulnerability** (a lingering overlap or a double hit in the same
  frame cannot chain-drain health).
- Damage feedback: hit SFX, red screen flash, camera shake, and a short
  **slow-mo** (`Time.timeScale = 0.35` for 0.3 s).
- Third hit → death: the character plays the `Death` state (via the `isDead`
  bool in `Character.controller`), music stops, the game-over panel pops in
  after ~1 s.

## Blockers

One blocker per row, in one of three lanes. The obstacle prefab
(`Assets/_Prefabs/Obsticle.prefab`) carries three nested variants; the spawner
activates exactly one per row:

| Variant | How to pass |
|---|---|
| `BlockerJump` (low) | Jump over it |
| `BlockerStandard` (tall, low ceiling) | Slide under it |
| `BlockerRoll` (tall, low ceiling) | Slide under it |

To avoid unfair walls: the spawner rarely repeats the same lane (`60 %` chance
to pick a different one) and rarely repeats the same variant back-to-back
(`70 %` chance to vary).

## Coins

- Rows of 4–8 coins spawn **only inside the gap** between two consecutive
  blockers, so a coin row can never end up inside the next obstacle.
- Coin rows usually sit in a different lane than the blocker, rewarding a lane
  change.
- **Coin magnet**: coins within 1.7 m of the player are gently pulled in
  (`Vector3.MoveTowards`, 20 m/s) for a satisfying pickup.
- Pickup: coin deactivates, +1 coin counter, sparkle burst, coin SFX.

## Difficulty curve

- Speed ramps from **10 → 26 m/s** (≈ +0.4 m/s per second, hard cap).
- The obstacle gap scales with speed: `lerp(8 m, 24 m, speedFactor)` — more
  room to react as it gets faster.
- Score accrues as metres travelled (1 point per metre).

## Scoring and high score

- `Score` — metres travelled, shown in the HUD.
- `Coins` — coins collected.
- `Time` — seconds survived.
- **High score** persists in `PlayerPrefs` under `RunnerGame.HighScore`,
  shown on the game-over panel (with a `* NEW RECORD *` flag + confetti when
  beaten) and on the main menu (hidden until you have a score).
