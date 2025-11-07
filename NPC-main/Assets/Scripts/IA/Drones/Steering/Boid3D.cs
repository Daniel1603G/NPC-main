using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Versión 3D del Boid que permite movimiento vertical.
/// Usado para drones que vuelan.
/// MEJORADO: Ahora con movimiento ondulante orgánico.
/// </summary>
public class Boid3D : SteeringEntity
{
    [Header("3D Flight Settings")]
    [SerializeField] private float minFlightHeight = 3f;
    [SerializeField] private float maxFlightHeight = 10f;
    [SerializeField] private float heightCorrectionForce = 2f;
    
    [Header("Organic Movement")]
    [SerializeField] private bool enableWavyMovement = true;
    [SerializeField] private float waveFrequency = 2f;
    [SerializeField] private float waveAmplitude = 0.5f;
    [SerializeField] private float waveOffset;
    
    // ✅ CORRECCIÓN 1: Cambiar tipo de List<Boid3D> a List<IFlockingBevaviour>
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
        
        waveOffset = Random.Range(0f, Mathf.PI * 2f);
        
        Vector3 randomDir = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.3f, 0.3f),
            Random.Range(-1f, 1f)
        ).normalized;
        
        AddForce(randomDir * maxSpeed);
        
        flockingBehabiours.AddRange(GetComponents<IFlockingBevaviour>());
    }
    
    void Update()
    {
        if (BoidsInRange())
            Flocking();
        
        if (enableWavyMovement)
            ApplyWavyMovement();
        
        MaintainFlightHeight();
        
        Move();
    }
    
    private void ApplyWavyMovement()
    {
        float lateralWave = Mathf.Sin((Time.time + waveOffset) * waveFrequency) * waveAmplitude;
        Vector3 lateralForce = transform.right * lateralWave;
        
        float verticalWave = Mathf.Cos((Time.time + waveOffset) * waveFrequency * 0.5f) * waveAmplitude * 0.3f;
        Vector3 verticalForce = Vector3.up * verticalWave;
        
        float rollWave = Mathf.Sin((Time.time + waveOffset) * waveFrequency * 1.5f) * 15f;
        transform.Rotate(Vector3.forward, rollWave * Time.deltaTime);
        
        AddForce(lateralForce + verticalForce);
    }
    
    private bool BoidsInRange()
    {
        if (FM == null) return false;
        
        for (int i = 0; i < AllBoids.Count; i++)
        {
            var boid = AllBoids[i];
            if (boid == this) continue;
            
            float sqrDistance = (transform.position - boid.transform.position).sqrMagnitude;
            if (sqrDistance <= FM.cohesionRadius * FM.cohesionRadius)
                return true;
        }
        return false;
    }
    
    private void Flocking()
    {
        Vector3 flockingForce = 
            Separation() * FM.separationWeight +
            Cohesion() * FM.cohesionWeight +
            Alignment() * FM.alignmentWeight;
        
        AddForce(flockingForce);
    }
    
    private Vector3 Separation()
    {
        Vector3 totalDir = Vector3.zero;
        
        for (int i = 0; i < AllBoids.Count; i++)
        {
            var boid = AllBoids[i];
            if (boid == this) continue;
            
            Vector3 dir = transform.position - boid.transform.position;
            float magnitude = dir.magnitude;
            
            if (magnitude > FM.separationRadius) continue;
            
            dir = (dir / magnitude) * (1f / magnitude);
            totalDir += dir;
        }
        
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
    
    // ✅ CORRECCIÓN 2: Agregar "Dis" al inicio del método
    public void DisableWavyMovement()
    {
        enableWavyMovement = false;
    }
    
    public void EnableWavyMovement()
    {
        enableWavyMovement = true;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (FM == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, FM.cohesionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, FM.separationRadius);
    }
}