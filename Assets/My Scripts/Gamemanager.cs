
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField] int targetScore = 10;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject winPanel;

    [Header("Special coins")]
    [SerializeField] Spawner spawner;
    [Tooltip("Shows after collecting red coin")]
    [SerializeField] TMP_Text bonusText;
    [SerializeField] int bonusCoinCount = 50;
    [Tooltip("Points given by red coin")]
    [SerializeField] int redCoinPoints = 0;
    [Tooltip("Win after collecting all required coins")]
    [SerializeField] bool requireBonusToWin = true;
    [Tooltip("0 means random score")]
    [SerializeField] int forceRedAtScore = 0;

    int score;
    int redSpawnScore;
    int bonusCollected;
    bool redSpawned, redCollected, won;

    void Awake() => Instance = this;

    void Start()
    {
        Time.timeScale = 1f;
        redSpawnScore = forceRedAtScore > 0 ? forceRedAtScore : Random.Range(1, 11);
        Debug.Log($"Red coin will appear when the score reaches {redSpawnScore}");

        if (winPanel != null) winPanel.SetActive(false);
        if (bonusText != null) bonusText.gameObject.SetActive(false);
        UpdateUI();
    }

    public void OnCoinCollected(CoinType type, int value)
    {
        switch (type)
        {
            case CoinType.Normal: AddScore(value); break;
            case CoinType.Red: CollectRedCoin(); break;
            case CoinType.Blue: CollectBlueCoin(); break;
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
        TrySpawnRedCoin();
        CheckWin();
    }

    // spawn red coin once
    void TrySpawnRedCoin()
    {
        if (redSpawned || score < redSpawnScore) return;
        redSpawned = true;
        spawner.SpawnRedCoin();
        Debug.Log("Red coin spawned!");
    }

    // red coin starts bonus round
    void CollectRedCoin()
    {
        redCollected = true;
        spawner.SpawnBonusRing(bonusCoinCount);

        if (bonusText != null)
        {
            bonusText.gameObject.SetActive(true);
            UpdateBonusUI();
        }
        if (redCoinPoints > 0) AddScore(redCoinPoints);
        else { UpdateUI(); }
    }

    void CollectBlueCoin()
    {
        bonusCollected++;
        UpdateBonusUI();
        UpdateUI();
        CheckWin();
    }

    void UpdateUI()
    {
        if (scoreText == null) return;
        string s = $"Score: {score} / {targetScore}";
        if (requireBonusToWin && !won && score >= targetScore)
            s += redCollected ? "\nCollect all the BLUE coins!" : "\nFind the RED coin!";
        scoreText.text = s;
    }

    void UpdateBonusUI()
    {
        if (bonusText != null) bonusText.text = $"BonusCoins: {bonusCollected}/{bonusCoinCount}";
    }

    // check if game is completed
    void CheckWin()
    {
        if (won || score < targetScore) return;
        if (requireBonusToWin && !(redCollected && bonusCollected >= bonusCoinCount)) return;
        Win();
    }

    void Win()
    {
        won = true;
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
