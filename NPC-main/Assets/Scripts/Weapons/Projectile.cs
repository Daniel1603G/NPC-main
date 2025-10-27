using UnityEngine;

/// <summary>
/// Proyectil físico que explota al impactar.
/// Usado por armas explosivas.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private float damage;
    private float explosionRadius;
    private float speed;
    private LayerMask hitLayers;
    private Rigidbody rb;
    private bool hasExploded = false;
    
    public GameObject ImpactEffect { get; set; }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    /// <summary>
    /// Inicializa el proyectil con sus parámetros.
    /// </summary>
    public void Initialize(float damage, float explosionRadius, float speed, LayerMask hitLayers)
    {
        this.damage = damage;
        this.explosionRadius = explosionRadius;
        this.speed = speed;
        this.hitLayers = hitLayers;
        
        // Aplicar velocidad inicial
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        
        // Destruir después de un tiempo por seguridad
        Destroy(gameObject, 10f);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        
        Explode(collision.contacts[0].point);
    }
    
    /// <summary>
    /// Ejecuta la explosión.
    /// </summary>
    private void Explode(Vector3 explosionPoint)
    {
        hasExploded = true;
        
        // Crear efecto visual
        if (ImpactEffect != null)
        {
            GameObject effect = Instantiate(ImpactEffect, explosionPoint, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        // Detectar todos los objetos en el radio de explosión
        Collider[] hitColliders = Physics.OverlapSphere(explosionPoint, explosionRadius, hitLayers);
        
        foreach (Collider hitCollider in hitColliders)
        {
            // Calcular distancia para daño por falloff
            float distance = Vector3.Distance(explosionPoint, hitCollider.transform.position);
            float damageFalloff = 1f - (distance / explosionRadius);
            float finalDamage = damage * damageFalloff;
            
            // Aplicar daño
            var playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(finalDamage);
            }
            
            // Aplicar fuerza si tiene Rigidbody
            Rigidbody targetRb = hitCollider.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 explosionDir = (hitCollider.transform.position - explosionPoint).normalized;
                targetRb.AddForce(explosionDir * 500f * damageFalloff, ForceMode.Impulse);
            }
        }
        
        // Destruir el proyectil
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualizar radio de explosión en el editor
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}