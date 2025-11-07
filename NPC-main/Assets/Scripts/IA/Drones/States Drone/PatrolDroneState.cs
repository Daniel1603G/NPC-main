using UnityEngine;

/// <summary>
/// Estado Patrol: El drone patrulla usando flocking.
/// Vuela en formación con otros drones buscando amenazas.
/// </summary>
public class PatrolDroneState : IState
{
    private readonly DroneAI ai;
    
    public PatrolDroneState(DroneAI ai)
    {
        this.ai = ai;
    }
    
    public void Enter()
    {
        Debug.Log($"{ai.name}: Patrullando");
    }
    
    public void Execute()
    {
        // Si detecta al jugador → Chase
        if (ai.CanSeePlayer())
        {
            ai.ChangeState(ai.ChaseStateInstance);
            return;
        }
        
        // El flocking se maneja automáticamente en Boid3D
        // Aquí solo verificamos transiciones
    }
    
    public void Exit()
    {
    }
}