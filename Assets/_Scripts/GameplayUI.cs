using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private Image pausePanel;
    [SerializeField] private Image gameOverPanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button pauseResumeButton;
    [SerializeField] private Button pauseQuitButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button resultRestartButton;
    [SerializeField] private Button resultQuitButton;
    [SerializeField] private TextMeshProUGUI resultScoreText;
    [SerializeField] private TextMeshProUGUI resultCoinText;
    [SerializeField] private TextMeshProUGUI resultTimeText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    private GameManager gameManager;

    void Start()
    {
        pausePanel.gameObject.SetActive(false);
        gameOverPanel.gameObject.SetActive(false);

        pauseButton.onClick.AddListener(PauseGame);
        pauseResumeButton.onClick.AddListener(ResumeGame);
        pauseQuitButton.onClick.AddListener(QuitGame);
        pauseRestartButton.onClick.AddListener(RestartGame);
        resultRestartButton.onClick.AddListener(RestartGame);
        resultQuitButton.onClick.AddListener(QuitGame);

        gameManager = GameManager.instance;
    }

    private void ResumeGame()
    {
        pausePanel.gameObject.SetActive(false);
        gameManager.isPaused = false;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    private void QuitGame()
    {
        SceneManager.LoadScene(0);
    }

    private void PauseGame()
    {
        pausePanel.gameObject.SetActive(true);
        gameManager.isPaused = true;
    }

    public void GameOver(int coinsCollected, float playerScore, float playerTime)
    {
        resultCoinText.text = "Coins Collected : " + coinsCollected;
        resultScoreText.text = "Score : " + (int)playerScore;
        resultTimeText.text = "Time Taken : " + (int)playerTime;
        gameOverPanel.gameObject.SetActive(true);
    }
}
