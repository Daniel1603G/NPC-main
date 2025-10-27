using System;
using UnityEngine;

/// <summary>
/// Sistema de salud para enemigos.
/// Permite que reciban daño de las armas del jugador.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showHealthBar = false;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("Drops")]
    [SerializeField] private GameObject[] dropPrefabs; // Items que suelta al morir
    [SerializeField] private float dropChance = 0.3f;
    
    private AudioSource audioSource;
    private Renderer[] renderers;
    private Color originalColor;
    private bool isDead = false;
    
    // Eventos
    public event Action<float> OnHealthChanged; // Normalizado 0-1
    public event Action OnDeath;
    
    // Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => currentHealth / maxHealth;
    public bool IsDead => isDead;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        renderers = GetComponentsInChildren<Renderer>();
        
        if (renderers.Length > 0)
        {
            originalColor = renderers[0].material.color;
        }
        
        currentHealth = maxHealth;
    }
    
    /// <summary>
    /// Aplica daño al enemigo.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);
        
        // Disparar evento
        OnHealthChanged?.Invoke(HealthPercent);
        
        // Feedback visual
        StartCoroutine(FlashRed());
        
        // Sonido de daño
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        
        Debug.Log($"{gameObject.name} recibió {damage} de daño. Salud: {currentHealth}/{maxHealth}");
        
        // Verificar muerte
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Mata al enemigo.
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        Debug.Log($"{gameObject.name} ha muerto!");
        
        // Disparar evento
        OnDeath?.Invoke();
        
        // Efecto de muerte
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Sonido de muerte
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Drop items
        SpawnDrops();
        
        // Desactivar AI
        DisableAI();
        
        // Destruir después de un delay
        Destroy(gameObject, 2f);
    }
    
    /// <summary>
    /// Desactiva los componentes de IA.
    /// </summary>
    private void DisableAI()
    {
        // Desactivar GuardAI
        var guardAI = GetComponent<GuardAI>();
        if (guardAI != null)
        {
            guardAI.enabled = false;
        }
        
        // Desactivar RunnerAI
        var runnerAI = GetComponent<RunnerAI>();
        if (runnerAI != null)
        {
            runnerAI.enabled = false;
        }
        
        // Desactivar CharacterController
        var controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }
    
    /// <summary>
    /// Spawnea items al morir.
    /// </summary>
    private void SpawnDrops()
    {
        if (dropPrefabs == null || dropPrefabs.Length == 0) return;
        
        if (UnityEngine.Random.value <= dropChance)
        {
            GameObject randomDrop = dropPrefabs[UnityEngine.Random.Range(0, dropPrefabs.Length)];
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
            Instantiate(randomDrop, dropPosition, Quaternion.identity);
        }
    }
    
    /// <summary>
    /// Flash rojo cuando recibe daño.
    /// </summary>
    private System.Collections.IEnumerator FlashRed()
    {
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
        }
        
        yield return new WaitForSeconds(0.1f);
        
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material.color = originalColor;
            }
        }
    }
    
    /// <summary>
    /// Cura al enemigo (útil para testing).
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(HealthPercent);
    }
    
    // Debug
    private void OnDrawGizmosSelected()
    {
        if (isDead)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.3f);
        }
    }
}