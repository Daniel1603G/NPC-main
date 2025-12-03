using UnityEngine;


public class AlertState : IState
{
    private readonly GuardAI ai;
    private float alertStartTime;
    private float alertDuration = 5f;
    private float rotationSpeed = 120f;
    private Vector3 lastKnownThreatDirection;
    private bool hasCheckedThreatDirection;
    
   
    private float moveSpeed = 1.5f; 
    private Vector3 searchDirection;
    private float directionChangeInterval = 2f;
    private float lastDirectionChange;
    
    public AlertState(GuardAI ai)
    {
        this.ai = ai;
    }
    
    public void Enter()
    {
        alertStartTime = Time.time;
        lastDirectionChange = Time.time;
        hasCheckedThreatDirection = false;
        
     
        searchDirection = ai.transform.forward;
        
        Debug.Log($"{ai.name}: ¡ALERTA! Buscando amenaza...");
    }
    
    public void Execute()
    {
        
        if (ai.IsPlayerInDetectionRange())
        {
            ai.ChangeState(ai.GetCombatState());
            return;
        }
        
       
        if (Time.time - alertStartTime > alertDuration)
        {
            Debug.Log($"{ai.name}: Alerta terminada, volviendo a patrulla");
            ai.ChangeState(ai.PatrolStateInstance);
            return;
        }
        
   
        if (!hasCheckedThreatDirection && lastKnownThreatDirection != Vector3.zero)
        {
            Vector3 targetDirection = lastKnownThreatDirection;
            targetDirection.y = 0f;
            
            if (targetDirection != Vector3.zero)
            {
                
                ai.MoveTowards(ai.transform.position + targetDirection * 3f, moveSpeed);
                
           
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                ai.transform.rotation = Quaternion.RotateTowards(
                    ai.transform.rotation,
                    targetRotation,
                    rotationSpeed * 2f * Time.deltaTime
                );
                
                float angleDiff = Quaternion.Angle(ai.transform.rotation, targetRotation);
                if (angleDiff < 5f)
                {
                    hasCheckedThreatDirection = true;
                    lastDirectionChange = Time.time;
                }
            }
        }
        else
        {
            // Patrulla de búsqueda: caminar y rotar
            
            // Cambiar dirección periódicamente
            if (Time.time - lastDirectionChange > directionChangeInterval)
            {
                // Nueva dirección aleatoria
                float randomAngle = Random.Range(-90f, 90f);
                searchDirection = Quaternion.Euler(0f, randomAngle, 0f) * ai.transform.forward;
                lastDirectionChange = Time.time;
            }
            
            // Moverse en la dirección de búsqueda
            Vector3 targetPos = ai.transform.position + searchDirection * 2f;
            ai.MoveTowards(targetPos, moveSpeed);
            
            // Escaneo rotacional mientras camina
            ai.transform.Rotate(Vector3.up, rotationSpeed * 0.5f * Time.deltaTime);
        }
    }
    
    public void Exit()
    {
        Debug.Log($"{ai.name}: Saliendo de alerta");
    }
    
    public void SetThreatDirection(Vector3 direction)
    {
        lastKnownThreatDirection = direction;
        hasCheckedThreatDirection = false;
    }
}