using UnityEngine;

/// <summary>
/// Sistema de arma para enemigos.
/// Maneja disparo de proyectiles con cooldown.
/// Patrón: Template Method (estructura común para diferentes armas)
/// </summary>
public class EnemyWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f; // Disparos por segundo
    [SerializeField] private float projectileDamage = 15f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private LayerMask hitLayers;
    
    [Header("Accuracy")]
    [SerializeField] private float spreadAngle = 5f; // Dispersión en grados
    [SerializeField] private bool useLeading = true; // Predicción de movimiento del jugador
    [SerializeField] private float leadingPredictionTime = 0.5f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject muzzleFlashEffect;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTrigger = "Shoot";
    
    private float nextFireTime = 0f;
    
    public bool CanFire => Time.time >= nextFireTime;
    
    private void Awake()
    {
        if (firePoint == null)
        {
            // Crear firePoint si no existe
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = Vector3.forward;
            firePoint = fp.transform;
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }
    }
    
    /// <summary>
    /// Dispara un proyectil hacia el objetivo.
    /// </summary>
    public bool TryShoot(Transform target)
    {
        if (!CanFire || target == null || projectilePrefab == null)
            return false;
        
        // Calcular dirección del disparo
        Vector3 shootDirection = CalculateShootDirection(target);
        
        // Aplicar dispersión
        shootDirection = ApplySpread(shootDirection);
        
        // Crear proyectil
        SpawnProjectile(shootDirection);
        
        // Efectos
        PlayFireEffects();
        
        // Trigger animación
        if (animator != null && !string.IsNullOrEmpty(shootTrigger))
        {
            animator.SetTrigger(shootTrigger);
        }
        
        // Actualizar cooldown
        nextFireTime = Time.time + (1f / fireRate);
        
        return true;
    }
    
    /// <summary>
    /// Calcula la dirección del disparo con predicción opcional.
    /// </summary>
    private Vector3 CalculateShootDirection(Transform target)
    {
        Vector3 targetPosition = target.position;
        
        // Predicción de movimiento del jugador
        if (useLeading)
        {
            // Intentar obtener velocity del jugador
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            CharacterController targetCC = target.GetComponent<CharacterController>();
            
            Vector3 targetVelocity = Vector3.zero;
            
            if (targetRb != null)
            {
                targetVelocity = targetRb.velocity;
            }
            else if (targetCC != null)
            {
                targetVelocity = targetCC.velocity;
            }
            
            // Calcular posición predicha
            targetPosition += targetVelocity * leadingPredictionTime;
        }
        
        // Apuntar ligeramente hacia arriba del centro del objetivo (torso)
        targetPosition += Vector3.up * 1f;
        
        Vector3 direction = targetPosition - firePoint.position;
        return direction.normalized;
    }
    
    /// <summary>
    /// Aplica dispersión al disparo.
    /// </summary>
    private Vector3 ApplySpread(Vector3 direction)
    {
        if (spreadAngle <= 0f)
            return direction;
        
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        
        Quaternion spread = Quaternion.Euler(spreadX, spreadY, 0f);
        return spread * direction;
    }
    
    /// <summary>
    /// Spawnea el proyectil.
    /// </summary>
    private void SpawnProjectile(Vector3 direction)
    {
        GameObject projectileObj = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );
        
        // Inicializar el proyectil
        EnemyProjectile projectile = projectileObj.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, projectileDamage, projectileSpeed, hitLayers);
        }
    }
    
    /// <summary>
    /// Reproduce efectos visuales y de audio.
    /// </summary>
    private void PlayFireEffects()
    {
        // Muzzle flash
        if (muzzleFlashEffect != null)
        {
            GameObject flash = Instantiate(muzzleFlashEffect, firePoint.position, firePoint.rotation);
            Destroy(flash, 1f);
        }
        
        // Sonido
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }
    
    /// <summary>
    /// Dibuja el alcance del arma en el editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(firePoint.position, firePoint.forward * 10f);
        
        // Visualizar dispersión
        if (spreadAngle > 0f)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Vector3 spread1 = Quaternion.Euler(spreadAngle, spreadAngle, 0) * firePoint.forward;
            Vector3 spread2 = Quaternion.Euler(-spreadAngle, -spreadAngle, 0) * firePoint.forward;
            Gizmos.DrawRay(firePoint.position, spread1 * 10f);
            Gizmos.DrawRay(firePoint.position, spread2 * 10f);
        }
    }
}