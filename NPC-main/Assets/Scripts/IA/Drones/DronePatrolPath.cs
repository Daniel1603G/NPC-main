using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera y gestiona rutas de patrulla para drones.
/// Soporta patrones en círculo, figura 8, y custom.
/// </summary>
public class DronePatrolPath : MonoBehaviour
{
    public enum PatrolPattern
    {
        Circle,
        FigureEight,
        Square,
        Custom
    }
    
    [Header("Pattern Settings")]
    [SerializeField] private PatrolPattern patternType = PatrolPattern.Circle;
    [SerializeField] private float patternSize = 10f;
    [SerializeField] private int waypointCount = 8;
    [SerializeField] private float baseHeight = 5f;
    [SerializeField] private float heightVariation = 2f;
    
    [Header("Custom Waypoints")]
    [SerializeField] private Transform[] customWaypoints;
    
    private List<Vector3> waypoints = new List<Vector3>();
    
    public List<Vector3> Waypoints => waypoints;
    
    private void Awake()
    {
        GenerateWaypoints();
    }
    
    private void OnValidate()
    {
        GenerateWaypoints();
    }
    

    private void GenerateWaypoints()
    {
        waypoints.Clear();
        
        switch (patternType)
        {
            case PatrolPattern.Circle:
                GenerateCirclePattern();
                break;
            case PatrolPattern.FigureEight:
                GenerateFigureEightPattern();
                break;
            case PatrolPattern.Square:
                GenerateSquarePattern();
                break;
            case PatrolPattern.Custom:
                GenerateCustomPattern();
                break;
        }
    }
    
    
    private void GenerateCirclePattern()
    {
        Vector3 center = transform.position;
        
        for (int i = 0; i < waypointCount; i++)
        {
            float angle = (i / (float)waypointCount) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * patternSize;
            float z = Mathf.Sin(angle) * patternSize;
            
         
            float heightOffset = Mathf.Sin(angle * 2f) * heightVariation;
            float y = baseHeight + heightOffset;
            
            Vector3 waypoint = center + new Vector3(x, y, z);
            waypoints.Add(waypoint);
        }
    }
    
 
    private void GenerateFigureEightPattern()
    {
        Vector3 center = transform.position;
        
        for (int i = 0; i < waypointCount; i++)
        {
            float t = (i / (float)waypointCount) * Mathf.PI * 2f;
            
          
            float scale = patternSize / (1f + Mathf.Sin(t) * Mathf.Sin(t));
            float x = scale * Mathf.Cos(t);
            float z = scale * Mathf.Sin(t) * Mathf.Cos(t);
            
            
            float heightOffset = Mathf.Sin(t * 3f) * heightVariation;
            float y = baseHeight + heightOffset;
            
            Vector3 waypoint = center + new Vector3(x, y, z);
            waypoints.Add(waypoint);
        }
    }
    
    
    private void GenerateSquarePattern()
    {
        Vector3 center = transform.position;
        float halfSize = patternSize * 0.5f;
        
        // Esquinas del cuadrado
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-halfSize, baseHeight, -halfSize),
            new Vector3(halfSize, baseHeight, -halfSize),
            new Vector3(halfSize, baseHeight, halfSize),
            new Vector3(-halfSize, baseHeight, halfSize)
        };
        
        // Generar waypoints interpolados entre esquinas
        int pointsPerSide = waypointCount / 4;
        for (int side = 0; side < 4; side++)
        {
            Vector3 start = corners[side];
            Vector3 end = corners[(side + 1) % 4];
            
            for (int i = 0; i < pointsPerSide; i++)
            {
                float t = i / (float)pointsPerSide;
                Vector3 waypoint = center + Vector3.Lerp(start, end, t);
                
               
                waypoint.y += Mathf.Sin(t * Mathf.PI) * heightVariation;
                
                waypoints.Add(waypoint);
            }
        }
    }
    

    private void GenerateCustomPattern()
    {
        if (customWaypoints == null || customWaypoints.Length == 0)
        {
            Debug.LogWarning("No custom waypoints assigned!");
            return;
        }
        
        foreach (var wp in customWaypoints)
        {
            if (wp != null)
                waypoints.Add(wp.position);
        }
    }
    
    
    public int GetClosestWaypointIndex(Vector3 position)
    {
        if (waypoints.Count == 0) return 0;
        
        int closestIndex = 0;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < waypoints.Count; i++)
        {
            float distance = Vector3.Distance(position, waypoints[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        
        return closestIndex;
    }
    
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0)
            return;
        
        // Dibujar waypoints
        Gizmos.color = Color.cyan;
        foreach (var wp in waypoints)
        {
            Gizmos.DrawWireSphere(wp, 0.3f);
        }
        
        // Dibujar líneas conectando waypoints
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 current = waypoints[i];
            Vector3 next = waypoints[(i + 1) % waypoints.Count];
            Gizmos.DrawLine(current, next);
        }
    }
}