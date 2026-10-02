# Known issues and editor-side cleanups

Things that were deliberately left as-is (verifying or changing them requires
opening the project in Unity), plus documented limitations.

## `Character.prefab` contains a hidden duplicate model

The character prefab instantiates the character model **twice**: the visible
model (with the `Character` Animator) and a nested `Character.FBX` instance
that is deactivated. It has no visible effect (the second model is off), but
it is dead weight and easy to confuse in the inspector. Removing it is an
editor-side task: open `Assets/_Prefabs/Character.prefab`, delete the
deactivated duplicate in the hierarchy, save. It was left untouched here
because, without Unity, the exact identity of the *visible* model couldn't be
proven — deleting the wrong one would break the character.

## Scene object still named `ObsticleSpawner`

The spawner GameObject in `Gameplay.unity` is still spelled `ObsticleSpawner`
(the script file was renamed; the GameObject name is cosmetic and not
referenced by code — all references are by fileID). Rename it in the Hierarchy
if you like.

## Menu scene uses the default skybox

`Gameplay.unity` uses the `FS013_Sunrise` panorama skybox with matching fog;
`Menu.unity` keeps Unity's default skybox with plain background color. If you
want the menu to share the sunrise look, assign the same
Render Settings skybox to the Menu scene (Render → Lighting / scene
ambient & fog settings) — a one-click editor change.

## DOTween is pinned to 1.2.340

The bundled DOTween predates several convenience APIs (`SetAs(id)`,
`DOKill(id)`, `AsyncWaitable()`). The code in this repo is written against the
1.2.340 surface (see the compatibility table in [FX.md](FX.md)). If you
upgrade DOTween, the current code keeps working, but you may then simplify
toward the newer APIs.

## Physics model is transform-driven

The player's `Body` carries a **kinematic** `Rigidbody` + non-trigger
`SphereCollider` purely so trigger events (`OnTriggerEnter`) fire against the
blocker/coin triggers. Movement is 100 % transform/tween based — there is no
ground-collision or gravity simulation. If you add real physics later, keep
the body kinematic or move all movement into the Rigidbody.

## Custom tags required

`Coin` and `Obstacle` are **custom tags** (the coin prefab is tagged `Coin`;
the blocker prefabs are tagged `Obstacle`). Do not remove them from
*Project Settings → Tags and Layer Settings* or collision detection silently
stops working.

## Skybox pack size

See [PERFORMANCE.md](PERFORMANCE.md) — the 1.4 GB `Fantasy Skybox FREE` pack
dominates repo size; only one material from it is used by the game.

## Audio is placeholder-grade

All music/SFX in `_Audio/` are simple synthesized tones (project-owned, no
license concerns). Replace the WAV files or reassign the `AudioDirector`
fields to drop in real audio — no code changes needed
(see [AUDIO.md](AUDIO.md)).

## Verified constraints (don't regress)

These were checked carefully because a wrong value here breaks gameplay:

- Spawn marker `z = +150`, destroy marker `z = -5` **relative to the player**
  (both are children of the Player GO).
- Camera far clip = 1000, FOV 60; Exp2 fog density 0.01 (hides the 150 m spawn
  line at ≈90 %).
- Blocker colliders are **triggers**; coin collider is a **trigger**
  (`SphereCollider`, center `y 0.6`, radius `0.34`); player body collider is a
  non-trigger sphere (`center y 0.4`, radius `0.25`).
- `isDead` parameter in `Character.controller` is a **bool** (`m_Type: 2`).
- Build Settings order: `Menu`, `Gameplay` (the boot scene is `Menu`).
