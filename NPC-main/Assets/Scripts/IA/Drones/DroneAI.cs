using UnityEngine;


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
    [SerializeField] private float attackRange = 12f; // Rango para aumentar velocidad
    
    [Header("Patrol Area")]
    [SerializeField] private Vector3 patrolCenter = Vector3.zero;
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float changeTargetInterval = 5f; // Cambiar objetivo cada 5s
    
    [Header("Speed Settings")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 6f; // Más rápido al perseguir
    [SerializeField] private float kamikazeSpeed = 10f; // Muy rápido al atacar
    
    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    
    [Header("Audio")]
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private AudioSource alertAudioSource;
    [SerializeField] private AudioClip engineIdleClip;
    [SerializeField] private AudioClip engineChaseClip;
    [SerializeField] private AudioClip kamikazeAlarmClip;
    
    [Header("Visuals")]
    [SerializeField] private Light warningLight;
    [SerializeField] private Renderer droneRenderer;
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color alertColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    
    private Boid3D boid;
    private IState currentState;
    private PatrolDroneState patrolState;
    private ChaseDroneState chaseState;
    private AttackDroneState attackState;
    
    private Vector3 currentPatrolTarget;
    private float lastTargetChangeTime;
    private Material droneMaterial;
    
    // Properties
    public Transform Player => player;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float KamikazeSpeed => kamikazeSpeed;
    public Boid3D Boid => boid;
    public Vector3 PatrolCenter => patrolCenter;
    public float PatrolRadius => patrolRadius;
    
    public PatrolDroneState PatrolStateInstance => patrolState;
    public ChaseDroneState ChaseStateInstance => chaseState;
    public AttackDroneState AttackStateInstance => attackState;
    
    private void Awake()
    {
        boid = GetComponent<Boid3D>();
        
        if (lineOfSight == null)
            lineOfSight = GetComponent<LineOfSight>();
        
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
        
        SetupAudio();
        SetupVisuals();
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // Inicializar estados
        patrolState = new PatrolDroneState(this);
        chaseState = new ChaseDroneState(this);
        attackState = new AttackDroneState(this);
        
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnDroneDeath;
        }
        
        // Patrol center por defecto es la posición inicial
        if (patrolCenter == Vector3.zero)
        {
            patrolCenter = transform.position;
        }
    }
    
    private void SetupAudio()
    {
        if (engineAudioSource == null)
        {
            GameObject engineObj = new GameObject("EngineAudio");
            engineObj.transform.SetParent(transform);
            engineAudioSource = engineObj.AddComponent<AudioSource>();
        }
        
        if (alertAudioSource == null)
        {
            GameObject alertObj = new GameObject("AlertAudio");
            alertObj.transform.SetParent(transform);
            alertAudioSource = alertObj.AddComponent<AudioSource>();
        }
        
        engineAudioSource.loop = true;
        engineAudioSource.spatialBlend = 1f;
        engineAudioSource.minDistance = 5f;
        engineAudioSource.maxDistance = 30f;
        engineAudioSource.volume = 0.5f;
        
        alertAudioSource.loop = false;
        alertAudioSource.spatialBlend = 1f;
        alertAudioSource.minDistance = 5f;
        alertAudioSource.maxDistance = 40f;
        alertAudioSource.volume = 0.8f;
    }
    
    private void SetupVisuals()
    {
        if (warningLight == null)
        {
            GameObject lightObj = new GameObject("WarningLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            warningLight = lightObj.AddComponent<Light>();
            warningLight.type = LightType.Point;
            warningLight.range = 5f;
            warningLight.intensity = 1f;
            warningLight.color = normalColor;
        }
        
        if (droneRenderer != null)
        {
            droneMaterial = droneRenderer.material;
            SetDroneColor(normalColor);
        }
    }
    
    private void Start()
    {
        ResetDrone();
    }
    
    public void ResetDrone()
    {
        GenerateRandomPatrolTarget();
        ChangeState(patrolState);
        StartEngineSound();
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
        
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }
    
    public bool IsInAttackRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= attackRange;
    }
    
    /// <summary>
    /// Genera un objetivo aleatorio dentro del área de patrol.
    /// </summary>
    public void GenerateRandomPatrolTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        float randomHeight = Random.Range(boid.GetComponent<Boid3D>().minFlightHeight, 
                                         boid.GetComponent<Boid3D>().maxFlightHeight);
        
        currentPatrolTarget = patrolCenter + new Vector3(randomCircle.x, randomHeight, randomCircle.y);
        lastTargetChangeTime = Time.time;
    }
    
    public Vector3 GetCurrentPatrolTarget()
    {
        // Cambiar objetivo periódicamente
        if (Time.time - lastTargetChangeTime > changeTargetInterval)
        {
            GenerateRandomPatrolTarget();
        }
        
        return currentPatrolTarget;
    }
    
    // === AUDIO & VISUALS ===
    
    public void StartEngineSound()
    {
        if (engineAudioSource != null && engineIdleClip != null && !engineAudioSource.isPlaying)
        {
            engineAudioSource.clip = engineIdleClip;
            engineAudioSource.pitch = 1f;
            engineAudioSource.Play();
        }
    }
    
    public void SetPatrolMode()
    {
        SetDroneColor(normalColor);
        
        if (warningLight != null)
        {
            warningLight.color = normalColor;
            warningLight.intensity = 1f;
        }
        
        if (engineAudioSource != null)
        {
            engineAudioSource.pitch = 1f;
        }
        
        StopAlarm();
    }
    
    public void SetChaseMode()
    {
        SetDroneColor(alertColor);
        
        if (warningLight != null)
        {
            warningLight.color = alertColor;
            warningLight.intensity = 2f;
        }
        
        if (engineAudioSource != null && engineChaseClip != null)
        {
            engineAudioSource.clip = engineChaseClip;
            if (!engineAudioSource.isPlaying)
                engineAudioSource.Play();
            engineAudioSource.pitch = 1.2f;
        }
    }
    
    public void SetAttackMode()
    {
        SetDroneColor(dangerColor);
        
        if (warningLight != null)
        {
            warningLight.color = dangerColor;
            warningLight.intensity = 5f;
        }
        
        if (engineAudioSource != null)
        {
            engineAudioSource.pitch = 1.5f;
        }
        
        // Alarma continua
        if (alertAudioSource != null && kamikazeAlarmClip != null && !alertAudioSource.isPlaying)
        {
            alertAudioSource.clip = kamikazeAlarmClip;
            alertAudioSource.loop = true;
            alertAudioSource.Play();
        }
    }
    
    private void StopAlarm()
    {
        if (alertAudioSource != null && alertAudioSource.isPlaying)
        {
            alertAudioSource.Stop();
            alertAudioSource.loop = false;
        }
    }
    
    private void SetDroneColor(Color color)
    {
        if (droneMaterial != null)
        {
            droneMaterial.SetColor("_Color", color);
            
            if (droneMaterial.HasProperty("_EmissionColor"))
            {
                droneMaterial.EnableKeyword("_EMISSION");
                droneMaterial.SetColor("_EmissionColor", color * 0.5f);
            }
        }
    }
    
    // === COLLISION & EXPLOSION ===
    
    private void OnCollisionEnter(Collision collision)
    {
        // Explotar SIEMPRE al tocar al jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name}: ¡Contacto con jugador! EXPLOSIÓN");
            Explode();
        }
    }
    
    public void Explode()
    {
        // Efecto visual
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        // Sonido
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 1f);
        }
        
        // Daño en área
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hitCollider in hitColliders)
        {
            var playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                float damageFalloff = 1f - (distance / explosionRadius);
                float finalDamage = explosionDamage * Mathf.Max(damageFalloff, 0.5f);
                
                playerHealth.TakeDamage(finalDamage);
                Debug.Log($"Drone causó {finalDamage} de daño por explosión");
            }
            
            var enemyHealth = hitCollider.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth != this.enemyHealth)
            {
                enemyHealth.TakeDamage(explosionDamage * 0.3f);
            }
        }
        
        // Devolver al pool o destruir
        if (DronePool.Instance != null)
        {
            DronePool.Instance.ReturnDrone(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDroneDeath()
    {
        Explode();
    }
    
    private void OnDestroy()
    {
        if (FlockingManager3D.Instance != null && boid != null)
        {
            FlockingManager3D.Instance.RemoveBoid(boid);
        }
    }
    
    private void OnDrawGizmos()
    {
        // Área de patrol
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
        
        // Rangos de detección
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}