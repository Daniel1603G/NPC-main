using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GuardAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Range")]
    [SerializeField] private float attackRange = 1.5f;

    [Header("Idle Settings")]
    [SerializeField] private float idleDuration = 3f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    private float yVel;
    private bool isGrounded;
    
    [Header("Combat Settings")]
    [SerializeField] private EnemyWeapon enemyWeapon;
    [SerializeField] private float shootingDistance = 15f; 

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float arriveThreshold = 0.2f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [Tooltip("How long to continue chasing after losing sight of the player.")]
    [SerializeField] private float lostSightDuration = 1.5f;

    [Header("Rotation Settings")]
    [Tooltip("Angular speed (degrees per second) used to rotate the guard towards its movement direction")]
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Steering Settings")]
    [Tooltip("Layer mask defining which layers are considered obstacles for avoidance.")]
    [SerializeField] private LayerMask obstacleMask;
    [Tooltip("How far ahead the guard looks when avoiding obstacles.")]
    [SerializeField] private float avoidDistance = 2f;
    [Tooltip("Strength of the avoidance steering force.")]
    [SerializeField] private float avoidStrength = 2f;

    private CharacterController controller;
    private IState currentState;
    private AlertState alertState;

    private IdleState idleState;
    private PatrolState patrolState;
    private ChaseState chaseState;
    private AttackState attackState;
    private ShootingState shootingState;
    private FleeState fleeState;

    private ITreeNode _rootNode;

    public IdleState IdleStateInstance => idleState;
    public PatrolState PatrolStateInstance => patrolState;
    public ChaseState ChaseStateInstance => chaseState;
    public AttackState AttackStateInstance => attackState;
    public FleeState FleeStateInstance => fleeState;
    
    public AlertState AlertStateInstance => alertState;
    
    public ShootingState ShootingStateInstance => shootingState;
    public EnemyWeapon Weapon => enemyWeapon;

    [SerializeField] private LineOfSight lineOfSight;

    private Transform[] patrolPoints;
    private int patrolIndex;
    private float lastSawPlayerTime;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    [Header("Alert System")]
    [SerializeField] private EnemyHealth enemyHealth;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsAttackingParam = Animator.StringToHash("IsAttack");

    private Vector3 lastPosition;


    public Transform Player => player;
    public float AttackRange => attackRange;
    public float IdleDuration => idleDuration;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float ArriveThreshold => arriveThreshold;
    public IState CurrentState => currentState;

    private void Awake()
    {
        lastPosition = transform.position;

        lastSawPlayerTime = -Mathf.Infinity;
        controller = GetComponent<CharacterController>();

        if (lineOfSight == null)
            lineOfSight = GetComponent<LineOfSight>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(); 

        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            patrolPoints = (Transform[])patrolWaypoints.Clone();
        else
            patrolPoints = new Transform[0];

        patrolIndex = 0;

        
        if (enemyWeapon == null)
            enemyWeapon = GetComponentInChildren<EnemyWeapon>();
    
        if (enemyWeapon != null)
        {
            shootingState = new ShootingState(this, enemyWeapon);
            Debug.Log($"{gameObject.name}: ShootingState inicializado");
        }
        
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
    
        if (enemyHealth != null)
        {
            enemyHealth.OnDamagedFrom += OnReceivedDamage;
        }

        if (enemyHealth != null)
        {
            enemyHealth.OnDamagedFrom += OnReceivedDamage;
        
          
            enemyHealth.OnLowHealth += OnLowHealth;
        }
        alertState = new AlertState(this);
        idleState = new IdleState(this);
        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
           fleeState = new FleeState(this, enemyHealth);
    }

    private void Start() => ChangeState(idleState);

    private void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && yVel < 0) yVel = -2f;

        yVel += gravity * Time.deltaTime;
        currentState?.Execute();

        UpdateAnimator(); 
    }

    

    private void UpdateAnimator()
    {
        if (animator == null) return;

      
        Vector3 displacement = transform.position - lastPosition;
        displacement.y = 0f;

        float currentSpeed = displacement.magnitude / Time.deltaTime;

    
        animator.SetFloat(SpeedParam, currentSpeed);

      
        bool isAttacking = (currentState == attackState);
        animator.SetBool(IsAttackingParam, isAttacking);

        
        lastPosition = transform.position;
    }


    public void ChangeState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
    public IState GetCombatState()
    {
       
        if (shootingState != null && enemyWeapon != null)
        {
            return shootingState;
        }
    
      
        return chaseState;
    }
    public bool IsPlayerInDetectionRange()
    {
        if (player == null) return false;
        bool visible = (lineOfSight != null) && lineOfSight.CanSeeTarget(player);

        if (visible)
        {
            lastSawPlayerTime = Time.time;
            return true;
        }
        return (Time.time - lastSawPlayerTime) <= lostSightDuration;
    }

    public bool IsPlayerInAttackRange()
    {
        if (player == null) return false;

        bool withinRange = Vector3.Distance(transform.position, player.position) <= attackRange;
        if (!withinRange) return false;

        return IsPlayerInDetectionRange();
    }

    public void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f) return;

        Vector3 desiredDir = direction.normalized;
        Vector3 avoidance = ComputeObstacleAvoidance(desiredDir);

        Vector3 finalDir = desiredDir + avoidance * avoidStrength;
        finalDir.y = 0f;
        if (finalDir.sqrMagnitude > 0.0001f)
            finalDir = finalDir.normalized;

     
        Vector3 move = finalDir * speed;
        move.y = yVel; 

        controller.Move(move * Time.deltaTime);
   

        Quaternion targetRotation = Quaternion.LookRotation(finalDir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }


    private Vector3 ComputeObstacleAvoidance(Vector3 desiredDirection)
    {
        if (obstacleMask == 0) return Vector3.zero;
        float radius = controller != null ? controller.radius : 0.5f;
        float halfH = controller != null ? controller.height * 0.5f : 1f;

        Ray ray = new Ray(transform.position + Vector3.up * halfH, desiredDirection);
        if (Physics.SphereCast(ray, radius, out RaycastHit hit, avoidDistance, obstacleMask))
        {
            Vector3 avoidDir = Vector3.Cross(hit.normal, Vector3.up);
            avoidDir.y = 0f;
            return avoidDir.normalized;
        }
        return Vector3.zero;
    }

    public bool IsAtPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return false;
        Transform currentTarget = patrolPoints[Mathf.Clamp(patrolIndex, 0, patrolPoints.Length - 1)];
        if (currentTarget == null) return false;
        return Vector3.Distance(transform.position, currentTarget.position) <= arriveThreshold;
    }

    public Transform GetNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return null;
        Transform nextPoint = patrolPoints[patrolIndex];
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        return nextPoint;
    }
    public void ResetPatrol() => patrolIndex = 0;
    
    public bool ShouldShoot()
    {
        if (enemyWeapon == null || Player == null)
            return false;
    
        float distance = Vector3.Distance(transform.position, Player.position);
    
   
        return distance >= attackRange && distance <= shootingDistance && IsPlayerInDetectionRange();
    }
    
    private void OnReceivedDamage(Vector3 damageDirection)
    {
        if (currentState != attackState && currentState != shootingState)
        {
           
            if (alertState != null)
            {
                alertState.SetThreatDirection(damageDirection);
                ChangeState(alertState);
            }
        }
        else
        {
           
            Vector3 targetDir = damageDirection;
            targetDir.y = 0f;
            if (targetDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(targetDir);
            }
        }
    }
    
    private void OnLowHealth()
    {
        Debug.Log($"{gameObject.name}: ¡Salud crítica! Activando huida...");
    
        if (fleeState != null)
        {
            ChangeState(fleeState);
        }
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDamagedFrom -= OnReceivedDamage;
            enemyHealth.OnLowHealth -= OnLowHealth;
        }
    }
}
