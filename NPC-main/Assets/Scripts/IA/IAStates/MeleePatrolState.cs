using UnityEngine;

public class MeleePatrolState : IState
{
    private readonly MeleeGuardAI ai;
    private Transform currentTarget;

    public MeleePatrolState(MeleeGuardAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        currentTarget = ai.GetNextPatrolPoint();
    }

    public void Execute()
    {
        if (ai.IsPlayerInDetectionRange())
        {
            ai.ChangeState(ai.ChaseStateInstance);
            return;
        }

        if (currentTarget == null)
            return;

        float distanceToTarget = Vector3.Distance(ai.transform.position, currentTarget.position);
        if (distanceToTarget <= ai.ArriveThreshold)
        {
            ai.ChangeState(ai.IdleStateInstance);
            return;
        }

        ai.MoveTowards(currentTarget.position, ai.PatrolSpeed);
    }

    public void Exit() { }
}
