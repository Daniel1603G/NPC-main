using UnityEngine;

public class GuardDecisionTree : MonoBehaviour
{
    private GuardAI ai;
    private ITreeNode rootNode;

    private void Awake()
    {
        ai = GetComponent<GuardAI>();
        CreateDecisionTree();
    }

    private void Update()
    {
        rootNode?.Execute();
    }

    private void CreateDecisionTree()
    {
        var attackNode = new ActionNode(() =>
        {
            if (ai.CurrentState != ai.AttackStateInstance)
            {
                ai.ChangeState(ai.AttackStateInstance);
            }
        });

        var chaseNode = new ActionNode(() =>
        {
            if (ai.CurrentState != ai.ChaseStateInstance)
            {
                ai.ChangeState(ai.ChaseStateInstance);
            }
        });

        var idleNode = new ActionNode(() =>
        {
            if (ai.CurrentState != ai.IdleStateInstance)
            {
                ai.ChangeState(ai.IdleStateInstance);
            }
        });

        var patrolNode = new ActionNode(() =>
        {
            if (ai.CurrentState != ai.PatrolStateInstance)
            {
                ai.ChangeState(ai.PatrolStateInstance);
            }
        });

        rootNode = new QuestionNode(
            () => ai.IsPlayerInAttackRange(),
            attackNode,
            new QuestionNode(
                () => ai.IsPlayerInDetectionRange(),
                chaseNode,
                new QuestionNode(
                    () => ai.IsAtPatrolPoint() && ai.CurrentState != ai.IdleStateInstance,
                    idleNode,
                    patrolNode
                )
            )
        );
    }
}