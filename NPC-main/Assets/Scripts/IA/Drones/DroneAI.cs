using UnityEngine;

/// <summary>
/// IA para drones kamikaze que usan flocking.
/// Patrullan en grupo, detectan al jugador y se inmolan.
/// </summary>
[RequireComponent(typeof(Boid3D))]
[RequireComponent(typeof(SphereCollider))]
public class DroneAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LineOfSight lineOfSight;
    [SerializeField] private EnemyHealth enemyHealth;
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float kamikazeActivationRange = 10f;
    
    [Header("Kamikaze Settings")]
    [SerializeField] private float kamikazeSpeed = 15f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    
    [Header("Audio")]
    [SerializeField] private AudioSource droneAudioSource;
    [SerializeField] private AudioClip alertSound;
    
    private Boid3D boid;
    private IState currentState;
    private PatrolDroneState patrolState;
    private ChaseDroneState chaseState;
    private KamikazeState kamikazeState;
    
    // Properties
    public Transform Player => player;
    public float DetectionRange => detectionRange;
    public float KamikazeActivationRange => kamikazeActivationRange;
    public float KamikazeSpeed => kamikazeSpeed;
    public Boid3D Boid => boid;
    
    public PatrolDroneState PatrolStateInstance => patrolState;
    public ChaseDroneState ChaseStateInstance => chaseState;
    public KamikazeState KamikazeStateInstance => kamikazeState;
    
    private void Awake()
    {
        boid = GetComponent<Boid3D>();
        
        if (lineOfSight == null)
            lineOfSight = GetComponent<LineOfSight>();
        
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
        
        if (droneAudioSource == null)
            droneAudioSource = GetComponent<AudioSource>();
        
        // Buscar jugador si no está asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // Inicializar estados
        patrolState = new PatrolDroneState(this);
        chaseState = new ChaseDroneState(this);
        kamikazeState = new KamikazeState(this);
        
        // Conectar evento de muerte
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnDroneDeath;
        }
    }
    
    private void Start()
    {
        ChangeState(patrolState);
    }
    
    private void Update()
    {
        currentState?.Execute();
    }
    
    public void ChangeState(IState newState)
    {
        if (currentState == newState) return;
        
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
    
    public bool CanSeePlayer()
    {
        if (player == null) return false;
        
        if (lineOfSight != null)
            return lineOfSight.CanSeeTarget(player);
        
        // Fallback: detección por distancia
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }
    
    public bool IsInKamikazeRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= kamikazeActivationRange;
    }
    
    /// <summary>
    /// Explota el drone causando daño en área.
    /// </summary>
    public void Explode()
    {
        Debug.Log($"{gameObject.name}: ¡EXPLOSIÓN!");
        
        // Efecto visual
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        // Sonido
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
        
        // Daño en área
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hitCollider in hitColliders)
        {
            // Daño al jugador
            var playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                float damageFalloff = 1f - (distance / explosionRadius);
                float finalDamage = explosionDamage * Mathf.Max(damageFalloff, 0.3f); // Mínimo 30% daño
                
                playerHealth.TakeDamage(finalDamage);
                Debug.Log($"Drone explotó causando {finalDamage} de daño al jugador");
            }
            
            // Daño a otros enemigos (opcional)
            var enemyHealth = hitCollider.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth != this.enemyHealth)
            {
                enemyHealth.TakeDamage(explosionDamage * 0.5f);
            }
        }
        
        // Destruir el drone
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Reproduce sonido de alerta.
    /// </summary>
    public void PlayAlertSound()
    {
        if (droneAudioSource != null && alertSound != null)
        {
            droneAudioSource.PlayOneShot(alertSound);
        }
    }
    
    private void OnDroneDeath()
    {
        // Explotar al morir
        Explode();
    }
    
    private void OnDestroy()
    {
        // Desregistrarse del manager
        if (FlockingManager3D.Instance != null && boid != null)
        {
            FlockingManager3D.Instance.RemoveBoid(boid);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Rango de kamikaze
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, kamikazeActivationRange);
        
        // Radio de explosión
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}