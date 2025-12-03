using UnityEngine;

public class PatrolDroneState : IState
{
    private readonly DroneAI ai;
    
    public PatrolDroneState(DroneAI ai)
    {
        this.ai = ai;
    }
    
    public void Enter()
    {
        ai.SetPatrolMode();
        
        if (ai.Boid != null)
        {
            ai.Boid.SetMaxSpeed(ai.PatrolSpeed);
        }
    }
    
    public void Execute()
    {
        
        if (ai.CanSeePlayer())
        {
            ai.ChangeState(ai.ChaseStateInstance);
            return;
        }
        
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