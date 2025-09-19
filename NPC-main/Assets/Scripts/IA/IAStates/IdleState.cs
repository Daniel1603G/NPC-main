using UnityEngine;

public class IdleState : IState
{
    private readonly GuardAI ai;
    private float timer;

    public IdleState(GuardAI ai)
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

        timer += Time.deltaTime;
        if (timer >= ai.IdleDuration)
        {
            ai.ChangeState(ai.PatrolStateInstance);
        }
    }

    public void Exit()
    {

    }
}