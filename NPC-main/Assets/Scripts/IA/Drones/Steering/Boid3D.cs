using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boid 3D con flocking constante y movimiento natural.
/// SIN rotaciones raras, SOLO movimiento suave.
/// </summary>
public class Boid3D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float maxSpeed = 5f;
    [SerializeField] protected float maxForce = 8f;
    [SerializeField] protected float rotationSmoothTime = 0.3f; // Suavizado de rotación
    
    [Header("3D Flight Settings")]
    [SerializeField]public float minFlightHeight = 3f;
    [SerializeField] public float maxFlightHeight = 10f;
    [SerializeField] private float heightCorrectionForce = 1.5f;
    
    [Header("Visual Movement")]
    [SerializeField] private bool enableSubtleBob = true;
    [SerializeField] private float bobFrequency = 1.5f;
    [SerializeField] private float bobAmplitude = 0.2f;
    
    protected Vector3 velocity;
    private Vector3 targetPosition;
    private bool hasTarget = false;
    private float bobOffset;
    private Vector3 rotationVelocity;
    
    private List<IFlockingBevaviour> flockingBehabiours = new List<IFlockingBevaviour>();
    
    public Vector3 Velocity => velocity;
    private List<Boid3D> AllBoids => FlockingManager3D.Instance != null ? FlockingManager3D.Instance.AllBoids : new List<Boid3D>();
    private FlockingManager3D FM => FlockingManager3D.Instance;
    
    void Start()
    {
        if (FlockingManager3D.Instance != null)
        {
            FlockingManager3D.Instance.AddBoid(this);
        }
        
        // Offset único para movimiento ondulante
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
        
        // Velocidad inicial aleatoria suave
        Vector3 randomDir = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;
        
        velocity = randomDir * (maxSpeed * 0.5f);
        
        flockingBehabiours.AddRange(GetComponents<IFlockingBevaviour>());
    }
    
    void Update()
    {
        // === FLOCKING SIEMPRE ACTIVO ===
        Vector3 flockingForce = Flocking();
        
        // Agregar objetivo si existe (patrol o chase)
        if (hasTarget)
        {
            Vector3 seekForce = Seek(targetPosition);
            flockingForce += seekForce * 0.4f; // 40% peso del objetivo
        }
        
        AddForce(flockingForce);
        
        // Mantener altura
        MaintainFlightHeight();
        
        // Movimiento ondulante sutil
        if (enableSubtleBob)
            ApplySubtleBob();
        
        // Moverse
        Move();
    }
    
    /// <summary>
    /// Establece un objetivo para el boid.
    /// </summary>
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        hasTarget = true;
    }
    
    /// <summary>
    /// Limpia el objetivo.
    /// </summary>
    public void ClearTarget()
    {
        hasTarget = false;
    }
    
    /// <summary>
    /// Ajusta la velocidad máxima dinámicamente.
    /// </summary>
    public void SetMaxSpeed(float speed)
    {
        maxSpeed = speed;
    }
    
    /// <summary>
    /// Movimiento ondulante MUY sutil (no mareante).
    /// </summary>
    private void ApplySubtleBob()
    {
        float bob = Mathf.Sin((Time.time + bobOffset) * bobFrequency) * bobAmplitude;
        Vector3 bobForce = Vector3.up * bob;
        AddForce(bobForce);
    }
    
    private Vector3 Flocking()
    {
        Vector3 flockingForce = Vector3.zero;
        
        // Solo aplicar flocking si hay otros boids cerca
        if (AllBoids.Count > 1)
        {
            flockingForce = 
                Separation() * FM.separationWeight +
                Cohesion() * FM.cohesionWeight +
                Alignment() * FM.alignmentWeight;
        }
        
        return flockingForce;
    }
    
    private Vector3 Separation()
    {
        Vector3 totalDir = Vector3.zero;
        int count = 0;
        
        for (int i = 0; i < AllBoids.Count; i++)
        {
            var boid = AllBoids[i];
            if (boid == this) continue;
            
            Vector3 dir = transform.position - boid.transform.position;
            float magnitude = dir.magnitude;
            
            if (magnitude > FM.separationRadius || magnitude < 0.01f) continue;
            
            // Inversamente proporcional a la distancia
            dir = (dir / magnitude) * (1f / magnitude);
            totalDir += dir;
            count++;
        }
        
        if (count == 0) return Vector3.zero;
        
        totalDir /= count;
        return Steer(totalDir.normalized * maxSpeed);
    }
    
    private Vector3 Cohesion()
    {
        Vector3 avgPos = Vector3.zero;
        int count = 0;
        
        for (int i = 0; i < AllBoids.Count; i++)
        {
            var boid = AllBoids[i];
            if (boid == this) continue;
            
            float sqrDistance = (transform.position - boid.transform.position).sqrMagnitude;
            if (sqrDistance > FM.cohesionRadius * FM.cohesionRadius) continue;
            
            avgPos += boid.transform.position;
            count++;
        }
        
        if (count == 0) return Vector3.zero;
        
        avgPos /= count;
        return Seek(avgPos);
    }
    
    private Vector3 Alignment()
    {
        Vector3 avgVelocity = Vector3.zero;
        int count = 0;
        
        for (int i = 0; i < AllBoids.Count; i++)
        {
            var boid = AllBoids[i];
            if (boid == this) continue;
            
            float sqrDistance = (transform.position - boid.transform.position).sqrMagnitude;
            if (sqrDistance > FM.cohesionRadius * FM.cohesionRadius) continue;
            
            avgVelocity += boid.Velocity;
            count++;
        }
        
        if (count == 0) return Vector3.zero;
        
        avgVelocity /= count;
        return Steer(avgVelocity.normalized * maxSpeed);
    }
    
    private void MaintainFlightHeight()
    {
        float currentHeight = transform.position.y;
        
        if (currentHeight < minFlightHeight)
        {
            Vector3 upForce = Vector3.up * heightCorrectionForce;
            AddForce(upForce);
        }
        else if (currentHeight > maxFlightHeight)
        {
            Vector3 downForce = Vector3.down * heightCorrectionForce;
            AddForce(downForce);
        }
    }
    
    // === STEERING METHODS ===
    
    public Vector3 Seek(Vector3 position)
    {
        Vector3 desired = position - transform.position;
        desired.y *= 0.5f; // Reducir movimiento vertical brusco
        return Steer(desired.normalized * maxSpeed);
    }
    
    public Vector3 Steer(Vector3 desired)
    {
        Vector3 steering = desired - velocity;
        return Vector3.ClampMagnitude(steering, maxForce * Time.deltaTime);
    }
    
    public void AddForce(Vector3 force)
    {
        velocity = Vector3.ClampMagnitude(velocity + force, maxSpeed);
    }
    
    /// <summary>
    /// Movimiento y rotación SUAVE sin giros raros.
    /// </summary>
    public void Move()
    {
        if (velocity.sqrMagnitude < 0.01f) return;
        
        // Movimiento
        transform.position += velocity * Time.deltaTime;
        
        // Rotación SUAVE solo en el plano horizontal
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        
        if (horizontalVelocity.sqrMagnitude > 0.1f)
        {
            // Usar SmoothDamp para rotación ultra suave
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime / rotationSmoothTime
            );
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (FM == null) return;
        
        // Cohesión
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, FM.cohesionRadius);
        
        // Separación
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, FM.separationRadius);
        
        // Objetivo actual
        if (hasTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }
        
        // Dirección de velocidad
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, velocity.normalized * 2f);
    }
}