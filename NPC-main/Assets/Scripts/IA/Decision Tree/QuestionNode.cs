using System;

public class QuestionNode : ITreeNode
{
    private readonly Func<bool> question;
    private readonly ITreeNode trueNode;
    private readonly ITreeNode falseNode;

    public QuestionNode(Func<bool> question, ITreeNode trueNode, ITreeNode falseNode)
    {
        this.question = question;
        this.trueNode = trueNode;
        this.falseNode = falseNode;
    }

    public void Execute()
    {
        if (question())
            trueNode.Execute();
        else
            falseNode.Execute();
    }
}