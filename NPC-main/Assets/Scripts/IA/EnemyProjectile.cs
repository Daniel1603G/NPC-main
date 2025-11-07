using UnityEngine;

/// <summary>
/// Proyectil físico disparado por enemigos.
/// Se mueve en línea recta y aplica daño al impactar.
/// Patrón: Strategy (diferentes tipos de proyectiles pueden heredar)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask hitLayers;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Light projectileLight;
    [SerializeField] private Color projectileColor = Color.red;
    
    [Header("Audio")]
    [SerializeField] private AudioClip impactSound;
    
    private Rigidbody rb;
    private bool hasHit = false;
    private Vector3 previousPosition;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Configurar Rigidbody
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // Configurar trail si existe
        if (trail != null)
        {
            trail.startColor = projectileColor;
            trail.endColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, 0f);
        }
        
        // Configurar luz si existe
        if (projectileLight != null)
        {
            projectileLight.color = projectileColor;
        }
        
        previousPosition = transform.position;
    }
    
 
    public void Initialize(Vector3 direction, float damage, float speed, LayerMask hitLayers)
    {
        this.damage = damage;
        this.speed = speed;
        this.hitLayers = hitLayers;
        
        // Aplicar velocidad
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }
        
        // Orientar el proyectil
        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }
        
        // Destruir después del lifetime
        Destroy(gameObject, lifetime);
    }
    
    private void Update()
    {
     
        if (hasHit) return;
        
        Vector3 currentPosition = transform.position;
        Vector3 direction = currentPosition - previousPosition;
        float distance = direction.magnitude;
        
        if (distance > 0.01f)
        {
            if (Physics.Raycast(previousPosition, direction.normalized, out RaycastHit hit, distance, hitLayers))
            {
                OnHit(hit.point, hit.normal, hit.collider);
            }
        }
        
        previousPosition = currentPosition;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        
        // Llamar directamente con los datos del collision
        OnHit(collision.contacts[0].point, collision.contacts[0].normal, collision.collider);
    }
    
   
    private void OnHit(Vector3 hitPoint, Vector3 hitNormal, Collider hitCollider)
    {
        if (hasHit) return;
        hasHit = true;
        
        // Detener el proyectil
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Aplicar daño
        ApplyDamage(hitCollider.gameObject);
        
        // Efecto de impacto
        CreateImpactEffect(hitPoint, hitNormal);
        
        // Sonido de impacto
        PlayImpactSound(hitPoint);
        
        // Destruir el proyectil
        Destroy(gameObject, 0.05f);
    }
    

    private void ApplyDamage(GameObject target)
    {
        // Calcular origen del daño (posición del proyectil cuando se disparó)
        Vector3 damageOrigin = transform.position - transform.forward * 2f;
    
        // Intentar aplicar daño al jugador
        var playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"Proyectil impactó al jugador por {damage} de daño");
            return;
        }
    
        // Si impacta a otro enemigo (fuego amigo)
        var enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // Pasar la posición de origen del daño
            enemyHealth.TakeDamage(damage * 0.5f, damageOrigin);
            Debug.Log($"Proyectil impactó a {target.name}");
        }
    }
    
    /// <summary>
    /// Crea el efecto visual de impacto.
    /// </summary>
    private void CreateImpactEffect(Vector3 position, Vector3 normal)
    {
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, position, Quaternion.LookRotation(normal));
            Destroy(effect, 2f);
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de impacto.
    /// </summary>
    private void PlayImpactSound(Vector3 position)
    {
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, position);
        }
    }
    
    private void OnDrawGizmos()
    {
        // Visualizar dirección del proyectil
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}