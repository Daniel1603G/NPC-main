using System;
using UnityEngine;

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
    [SerializeField] private GameObject[] dropPrefabs; 
    [SerializeField] private float dropChance = 0.3f;
    
    [Header("Alert System")]
    [SerializeField] private bool enableAlertOnDamage = true;
    
    [Header("Flee Behavior")]
    [SerializeField] private bool enableFleeAtLowHealth = true;
    [SerializeField, Range(0f, 0.5f)] private float fleeHealthThreshold = 0.3f;
    
    private AudioSource audioSource;
    private Renderer[] renderers;
    private Color originalColor;
    private bool isDead = false;
    

    public event Action<float> OnHealthChanged; 
    
    public event Action OnLowHealth;
    public event Action OnDeath;
    
    public event Action<Vector3> OnDamagedFrom; 
    

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

    public void TakeDamage(float damage, Vector3 damageOrigin = default)
    {
        if (isDead || damage <= 0f) return;
    
        float previousHealth = currentHealth;
    
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);
    
        OnHealthChanged?.Invoke(HealthPercent);
    
        // NUEVO: Notificar dirección del ataque
        if (enableAlertOnDamage && damageOrigin != Vector3.zero)
        {
            Vector3 damageDirection = damageOrigin - transform.position;
            OnDamagedFrom?.Invoke(damageDirection.normalized);
        }
    
        // NUEVO: Verificar si entró en zona de salud crítica
        if (enableFleeAtLowHealth && 
            previousHealth / maxHealth > fleeHealthThreshold && 
            currentHealth / maxHealth <= fleeHealthThreshold)
        {
            OnLowHealth?.Invoke();
            Debug.Log($"{gameObject.name}: ¡Salud crítica! HP: {currentHealth}/{maxHealth}");
        }
    
        StartCoroutine(FlashRed());
    
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
    
        Debug.Log($"{gameObject.name} recibió {damage} de daño. Salud: {currentHealth}/{maxHealth}");
    
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    
    /// <summary>
    /// Mata al enemigo.
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        Debug.Log($"{gameObject.name} ha muerto!");
        

        OnDeath?.Invoke();
        
   
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
      
        SpawnDrops();
        
   
        DisableAI();
        
        
        Destroy(gameObject, 2f);
    }
    

    private void DisableAI()
    {
 
        var guardAI = GetComponent<GuardAI>();
        if (guardAI != null)
        {
            guardAI.enabled = false;
        }
        
    
        var runnerAI = GetComponent<RunnerAI>();
        if (runnerAI != null)
        {
            runnerAI.enabled = false;
        }
        
      
        var controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }

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