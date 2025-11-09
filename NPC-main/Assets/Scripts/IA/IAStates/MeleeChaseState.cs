using UnityEngine;

public class MeleeChaseState : IState
{
    private readonly MeleeGuardAI ai;

    public MeleeChaseState(MeleeGuardAI ai)
    {
        this.ai = ai;
    }

    public void Enter() { }

    public void Execute()
    {
        if (!ai.IsPlayerInDetectionRange())
        {
            ai.ChangeState(ai.PatrolStateInstance);
            return;
        }

        if (ai.IsPlayerInAttackRange())
        {
            ai.ChangeState(ai.AttackStateInstance);
            return;
        }

        if (ai.Player != null)
            ai.MoveTowards(ai.Player.position, ai.ChaseSpeed);
    }

    public void Exit() { }
}
