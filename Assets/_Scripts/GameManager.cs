using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI coinText;
    public int countdownDuration = 3;
    public int coinsCollected = 0;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
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
            countdownText.text = countdownValue.ToString();
            yield return new WaitForSeconds(1f);
            countdownValue--;
        }

        countdownText.text = "Go!";
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);

        // Start the game
        StartGame();
    }

    private void StartGame()
    {
        Debug.Log("Game started!");
        // You can put your game start logic here
    }

    public void CoinCollected()
    {
        coinsCollected++;
        coinText.text = "Coins: " + coinsCollected.ToString();
        // You can put additional logic here for coin collection
    }
}
