using UnityEngine;

/// <summary>
/// Chase: Flocking + perseguir jugador a velocidad media.
/// </summary>
public class ChaseDroneState : IState
{
    private readonly DroneAI ai;
    
    public ChaseDroneState(DroneAI ai)
    {
        this.ai = ai;
    }
    
    public void Enter()
    {
        Debug.Log($"{ai.name}: Persiguiendo jugador");
        ai.SetChaseMode();
        
        // Velocidad media
        if (ai.Boid != null)
        {
            ai.Boid.SetMaxSpeed(ai.ChaseSpeed);
        }
    }
    
    public void Execute()
    {
        // Perdió de vista al jugador
        if (!ai.CanSeePlayer())
        {
            ai.ChangeState(ai.PatrolStateInstance);
            return;
        }
        
        // Está lo suficientemente cerca para atacar
        if (ai.IsInAttackRange())
        {
            ai.ChangeState(ai.AttackStateInstance);
            return;
        }
        
        // Perseguir
        if (ai.Player != null && ai.Boid != null)
        {
            ai.Boid.SetTarget(ai.Player.position);
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