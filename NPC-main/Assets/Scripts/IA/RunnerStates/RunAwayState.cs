using UnityEngine;

public class RunAwayState : IState
{
    private readonly RunnerAI ai;
    private Vector3 lastPlayerPosition;

    public RunAwayState(RunnerAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        if (ai.Player != null)
        {
            lastPlayerPosition = ai.Player.position;
        }
        else
        {
            lastPlayerPosition = Vector3.zero;
        }
    }

    public void Execute()
    {
        if (!ai.CanSeePlayerWithMemory())
        {
            ai.ChangeState(ai.IdleStateInstance);
            return;
        }
        if (ai.Player == null) return;
        Vector3 currentPos = ai.Player.position;
        Vector3 velocity = Vector3.zero;
        if (Time.deltaTime > Mathf.Epsilon)
        {
            velocity = (currentPos - lastPlayerPosition) / Time.deltaTime;
        }
        lastPlayerPosition = currentPos;

        Vector3 fleeDir = ai.transform.position - currentPos;

        if (fleeDir.sqrMagnitude < 0.0001f)
        {
            fleeDir = -ai.transform.forward;
        }

        ai.MoveInDirection(fleeDir, ai.FleeSpeed);
    }

    public void Exit()
    {

    }
}