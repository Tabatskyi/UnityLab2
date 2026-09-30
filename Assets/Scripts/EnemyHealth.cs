using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("Score Settings")]
    [Tooltip("Points awarded to the player when this enemy is destroyed.")]
    [SerializeField] private int scoreValue = 1;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;
    public static event Action<EnemyHealth> OnAnyEnemyDied;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public int ScoreValue
    {
        get => scoreValue;
        set => scoreValue = value;
    }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = newMaxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        OnAnyEnemyDied?.Invoke(this);

        ScoreManager scoreManager = ScoreManager.Instance;
        if (scoreManager == null)
        {
#if UNITY_2023_1_OR_NEWER
            scoreManager = FindFirstObjectByType<ScoreManager>();
#else
            scoreManager = FindObjectOfType<ScoreManager>();
#endif
        }

        if (scoreManager != null)
        {
            scoreManager.AddScore(scoreValue);
        }

        Debug.Log("Enemy Destroyed!");
        Destroy(gameObject);
    }
}