using UnityEngine;

/// <summary>
/// Estado Flee: El enemigo huye del jugador cuando tiene poca vida.
/// Añade realismo mostrando que el enemigo tiene miedo/instinto de supervivencia.
/// </summary>
public class FleeState : IState
{
    private readonly GuardAI ai;
    private readonly EnemyHealth health;
    
    private float fleeSpeed = 4f; // Más rápido que patrol, menos que chase
    private float fleeDistance = 15f; // Distancia que quiere mantener
    private float panicDuration = 8f; // Tiempo máximo huyendo antes de volver a combatir
    private float fleeStartTime;
    
    public FleeState(GuardAI ai, EnemyHealth health)
    {
        this.ai = ai;
        this.health = health;
    }
    
    public void Enter()
    {
        fleeStartTime = Time.time;
        Debug.Log($"{ai.name}: ¡Vida crítica! Huyendo...");
    }
    
    public void Execute()
    {
        // Si se recuperó algo de vida (powerup, etc.) → Volver a combate
        if (health != null && health.HealthPercent > 0.35f) // 35% para hysteresis
        {
            ai.ChangeState(ai.GetCombatState());
            return;
        }
        
        // Si está muy lejos del jugador y pasó tiempo → Patrol cauteloso
        if (ai.Player != null)
        {
            float distanceToPlayer = Vector3.Distance(ai.transform.position, ai.Player.position);
            
            if (distanceToPlayer > fleeDistance * 1.5f && Time.time - fleeStartTime > panicDuration)
            {
                // Recuperó compostura, volver a patrullar
                ai.ChangeState(ai.PatrolStateInstance);
                return;
            }
        }
        
        // === COMPORTAMIENTO DE HUIDA ===
        PerformFlee();
    }
    
    public void Exit()
    {
        Debug.Log($"{ai.name}: Dejando de huir");
    }
    
    /// <summary>
    /// Huye del jugador usando steering behavior.
    /// </summary>
    private void PerformFlee()
    {
        if (ai.Player == null) return;
        
        // Dirección opuesta al jugador
        Vector3 fleeDirection = ai.transform.position - ai.Player.position;
        fleeDirection.y = 0f;
        
        if (fleeDirection.sqrMagnitude < 0.01f)
        {
            // Si está justo encima, huir en dirección aleatoria
            fleeDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        }
        
        fleeDirection = fleeDirection.normalized;
        
        // Calcular posición objetivo (lejos del jugador)
        Vector3 targetPosition = ai.transform.position + fleeDirection * 5f;
        
        // Moverse hacia allá
        ai.MoveTowards(targetPosition, fleeSpeed);
        
        // Mirar ocasionalmente hacia atrás (para ver si sigue persiguiendo)
        if (Random.value < 0.05f) // 5% chance por frame
        {
            Vector3 lookBack = -fleeDirection;
            if (lookBack != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookBack);
                ai.transform.rotation = Quaternion.RotateTowards(
                    ai.transform.rotation,
                    lookRotation,
                    180f * Time.deltaTime
                );
            }
        }
    }
}