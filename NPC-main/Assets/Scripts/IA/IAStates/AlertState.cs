using UnityEngine;

public class AlertState : IState
{
    private readonly GuardAI ai;
    private float alertStartTime;
    private float alertDuration = 5f; 
    private float rotationSpeed = 120f; 
    private Vector3 lastKnownThreatDirection;
    private bool hasCheckedThreatDirection;
    
    public AlertState(GuardAI ai)
    {
        this.ai = ai;
    }
    
    public void Enter()
    {
        alertStartTime = Time.time;
        hasCheckedThreatDirection = false;
        
        Debug.Log($"{ai.name}: ¡ALERTA! Buscando amenaza...");
    }
    
    public void Execute()
    {
        // Si detecta al jugador → Shooting
        if (ai.IsPlayerInDetectionRange())
        {
            IState targetState = (IState)ai.ShootingStateInstance ?? (IState)ai.ChaseStateInstance;
            ai.ChangeState(targetState);
            return;
        }
        
        // Si pasó el tiempo de alerta → Patrol
        if (Time.time - alertStartTime > alertDuration)
        {
            Debug.Log($"{ai.name}: Alerta terminada, volviendo a patrulla");
            ai.ChangeState(ai.PatrolStateInstance);
            return;
        }
        
        // === COMPORTAMIENTO DE BÚSQUEDA ===
        
        // Primero, mirar hacia la dirección del último ataque
        if (!hasCheckedThreatDirection && lastKnownThreatDirection != Vector3.zero)
        {
            Vector3 targetDirection = lastKnownThreatDirection;
            targetDirection.y = 0f;
            
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                ai.transform.rotation = Quaternion.RotateTowards(
                    ai.transform.rotation,
                    targetRotation,
                    rotationSpeed * 2f * Time.deltaTime // Rotación rápida inicial
                );
                
                // Verificar si ya está mirando en esa dirección
                float angleDiff = Quaternion.Angle(ai.transform.rotation, targetRotation);
                if (angleDiff < 5f)
                {
                    hasCheckedThreatDirection = true;
                }
            }
        }
        else
        {
            // Escaneo 360° lento
            ai.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    public void Exit()
    {
      //  Debug.Log($"{ai.name}: Saliendo de alerta`);
    }
    
    /// <summary>
    /// Establece la dirección desde donde vino el ataque.
    /// </summary>
    public void SetThreatDirection(Vector3 direction)
    {
        lastKnownThreatDirection = direction;
        hasCheckedThreatDirection = false;
    }
}