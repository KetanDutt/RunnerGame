using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private bool _gameStarted = false;
    public bool isPaused = false;
    public bool isOver = false;

    [SerializeField] private int countdownDuration = 3;
    private int coinsCollected = 0;
    private float playerScore = 0;
    private float playerTime = 0;

    public static event Action onGameStart;

    [SerializeField] private GameplayUI gameplayUI;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCountdown();
    }

    private void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        int countdownValue = countdownDuration;

        while (countdownValue > 0)
        {
            gameplayUI.countdownText.text = countdownValue.ToString();
            yield return new WaitForSecondsRealtime(1f);
            countdownValue--;
        }

        gameplayUI.countdownText.text = "Go!";
        yield return new WaitForSecondsRealtime(1f);
        gameplayUI.countdownText.gameObject.SetActive(false);

        // Start the game
        StartGame();
    }

    private void StartGame()
    {
        _gameStarted = true;
        onGameStart?.Invoke();
    }

    public void CoinCollected()
    {
        coinsCollected++;
        gameplayUI.coinText.text = $"Coins: {coinsCollected}";
    }

    public bool isRunning()
    {
        return _gameStarted && !isPaused && !isOver;
    }

    private void Update()
    {
        gameplayUI.scoreText.text = $"Score: {(int)playerScore}";
        gameplayUI.timeText.text = $"Time: {(int)playerTime}";
    }

    public void GameOver()
    {
        isOver = true;
        gameplayUI.GameOver(coinsCollected, playerScore, playerTime);
    }

    public int CoinsCollected => coinsCollected;

    public float PlayerScore
    {
        get => playerScore;
        set => playerScore = value;
    }

    public float PlayerTime
    {
        get => playerTime;
        set => playerTime = value;
    }
}
