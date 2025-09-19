using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Tooltip("Reference to the PlayerHealth component controlling the player's life.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Slider UI element that visually represents the player's health.")]
    [SerializeField] private Slider healthSlider;
    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
        }

        if (playerHealth != null && healthSlider != null)
        {
            healthSlider.value = playerHealth.CurrentHealth / playerHealth.MaxHealth;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(float normalizedHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = normalizedHealth;
        }
    }
}
