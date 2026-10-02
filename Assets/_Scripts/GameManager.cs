using System.Collections;
using UnityEngine;

namespace RunnerGame
{
    /// <summary>
    /// Owns the run state: countdown, pause, score, coins, time, high score
    /// and the transition into game over. One instance lives per gameplay scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public const string HighScoreKey = "RunnerGame.HighScore";

        public static GameManager Instance { get; private set; }

        [Header("Countdown")]
        [SerializeField] private int countdownDuration = 3;

        [Header("UI")]
        [SerializeField] private GameplayUI gameplayUI;

        private bool _gameStarted;
        private bool _paused;
        private bool _over;
        private int _coins;
        private float _score;
        private float _time;

        // ---- State ----
        public bool IsRunning => _gameStarted && !_paused && !_over;
        public bool IsStarted => _gameStarted;
        public bool IsPaused => _paused;
        public bool IsOver => _over;
        public int CoinsCollected => _coins;
        public float PlayerScore => _score;
        public float PlayerTime => _time;
        public int HighScore { get; private set; }
        public bool IsNewRecord { get; private set; }

        /// <summary>Null-safe convenience check for systems that poll every frame.</summary>
        public static bool IsGameRunning()
        {
            return Instance != null && Instance.IsRunning;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
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
            // Make sure we never inherit a paused/slowed time scale from a
            // previous scene (e.g. restarting while the run was paused).
            Time.timeScale = 1f;
            StartCoroutine(CountdownRoutine());
        }

        private IEnumerator CountdownRoutine()
        {
            int value = Mathf.Max(1, countdownDuration);

            while (value > 0)
            {
                if (gameplayUI != null)
                {
                    gameplayUI.ShowCountdown(value.ToString());
                }

                AudioDirector.Instance?.PlayCountdownBeep();
                yield return new WaitForSecondsRealtime(1f);
                value--;
            }

            if (gameplayUI != null)
            {
                gameplayUI.ShowCountdown("GO!");
            }

            AudioDirector.Instance?.PlayGo();
            yield return new WaitForSecondsRealtime(0.7f);

            if (gameplayUI != null)
            {
                gameplayUI.HideCountdown();
            }

            _gameStarted = true;
            GameEvents.RaiseGameStarted();
        }

        private void Update()
        {
            if (IsRunning)
            {
                _time += Time.deltaTime;
            }
        }

        // ---- Scoring ----

        /// <summary>Called by the player controller once per frame with the distance travelled this frame.</summary>
        public void AddScore(float delta)
        {
            if (!_gameStarted || _over)
            {
                return;
            }

            _score += delta;
        }

        /// <summary>Called when the player collects a coin.</summary>
        public void CollectCoin(Vector3 coinWorldPosition)
        {
            if (!IsRunning)
            {
                return;
            }

            _coins++;
            AudioDirector.Instance?.PlayCoin();
            ParticleFX.Instance?.BurstCoin(coinWorldPosition);
            GameEvents.RaiseCoinCollected(coinWorldPosition);
        }

        // ---- Pause ----

        public void SetPaused(bool paused)
        {
            if (_over || !_gameStarted || _paused == paused)
            {
                return;
            }

            _paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            AudioDirector.Instance?.DuckMusic(paused);
            GameEvents.RaisePauseChanged(paused);
        }

        // ---- Game over ----

        public void GameOver()
        {
            if (_over)
            {
                return;
            }

            _over = true;
            Time.timeScale = 1f;

            int finalScore = Mathf.FloorToInt(_score);
            IsNewRecord = finalScore > HighScore && finalScore > 0;
            if (IsNewRecord)
            {
                HighScore = finalScore;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }

            AudioDirector.Instance?.StopMusic();
            AudioDirector.Instance?.PlayGameOver();

            GameEvents.RaiseGameOver(finalScore);
            if (gameplayUI != null)
            {
                gameplayUI.ShowGameOver(_coins, finalScore, Mathf.FloorToInt(_time), HighScore, IsNewRecord);
            }
        }
    }
}
