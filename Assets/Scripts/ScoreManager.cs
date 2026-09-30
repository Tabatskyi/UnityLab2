using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Display Settings")]
    [SerializeField] private string prefix = "Score: ";

    [Header("Score Settings")]
    [SerializeField] private int scoreMultiplier = 10;

    public int CurrentScore { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>() ?? GetComponentInChildren<TextMeshProUGUI>();
        }
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
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>() ?? GetComponentInChildren<TextMeshProUGUI>();
        }

        UpdateScoreDisplay();
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        CurrentScore += amount * scoreMultiplier;
        UpdateScoreDisplay();
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{prefix}{CurrentScore}";
        }
    }
}