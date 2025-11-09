using UnityEngine;

public class MeleeAttackState : IState
{
    private readonly MeleeGuardAI ai;
    private PlayerHealth playerHealth;

    private float lastAttackTime;
    private const float attackCooldown = 1.0f;
    private const float attackDamage = 25f; 

    public MeleeAttackState(MeleeGuardAI ai)
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

        // Mirar al jugador
        if (ai.Player != null)
        {
            Vector3 dir = ai.Player.position - ai.transform.position;
            dir.y = 0f;
            if (dir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir);
                ai.transform.rotation = Quaternion.RotateTowards(
                    ai.transform.rotation,
                    lookRot,
                    ai.RotationSpeed * Time.deltaTime);
            }
        }

        if (Time.time - lastAttackTime >= attackCooldown)
            DoAttack();
    }

    private void DoAttack()
    {
        lastAttackTime = Time.time;
        playerHealth?.TakeDamage(attackDamage);
        Debug.Log($"{ai.gameObject.name} realizó ataque melee al jugador");
    }

    public void Exit() { }
}
