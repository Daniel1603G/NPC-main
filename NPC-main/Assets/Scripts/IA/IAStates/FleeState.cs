using UnityEngine;


public class FleeState : IState
{
    private readonly GuardAI ai;
    private readonly EnemyHealth health;
    
    private float fleeSpeed = 4f; 
    private float fleeDistance = 15f; 
    private float panicDuration = 8f; 
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
       
        if (health != null && health.HealthPercent > 0.35f) 
        {
            ai.ChangeState(ai.GetCombatState());
            return;
        }
        
        
        if (ai.Player != null)
        {
            float distanceToPlayer = Vector3.Distance(ai.transform.position, ai.Player.position);
            
            if (distanceToPlayer > fleeDistance * 1.5f && Time.time - fleeStartTime > panicDuration)
            {
                
                ai.ChangeState(ai.PatrolStateInstance);
                return;
            }
        }
        
       
        PerformFlee();
    }
    
    public void Exit()
    {
        Debug.Log($"{ai.name}: Dejando de huir");
    }
    
   
    private void PerformFlee()
    {
        if (ai.Player == null) return;
        
        
        Vector3 fleeDirection = ai.transform.position - ai.Player.position;
        fleeDirection.y = 0f;
        
        if (fleeDirection.sqrMagnitude < 0.01f)
        {
            
            fleeDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        }
        
        fleeDirection = fleeDirection.normalized;
        
     
        Vector3 targetPosition = ai.transform.position + fleeDirection * 5f;
        
        
        ai.MoveTowards(targetPosition, fleeSpeed);
        
        
        if (Random.value < 0.05f) 
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