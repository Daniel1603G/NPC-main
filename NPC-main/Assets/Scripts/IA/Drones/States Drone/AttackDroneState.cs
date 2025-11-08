using UnityEngine;

/// <summary>
/// Attack: Velocidad MÁXIMA directo al jugador.
/// Explota al contacto.
/// </summary>
public class AttackDroneState : IState
{
    private readonly DroneAI ai;
    private float attackStartTime;
    private float maxAttackDuration = 6f;
    
    public AttackDroneState(DroneAI ai)
    {
        this.ai = ai;
    }
    
    public void Enter()
    {
        Debug.Log($"{ai.name}: ¡¡ATAQUE KAMIKAZE!!");
        ai.SetAttackMode();
        attackStartTime = Time.time;
        
        // Velocidad MÁXIMA
        if (ai.Boid != null)
        {
            ai.Boid.SetMaxSpeed(ai.KamikazeSpeed);
        }
    }
    
    public void Execute()
    {
        if (ai.Player == null)
        {
            ai.Explode();
            return;
        }
        
        // Timeout de seguridad
        if (Time.time - attackStartTime > maxAttackDuration)
        {
            ai.Explode();
            return;
        }
        
        // Ir directo al jugador
        if (ai.Boid != null)
        {
            ai.Boid.SetTarget(ai.Player.position);
        }
        
        // Si está MUY cerca, explotar preventivamente
        float distance = Vector3.Distance(ai.transform.position, ai.Player.position);
        if (distance < 0.8f)
        {
            ai.Explode();
        }
    }
    
    public void Exit()
    {
    }
}
