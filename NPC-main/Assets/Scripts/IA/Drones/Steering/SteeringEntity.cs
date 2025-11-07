using System;
using UnityEngine;

/// <summary>
/// Entidad base con comportamientos de steering.
/// Patrón: Template Method
/// </summary>
public class SteeringEntity : MonoBehaviour
{
    [Header("Steering Settings")]
    [SerializeField] protected float maxSpeed = 5f;
    [SerializeField] protected float maxForce = 10f;
    
    protected Vector3 velocity;
    
    
    public Vector3 Seek(Vector3 position)
    {
        Vector3 desired = position - transform.position;
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
  
    public void Move()
    {
        if (velocity == Vector3.zero) return;
        
     
        if (velocity.sqrMagnitude > 0.01f)
        {
            transform.forward = velocity.normalized;
        }
        
     
        transform.position += velocity * Time.deltaTime;
    }
}