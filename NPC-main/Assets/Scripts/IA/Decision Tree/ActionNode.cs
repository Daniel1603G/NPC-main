using System;

public class ActionNode : ITreeNode
{
    private readonly Action action;

    public ActionNode(Action action)
    {
        this.action = action;
    }

    public void Execute()
    {
        action?.Invoke();
    }
}