using System.Collections.Generic;
using UnityEngine;

namespace RunnerGame
{
    /// <summary>
    /// One instance per scene. Owns the music loop and a small pool of
    /// one-shot effect sources. Persisted preferences control mute state.
    /// Attach to a dedicated "AudioDirector" GameObject in each scene and
    /// assign the clips in the inspector.
    /// </summary>
    public class AudioDirector : MonoBehaviour
    {
        public const string KeyMusicEnabled = "RunnerGame.MusicEnabled";
        public const string KeySfxEnabled = "RunnerGame.SfxEnabled";

        public static AudioDirector Instance { get; private set; }

        [Header("Music")]
        [Tooltip("Looping music track for this scene (leave empty for silence).")]
        [SerializeField] private AudioClip musicLoop;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.55f;

        [Header("Sound effects")]
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.85f;
        [SerializeField] private AudioClip sfxClick;
        [SerializeField] private AudioClip sfxCoin;
        [SerializeField] private AudioClip sfxJump;
        [SerializeField] private AudioClip sfxSlide;
        [SerializeField] private AudioClip sfxHit;
        [SerializeField] private AudioClip sfxGameOver;
        [SerializeField] private AudioClip sfxCountdown;
        [SerializeField] private AudioClip sfxGo;

        [Header("Options")]
        [SerializeField] private int sfxPoolSize = 12;
        [SerializeField] private float sfxPitchRandomness = 0.04f;

        private AudioSource _musicSource;
        private readonly List<AudioSource> _sfxPool = new List<AudioSource>();
        private int _sfxCursor;
        private bool _musicEnabled;
        private bool _sfxEnabled;
        private float _musicTargetVolume;
        private bool _ducked;

        public bool MusicEnabled => _musicEnabled;
        public bool SfxEnabled => _sfxEnabled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _musicEnabled = PlayerPrefs.GetInt(KeyMusicEnabled, 1) == 1;
            _sfxEnabled = PlayerPrefs.GetInt(KeySfxEnabled, 1) == 1;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = 0f;

            for (int i = 0; i < Mathf.Max(1, sfxPoolSize); i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;
                _sfxPool.Add(source);
            }

            _musicTargetVolume = _musicEnabled ? musicVolume : 0f;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            if (musicLoop != null)
            {
                _musicSource.clip = musicLoop;
                _musicSource.Play();
            }
        }

        private void Update()
        {
            // Smooth music volume changes (mute/unmute/duck) without a tween dependency.
            float target = _musicTargetVolume;
            if (_musicSource != null && Mathf.Abs(_musicSource.volume - target) > 0.001f)
            {
                _musicSource.volume = Mathf.MoveTowards(_musicSource.volume, target, Time.unscaledDeltaTime * 1.5f);
            }
        }

        // ---- Music ----

        public void StopMusic()
        {
            _musicTargetVolume = 0f;
        }

        public void ResumeMusic()
        {
            if (_musicSource != null && _musicSource.clip != null)
            {
                _musicTargetVolume = _musicEnabled ? musicVolume : 0f;
                if (!_musicSource.isPlaying)
                {
                    _musicSource.Play();
                }
            }
        }

        /// <summary>Quiet the music while paused, restore it when resumed.</summary>
        public void DuckMusic(bool ducked)
        {
            _ducked = ducked;
            _musicTargetVolume = _musicEnabled ? musicVolume * (ducked ? 0.35f : 1f) : 0f;
        }

        public void ToggleMusic()
        {
            _musicEnabled = !_musicEnabled;
            PlayerPrefs.SetInt(KeyMusicEnabled, _musicEnabled ? 1 : 0);
            _musicTargetVolume = _musicEnabled ? musicVolume * (_ducked ? 0.35f : 1f) : 0f;
        }

        public void ToggleSfx()
        {
            _sfxEnabled = !_sfxEnabled;
            PlayerPrefs.SetInt(KeySfxEnabled, _sfxEnabled ? 1 : 0);
        }

        // ---- Effects ----

        public void PlayClick() => PlaySfx(sfxClick);
        public void PlayCoin() => PlaySfx(sfxCoin);
        public void PlayJump() => PlaySfx(sfxJump);
        public void PlaySlide() => PlaySfx(sfxSlide);
        public void PlayHit() => PlaySfx(sfxHit);
        public void PlayGameOver() => PlaySfx(sfxGameOver);
        public void PlayCountdownBeep() => PlaySfx(sfxCountdown);
        public void PlayGo() => PlaySfx(sfxGo);

        private void PlaySfx(AudioClip clip)
        {
            if (!_sfxEnabled || clip == null || _sfxPool.Count == 0)
            {
                return;
            }

            // Round-robin through the pool so rapid pickups never cut each other off.
            for (int attempt = 0; attempt < _sfxPool.Count; attempt++)
            {
                _sfxCursor = (_sfxCursor + 1) % _sfxPool.Count;
                var source = _sfxPool[_sfxCursor];
                if (source.isPlaying)
                {
                    continue;
                }

                source.pitch = 1f + Random.Range(-sfxPitchRandomness, sfxPitchRandomness);
                source.PlayOneShot(clip);
                return;
            }
        }
    }
}
