using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunnerGame
{
    /// <summary>
    /// All in-game UI: HUD counters, countdown, pause panel and the
    /// game over results panel. Panels and the countdown are animated
    /// with DOTween.
    /// </summary>
    public class GameplayUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private Image pausePanel;
        [SerializeField] private Image gameOverPanel;

        [Header("Pause buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button pauseResumeButton;
        [SerializeField] private Button pauseQuitButton;
        [SerializeField] private Button pauseRestartButton;

        [Header("Result buttons")]
        [SerializeField] private Button resultRestartButton;
        [SerializeField] private Button resultQuitButton;

        [Header("Result texts")]
        [SerializeField] private TextMeshProUGUI resultScoreText;
        [SerializeField] private TextMeshProUGUI resultCoinText;
        [SerializeField] private TextMeshProUGUI resultTimeText;
        [SerializeField] private TextMeshProUGUI bestText;
        [SerializeField] private Color newRecordColor = new Color(1f, 0.84f, 0.25f);

        [Header("HUD texts")]
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;

        private CanvasGroup _pauseGroup;
        private CanvasGroup _overGroup;
        // -1 so the first frame always writes the (correct) zero values over
        // whatever placeholder text the scene was saved with.
        private int _lastScore = -1;
        private int _lastCoins = -1;
        private int _lastTime = -1;

        private void Start()
        {
            if (pausePanel != null)
            {
                pausePanel.gameObject.SetActive(false);
                _pauseGroup = GetOrCreateCanvasGroup(pausePanel);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.gameObject.SetActive(false);
                _overGroup = GetOrCreateCanvasGroup(gameOverPanel);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(PauseGame);
            }

            if (pauseResumeButton != null)
            {
                pauseResumeButton.onClick.AddListener(ResumeGame);
            }

            if (pauseQuitButton != null)
            {
                pauseQuitButton.onClick.AddListener(QuitGame);
            }

            if (pauseRestartButton != null)
            {
                pauseRestartButton.onClick.AddListener(RestartGame);
            }

            if (resultRestartButton != null)
            {
                resultRestartButton.onClick.AddListener(RestartGame);
            }

            if (resultQuitButton != null)
            {
                resultQuitButton.onClick.AddListener(QuitGame);
            }
        }

        private void Update()
        {
            var game = GameManager.Instance;
            if (game == null)
            {
                return;
            }

            UpdateHud(game);

            // Keyboard: Esc / P toggles pause, M / N toggle music and SFX.
            if (game.IsStarted && !game.IsOver)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                {
                    TogglePause();
                }
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                AudioDirector.Instance?.ToggleMusic();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                AudioDirector.Instance?.ToggleSfx();
            }
        }

        private void UpdateHud(GameManager game)
        {
            if (game.IsOver)
            {
                return;
            }

            int score = Mathf.FloorToInt(game.PlayerScore);
            int coins = game.CoinsCollected;
            int time = Mathf.FloorToInt(game.PlayerTime);

            if (scoreText != null && score != _lastScore)
            {
                _lastScore = score;
                scoreText.text = "Score: " + score;
            }

            if (coinText != null && coins != _lastCoins)
            {
                _lastCoins = coins;
                coinText.text = "Coins: " + coins;
            }

            if (timeText != null && time != _lastTime)
            {
                _lastTime = time;
                timeText.text = "Time: " + time;
            }
        }

        // ---- Countdown ----

        public void ShowCountdown(string value)
        {
            if (countdownText == null)
            {
                return;
            }

            countdownText.gameObject.SetActive(true);
            countdownText.text = value;

            var rect = countdownText.rectTransform;
            bool isGo = value == "GO!";

            countdownText.DOKill();
            rect.DOKill();
            countdownText.color = new Color(1f, 1f, 1f, 1f);
            rect.localScale = Vector3.one * (isGo ? 1.7f : 1.3f);

            countdownText.DOFade(1f, 0.1f).SetUpdate(true);
            rect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void HideCountdown()
        {
            if (countdownText == null)
            {
                return;
            }

            countdownText.DOKill();
            countdownText.DOFade(0f, 0.25f).SetUpdate(true).OnComplete(() =>
            {
                if (countdownText != null && countdownText.gameObject.activeInHierarchy)
                {
                    countdownText.gameObject.SetActive(false);
                }
            });
        }

        // ---- Pause ----

        private void TogglePause()
        {
            var game = GameManager.Instance;
            if (game == null)
            {
                return;
            }

            if (game.IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            AudioDirector.Instance?.PlayClick();
            GameManager.Instance?.SetPaused(true);
            ShowPanel(pausePanel, _pauseGroup);
        }

        public void ResumeGame()
        {
            AudioDirector.Instance?.PlayClick();
            HidePanel(pausePanel, _pauseGroup);
            GameManager.Instance?.SetPaused(false);
        }

        public void RestartGame()
        {
            AudioDirector.Instance?.PlayClick();
            SceneFader.Get().FadeAndLoad(SceneNames.Gameplay);
        }

        public void QuitGame()
        {
            AudioDirector.Instance?.PlayClick();
            SceneFader.Get().FadeAndLoad(SceneNames.Menu);
        }

        // ---- Game over ----

        public void ShowGameOver(int coins, int score, int time, int highScore, bool isNewRecord)
        {
            if (resultScoreText != null)
            {
                resultScoreText.text = "Score : " + score;
            }

            if (resultCoinText != null)
            {
                resultCoinText.text = "Coins Collected : " + coins;
            }

            if (resultTimeText != null)
            {
                resultTimeText.text = "Time Taken : " + time + "s";
            }

            if (bestText != null)
            {
                bestText.text = "Best : " + highScore + (isNewRecord ? "    * NEW RECORD *" : string.Empty);
                bestText.color = isNewRecord ? newRecordColor : Color.white;
            }

            StartCoroutine(ShowGameOverRoutine(isNewRecord));
        }

        private IEnumerator ShowGameOverRoutine(bool isNewRecord)
        {
            // Let the death animation play before the panel pops in.
            yield return new WaitForSecondsRealtime(1.0f);

            if (isNewRecord)
            {
                ParticleFX.Instance?.BurstConfetti();
            }

            ShowPanel(gameOverPanel, _overGroup);
        }

        // ---- Panel helpers ----

        private static CanvasGroup GetOrCreateCanvasGroup(Image panelImage)
        {
            return panelImage.GetComponentInParent<CanvasGroup>() ?? panelImage.gameObject.AddComponent<CanvasGroup>();
        }

        private void ShowPanel(Image panel, CanvasGroup group)
        {
            if (panel == null)
            {
                return;
            }

            panel.gameObject.SetActive(true);
            if (group != null)
            {
                group.DOKill();
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() => group.blocksRaycasts = true);
            }
        }

        private void HidePanel(Image panel, CanvasGroup group)
        {
            if (panel == null)
            {
                return;
            }

            if (group != null)
            {
                group.DOKill();
                group.blocksRaycasts = false;
                group.DOFade(0f, 0.25f).SetUpdate(true).OnComplete(() =>
                {
                    if (panel != null && panel.gameObject.activeInHierarchy)
                    {
                        panel.gameObject.SetActive(false);
                    }
                });
            }
            else
            {
                panel.gameObject.SetActive(false);
            }
        }
    }
}
