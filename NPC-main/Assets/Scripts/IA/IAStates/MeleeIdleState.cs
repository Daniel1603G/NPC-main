using UnityEngine;

public class MeleeIdleState : IState
{
    private readonly MeleeGuardAI ai;
    private float timer;

    public MeleeIdleState(MeleeGuardAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        timer = 0f;
    }

    public void Execute()
    {
        if (ai.IsPlayerInDetectionRange())
        {
            ai.ChangeState(ai.ChaseStateInstance);
            return;
        }

        // si no hay patrol points, quedarse en idle
        if (!ai.HasPatrolPoints())
            return;

        timer += Time.deltaTime;
        if (timer >= ai.IdleDuration)
        {
            ai.ChangeState(ai.PatrolStateInstance);
        }
    }

    public void Exit() { }
}
