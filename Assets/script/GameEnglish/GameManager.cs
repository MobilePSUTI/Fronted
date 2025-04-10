using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Word Lists")]
    public List<string> easyWords = new List<string> { "cat", "dog", "sun", "hat", "pen" };
    public List<string> mediumWords = new List<string> { "apple", "house", "water", "light", "music" };
    public List<string> hardWords = new List<string> { "elephant", "computer", "keyboard", "adventure", "mountain" };

    [Header("Game Settings")]
    public float initialTimePerWord = 30f;
    public float difficultyIncreaseInterval = 60f;
    public int maxWordsToLose = 3;
    public const int MAX_COINS = 50; // Максимальное количество очков

    [Header("UI References")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI timerText;

    private int coinsCollected = 0;
    public float gameTimer = 0f;
    public int wordsMissed = 0;
    public int currentDifficulty = 0;
    public bool gameOver = false;

    public int CoinsCollected
    {
        get => coinsCollected;
        set
        {
            coinsCollected = Mathf.Min(value, MAX_COINS); // Не даём превысить 50
            coinsText.text = "Coins: " + coinsCollected;

            if (coinsCollected >= MAX_COINS && !gameOver)
            {
                GameOver(true);
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        MobileKeyboardHandler keyboardHandler = FindObjectOfType<MobileKeyboardHandler>();
        if (keyboardHandler != null)
        {
            keyboardHandler.OpenSystemKeyboard();
        }
#endif
    }

    private void Update()
    {
        if (!gameOver && !PauseManager.Instance.IsGamePaused())
        {
            gameTimer += Time.deltaTime;
            UpdateTimerUI();

            if (gameTimer >= difficultyIncreaseInterval && currentDifficulty < 2)
            {
                currentDifficulty++;
                difficultyIncreaseInterval *= 2;
            }
        }
    }

    public string GetRandomWord()
    {
        List<string> selectedList = easyWords;

        if (currentDifficulty == 1) selectedList = mediumWords;
        else if (currentDifficulty == 2)
        {
            int r = Random.Range(0, 3);
            selectedList = r == 0 ? easyWords : r == 1 ? mediumWords : hardWords;
        }

        return selectedList[Random.Range(0, selectedList.Count)];
    }

    public void AddCoins(int amount)
    {
        CoinsCollected += amount; // Используем свойство с защитой
    }

    public void WordMissed()
    {
        wordsMissed++;
        if (wordsMissed >= maxWordsToLose) GameOver(false);
    }

    private void GameOver(bool isWin)
    {
        gameOver = true;
        Debug.Log(isWin ? "You Win!" : "Game Over! Coins: " + coinsCollected);

        PlayerPrefs.SetInt("FinalCoins", coinsCollected);
        PlayerPrefs.SetFloat("FinalTime", gameTimer);
        PlayerPrefs.SetInt("IsWin", isWin ? 1 : 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameOverEnglish");
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(gameTimer / 60f);
        int seconds = Mathf.FloorToInt(gameTimer % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public int CalculateCoinReward()
    {
        return Random.Range(2, 5);
    }
}