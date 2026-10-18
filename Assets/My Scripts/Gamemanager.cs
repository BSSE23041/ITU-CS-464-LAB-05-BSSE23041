using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Part 4 of the lecture: ONE owner for the score, the text, the win and the restart.
// Extended with: a random Red coin (score 1-10), a ring of 50 Blue coins, and a hidden bonus text.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField] int targetScore = 10;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject winPanel;

    [Header("Special coins")]
    [SerializeField] Spawner spawner;
    [Tooltip("Starts hidden. Appears when the red coin is collected.")]
    [SerializeField] TMP_Text bonusText;
    [SerializeField] int bonusCoinCount = 50;
    [Tooltip("Points the red coin adds to the normal score (0 = none).")]
    [SerializeField] int redCoinPoints = 0;
    [Tooltip("ON: you only win after score >= target AND red coin AND all blue coins are collected.")]
    [SerializeField] bool requireBonusToWin = true;
    [Tooltip("0 = pick a random score from 1 to 10 each game. Type 2 (for example) to test quickly.")]
    [SerializeField] int forceRedAtScore = 0;

    int score;
    int redSpawnScore;        // the score at which the ONE red coin appears
    int bonusCollected;
    bool redSpawned, redCollected, won;

    void Awake() => Instance = this;

    void Start()
    {
        Time.timeScale = 1f;                                   // safety: never start frozen
        redSpawnScore = forceRedAtScore > 0 ? forceRedAtScore : Random.Range(1, 11);   // 1..10 inclusive
        Debug.Log($"Red coin will appear when the score reaches {redSpawnScore}");

        if (winPanel != null) winPanel.SetActive(false);
        if (bonusText != null) bonusText.gameObject.SetActive(false);   // hidden until needed
        UpdateUI();
    }

    // Every coin reports here (the coin never touches the score itself).
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
        TrySpawnRedCoin();      // TASK 1
        CheckWin();
    }

    // ---------------------------------------------------------------- TASK 1
    void TrySpawnRedCoin()
    {
        if (redSpawned || score < redSpawnScore) return;
        redSpawned = true;                       // the flag guarantees ONLY ONE red coin
        spawner.SpawnRedCoin();
        Debug.Log("Red coin spawned!");
    }

    // ---------------------------------------------------------------- TASK 2 + 3
    void CollectRedCoin()
    {
        redCollected = true;
        spawner.SpawnBonusRing(bonusCoinCount);  // 50 blue coins in a circle around the player

        if (bonusText != null)
        {
            bonusText.gameObject.SetActive(true);   // the hidden text appears
            UpdateBonusUI();                        // "BonusCoins: 0/50"
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

    // ---------------------------------------------------------------- UI
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

    // ---------------------------------------------------------------- win / restart
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
        Time.timeScale = 1f;                                   // timeScale survives a scene load
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}