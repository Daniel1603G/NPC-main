using UnityEngine;

public class ChaseState : IState
{
    private readonly GuardAI ai;

    private Vector3 lastPlayerPosition;

    public ChaseState(GuardAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {

        lastPlayerPosition = ai.Player != null ? ai.Player.position : Vector3.zero;
    }

    public void Execute()
    {
        if (ai.Player == null)
        {
            ai.ChangeState(ai.PatrolStateInstance);
            return;
        }

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

        Vector3 currentPos = ai.Player.position;
        Vector3 velocity = Vector3.zero;

        if (Time.deltaTime > Mathf.Epsilon)
        {
            velocity = (currentPos - lastPlayerPosition) / Time.deltaTime;
        }

        lastPlayerPosition = currentPos;
        float distance = Vector3.Distance(ai.transform.position, currentPos);
        float speed = velocity.magnitude;
        float lookAheadTime = speed > 0.1f ? distance / speed : 0f;
        Vector3 predictedPos = currentPos + velocity * lookAheadTime;

        ai.MoveTowards(predictedPos, ai.ChaseSpeed);
    }

    public void Exit()
    {

    }
}