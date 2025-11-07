using UnityEngine;

/// <summary>
/// Estado Kamikaze: El drone se lanza directamente al jugador para explotar.
/// Ignora flocking y va a máxima velocidad.
/// </summary>
public class KamikazeState : IState
{
    private readonly DroneAI ai;
    private float kamikazeStartTime;
    private float maxKamikazeDuration = 5f; // Explotar después de 5s si no impacta
    
    public KamikazeState(DroneAI ai)
    {
        this.ai = ai;
    }
    
    public void Enter()
    {
        Debug.Log($"{ai.name}: ¡¡¡KAMIKAZE!!!");
        kamikazeStartTime = Time.time;
        
        // Sonido de alerta intenso
        ai.PlayAlertSound();
    }
    
    public void Execute()
    {
        if (ai.Player == null)
        {
            // Si no hay jugador, explotar de todos modos
            ai.Explode();
            return;
        }
        
        // Explotar si pasó demasiado tiempo
        if (Time.time - kamikazeStartTime > maxKamikazeDuration)
        {
            ai.Explode();
            return;
        }
        
        // Movimiento directo hacia el jugador a máxima velocidad
        Vector3 toPlayer = ai.Player.position - ai.transform.position;
        
        // Ignorar flocking, ir directo
        if (ai.Boid != null)
        {
            // Resetear velocity y aplicar fuerza máxima
            Vector3 kamikazeVelocity = toPlayer.normalized * ai.KamikazeSpeed;
            ai.Boid.AddForce(kamikazeVelocity * 10f); // Fuerza muy alta
        }
        
        // Rotar hacia el objetivo
        if (toPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
            ai.transform.rotation = Quaternion.RotateTowards(
                ai.transform.rotation,
                targetRotation,
                720f * Time.deltaTime
            );
        }
        
        // Verificar colisión con el jugador
        float distanceToPlayer = Vector3.Distance(ai.transform.position, ai.Player.position);
        if (distanceToPlayer < 1f) // Muy cerca
        {
            ai.Explode();
        }
    }
    
    public void Exit()
    {
    }
}