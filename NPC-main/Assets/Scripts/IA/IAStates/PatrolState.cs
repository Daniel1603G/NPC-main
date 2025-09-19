using UnityEngine;

public class PatrolState : IState
{
    private readonly GuardAI ai;
    private Transform currentTarget;

    public PatrolState(GuardAI ai)
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

        if (currentTarget == null) return;

        float distanceToTarget = Vector3.Distance(ai.transform.position, currentTarget.position);
        if (distanceToTarget <= ai.ArriveThreshold)
        {
            ai.ChangeState(ai.IdleStateInstance);
            return;
        }

        ai.MoveTowards(currentTarget.position, ai.PatrolSpeed);
    }

    public void Exit()
    {

    }
}