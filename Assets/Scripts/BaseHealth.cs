using System;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public event Action<float, float> OnHealthChanged; //(current, max)
    public event Action OnDestroyed;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Base took {damage} damage. Current health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDestroyed?.Invoke();
        Debug.Log("Base Destroyed! Game Over.");
    }
}
