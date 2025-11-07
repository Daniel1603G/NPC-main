using UnityEngine;

/// <summary>
/// Estado Chase: El drone persigue al jugador mientras mantiene flocking.
/// </summary>
public class ChaseDroneState : IState
{
    private readonly DroneAI ai;
    private readonly float chaseForceMultiplier = 3f;
    
    public ChaseDroneState(DroneAI ai)
    {
        this.ai = ai;
    }
    
    public void Enter()
    {
        Debug.Log($"{ai.name}: ¡Objetivo detectado! Persiguiendo...");
        ai.PlayAlertSound();
    }
    
    public void Execute()
    {
        // Si pierde de vista al jugador → Patrol
        if (!ai.CanSeePlayer())
        {
            ai.ChangeState(ai.PatrolStateInstance);
            return;
        }
        
        // Si está lo suficientemente cerca → Kamikaze
        if (ai.IsInKamikazeRange())
        {
            ai.ChangeState(ai.KamikazeStateInstance);
            return;
        }
        
        // Agregar fuerza de persecución al jugador
        if (ai.Player != null && ai.Boid != null)
        {
            Vector3 toPlayer = ai.Boid.Seek(ai.Player.position);
            ai.Boid.AddForce(toPlayer * chaseForceMultiplier);
        }
    }
    
    public void Exit()
    {
    }
}