using UnityEngine;

public class ShootingState : IState
{
    private readonly GuardAI ai;
    private readonly EnemyWeapon weapon;
    
    private readonly float minShootingDistance = 3f;
    private readonly float maxShootingDistance = 25f;
    private readonly float optimalDistance = 10f;
    
    private readonly float strafeSpeed = 2f;
    private float strafeDirection = 1f;
    private float lastStrafeChangeTime;
    private float strafeChangeInterval = 2f;
    
    private float stateEnterTime;
    private float minStateTime = 1f; // tiempo para salir del estado 
    public ShootingState(GuardAI ai, EnemyWeapon weapon)
    {
        this.ai = ai;
        this.weapon = weapon;
    }
    
    public void Enter()
    {
        Debug.Log($"{ai.name}: Entrando en estado Shooting");
        stateEnterTime = Time.time;
        lastStrafeChangeTime = Time.time;
        
      
        strafeDirection = Random.value > 0.5f ? 1f : -1f;
    }
    
    public void Execute()
    {
        if (ai.Player == null)
        {
            ai.ChangeState(ai.PatrolStateInstance);
            return;
        }
        
        
        bool canTransition = (Time.time - stateEnterTime) > minStateTime;
        
     
        if (!ai.IsPlayerInDetectionRange() && canTransition)
        {
            ai.ChangeState(ai.AlertStateInstance);
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(ai.transform.position, ai.Player.position);
        
        // Si el jugador está en rango de ataque cuerpo a cuerpo → Attack
        if (ai.IsPlayerInAttackRange() && canTransition)
        {
            ai.ChangeState(ai.AttackStateInstance);
            return;
        }
        
    
        PerformTacticalMovement(distanceToPlayer);
        
     
        AimAtPlayer();
        
    
        if (weapon != null && weapon.CanFire)
        {
            weapon.TryShoot(ai.Player);
        }
    }
    
    public void Exit()
    {
        Debug.Log($"{ai.name}: Saliendo de estado Shooting");
    }
    
    private void PerformTacticalMovement(float distanceToPlayer)
    {
        Vector3 moveDirection = Vector3.zero;
        
        // === MANTENER DISTANCIA ÓPTIMA ===
        Vector3 toPlayer = ai.Player.position - ai.transform.position;
        toPlayer.y = 0f;
        
        if (distanceToPlayer < minShootingDistance)
        {
            // Demasiado cerca → Retroceder
            moveDirection = -toPlayer.normalized * ai.PatrolSpeed;
        }
        else if (distanceToPlayer > optimalDistance)
        {
            // Muy lejos → Avanzar
            moveDirection = toPlayer.normalized * ai.PatrolSpeed * 0.7f;
        }
        
     
        if (Time.time - lastStrafeChangeTime > strafeChangeInterval)
        {
            strafeDirection *= -1f;
            lastStrafeChangeTime = Time.time;
        }
        
    
        Vector3 strafeMovement = ai.transform.right * strafeDirection * strafeSpeed;
        moveDirection += strafeMovement;
        
        // === APLICAR MOVIMIENTO ===
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Vector3 targetPosition = ai.transform.position + moveDirection * Time.deltaTime;
            ai.MoveTowards(targetPosition, ai.PatrolSpeed);
        }
    }
    
 
    private void AimAtPlayer()
    {
        Vector3 directionToPlayer = ai.Player.position - ai.transform.position;
        directionToPlayer.y = 0f;
        
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            ai.transform.rotation = Quaternion.RotateTowards(
                ai.transform.rotation,
                targetRotation,
                720f * Time.deltaTime // Rotación muy rápida para tracking preciso
            );
        }
    }
}