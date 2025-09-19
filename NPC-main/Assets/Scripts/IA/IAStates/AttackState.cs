using UnityEngine;

public class AttackState : IState
{
    private readonly GuardAI ai;
    private float lastAttackTime;
    private PlayerHealth playerHealth;

    private const float attackCooldown = 1f;
    private const float attackDamage = 100f;

    public AttackState(GuardAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        if (ai.Player != null)
            playerHealth = ai.Player.GetComponent<PlayerHealth>();

        lastAttackTime = Time.time - attackCooldown;

        if (ai.IsPlayerInAttackRange())
            DoAttack();
    }

    public void Execute()
    {
        if (!ai.IsPlayerInDetectionRange())
        {
            ai.ChangeState(ai.PatrolStateInstance);
            return;
        }

        if (!ai.IsPlayerInAttackRange())
        {
            ai.ChangeState(ai.ChaseStateInstance);
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
            DoAttack();
    }

    private void DoAttack()
    {
        lastAttackTime = Time.time;
        playerHealth?.TakeDamage(attackDamage);
        Debug.Log("guardia ataca jugador");

    }

    public void Exit() { }
}
