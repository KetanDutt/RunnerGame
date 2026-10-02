# Audio

All audio is driven by **`AudioDirector`** — one instance per scene, attached to
the `AudioDirector` GameObject in `Menu.unity` and `Gameplay.unity`.

## Design

- **Music**: a single looping `AudioSource` (created in `Awake`), volume 0 →
  lerps up to the target each frame using `unscaledDeltaTime`, so fades work
  even while the game is paused.
- **SFX**: a round-robin **pool of 12 one-shot sources** — rapid coin pickups
  never cut each other off. Each play adds ±4 % random pitch for variety.
- **Ducking**: while paused the music target drops to 35 % (`DuckMusic`).
- **Mute options** persist in `PlayerPrefs` (`RunnerGame.MusicEnabled`,
  `RunnerGame.SfxEnabled`) and are toggled with `M` / `N` during gameplay.

## Clip list

| Role | Clip | Where used |
|---|---|---|
| Music (gameplay) | `_Audio/Music/Music_Gameplay.wav` | Gameplay `AudioDirector.musicLoop` |
| Music (menu) | `_Audio/Music/Music_Menu.wav` | Menu `AudioDirector.musicLoop` |
| Click | `_Audio/SFX/SFX_Click.wav` | All buttons |
| Coin | `_Audio/SFX/SFX_Coin.wav` | Coin pickup |
| Jump | `_Audio/SFX/SFX_Jump.wav` | Jump |
| Slide | `_Audio/SFX/SFX_Slide.wav` | Slide |
| Hit | `_Audio/SFX/SFX_Hit.wav` | Taking damage |
| Game over | `_Audio/SFX/SFX_GameOver.wav` | Death / game over |
| Countdown | `_Audio/SFX/SFX_Countdown.wav` | `3` `2` `1` |
| Go | `_Audio/SFX/SFX_Go.wav` | `GO!` |

All clips are 44.1 kHz mono WAVs, project-owned (synthesized), so there are no
third-party audio licenses.

## Using the API

```csharp
AudioDirector.Instance?.PlayCoin();      // one-shot (safe if null)
AudioDirector.Instance?.DuckMusic(true); // pause ducking
AudioDirector.Instance?.ToggleMusic();   // persisted mute toggle
```

Call sites (all guarded with `?.` since the director is per-scene):

- `GameManager` — countdown beeps, `Go`, game-over sting, music stop, ducking
- `PlayerController` — jump, slide
- `PlayerHealth` — hit
- `GameManager.CollectCoin` — coin
- `GameplayUI` / `MenuUI` — UI clicks

## Adding a new sound

1. Drop the clip into `Assets/_Audio/SFX/` (mono WAV recommended).
2. Add a `[SerializeField] private AudioClip sfxMySound;` field and a
   `PlayMySound()` wrapper in `AudioDirector`.
3. Assign the clip in the inspector on **both** `AudioDirector` objects
   (Menu + Gameplay) if it should play in both scenes.

> The clips are intentionally simple synth tones. Swapping in real music/SFX
> only requires replacing the files (same names) or reassigning the inspector
> fields — no code changes.
