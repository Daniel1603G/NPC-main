using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor de flocking para drones 3D.
/// Similar a FlockingManager pero para movimiento aéreo.
/// </summary>
public class FlockingManager3D : MonoBehaviour
{
    public static FlockingManager3D Instance { get; private set; }
    
    [Header("Radiuses")]
    [SerializeField] public float separationRadius = 2f;
    [SerializeField] public float cohesionRadius = 5f;
    
    [Header("Weights")]
    [SerializeField, Range(0, 3f)] public float separationWeight = 1.5f;
    [SerializeField, Range(0, 1f)] public float cohesionWeight = 1f;
    [SerializeField, Range(0, 1f)] public float alignmentWeight = 1f;
    
    [Header("Flight Bounds")]
    [SerializeField] private Vector3 flightCenter = Vector3.zero;
    [SerializeField] private Vector3 flightBounds = new Vector3(20f, 5f, 20f);
    
    private List<Boid3D> boids = new List<Boid3D>();
    
    public List<Boid3D> AllBoids => boids;
    public Vector3 FlightCenter => flightCenter;
    public Vector3 FlightBounds => flightBounds;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddBoid(Boid3D boid)
    {
        if (!boids.Contains(boid))
        {
            boids.Add(boid);
            Debug.Log($"Drone registrado. Total: {boids.Count}");
        }
    }
    
    public void RemoveBoid(Boid3D boid)
    {
        if (boids.Contains(boid))
        {
            boids.Remove(boid);
            Debug.Log($"Drone eliminado. Total: {boids.Count}");
        }
    }
    
    private void OnDrawGizmos()
    {
        // Dibujar área de vuelo
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireCube(flightCenter, flightBounds * 2f);
        
        // Centro del grupo
        if (boids.Count > 0)
        {
            Vector3 center = Vector3.zero;
            foreach (var boid in boids)
            {
                if (boid != null)
                    center += boid.transform.position;
            }
            center /= boids.Count;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, 1f);
        }
    }
}