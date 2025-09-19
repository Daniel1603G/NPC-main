using UnityEngine;

public class IdleRunnerState : IState
{
    private readonly RunnerAI ai;

    public IdleRunnerState(RunnerAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {

    }

    public void Execute()
    {
        if (ai.CanSeePlayerWithMemory())
        {
            ai.ChangeState(ai.RunAwayStateInstance);
            return;
        }

    }

    public void Exit()
    {

    }
}