using System;
using UnityEngine;

using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 100f;

    [SerializeField]
    private float currentHealth;
    public event Action<float> OnHealthChanged;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = Mathf.Clamp(maxHealth, 1f, float.MaxValue);

        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) return;
        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    protected virtual void Die()
    {
        Debug.Log("Player has died.");
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }
}