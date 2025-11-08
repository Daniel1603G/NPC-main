using UnityEngine;

/// <summary>
/// Patrol: Flocking + deambular por área.
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
        Debug.Log($"{ai.name}: Patrullando área");
        ai.SetPatrolMode();
        
        // Velocidad lenta
        if (ai.Boid != null)
        {
            ai.Boid.SetMaxSpeed(ai.PatrolSpeed);
        }
    }
    
    public void Execute()
    {
        // Detectar jugador
        if (ai.CanSeePlayer())
        {
            ai.ChangeState(ai.ChaseStateInstance);
            return;
        }
        
        // Establecer objetivo aleatorio
        if (ai.Boid != null)
        {
            ai.Boid.SetTarget(ai.GetCurrentPatrolTarget());
        }
    }
    
    public void Exit()
    {
        if (ai.Boid != null)
        {
            ai.Boid.ClearTarget();
        }
    }
}