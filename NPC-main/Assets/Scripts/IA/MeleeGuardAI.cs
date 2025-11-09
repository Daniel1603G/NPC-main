using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MeleeGuardAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LineOfSight lineOfSight;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyHealth enemyHealth;

    [Header("Range")]
    [SerializeField] private float attackRange = 1.5f;

    [Header("Idle Settings")]
    [SerializeField] private float idleDuration = 3f;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float arriveThreshold = 0.2f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float lostSightDuration = 1.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Steering Settings")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float avoidDistance = 2f;
    [SerializeField] private float avoidStrength = 2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    private float yVel;
    private bool isGrounded;

    private CharacterController controller;
    private IState currentState;

    private MeleeIdleState idleState;
    private MeleePatrolState patrolState;
    private MeleeChaseState chaseState;
    private MeleeAttackState attackState;

    private Transform[] patrolPoints;
    private int patrolIndex;
    private float lastSawPlayerTime;
    private Vector3 lastPosition;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsAttackingParam = Animator.StringToHash("IsAttack");

    // === Getters usados por los estados ===
    public Transform Player => player;
    public float AttackRange => attackRange;
    public float IdleDuration => idleDuration;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float ArriveThreshold => arriveThreshold;
    public float RotationSpeed => rotationSpeed;

    public MeleeIdleState IdleStateInstance => idleState;
    public MeleePatrolState PatrolStateInstance => patrolState;
    public MeleeChaseState ChaseStateInstance => chaseState;
    public MeleeAttackState AttackStateInstance => attackState;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (lineOfSight == null)
            lineOfSight = GetComponent<LineOfSight>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        if (enemyHealth != null)
            enemyHealth.OnDamagedFrom += OnReceivedDamage;

        // patrol points igual que GuardAI
        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            patrolPoints = (Transform[])patrolWaypoints.Clone();
        else
            patrolPoints = new Transform[0];

        patrolIndex = 0;
        lastSawPlayerTime = -Mathf.Infinity;
        lastPosition = transform.position;

        // instancias de estados (fijate que usan ESTA clase)
        idleState = new MeleeIdleState(this);
        patrolState = new MeleePatrolState(this);
        chaseState = new MeleeChaseState(this);
        attackState = new MeleeAttackState(this);
    }

    private void Start()
    {
        ChangeState(idleState);
    }

    private void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        isGrounded = controller.isGrounded;
        if (isGrounded && yVel < 0f)
            yVel = -2f;

        yVel += gravity * Time.deltaTime;

        currentState?.Execute();
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector3 displacement = transform.position - lastPosition;
        displacement.y = 0f;

        float currentSpeed = displacement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        animator.SetFloat(SpeedParam, currentSpeed);

        bool isAttacking = (currentState == attackState);
        animator.SetBool(IsAttackingParam, isAttacking);

        lastPosition = transform.position;
    }

    public void ChangeState(IState newState)
    {
        if (currentState == newState) return;
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    // ===== Helpers EXACTOS al GuardAI (sin shoot/flee) =====

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

    public Transform GetNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return null;

        Transform nextPoint = patrolPoints[patrolIndex];
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        return nextPoint;
    }

    public bool HasPatrolPoints()
    {
        return patrolPoints != null && patrolPoints.Length > 0;
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

    private void OnReceivedDamage(Vector3 damageDirection)
    {
        if (enemyHealth != null && enemyHealth.IsDead) return;

        damageDirection.y = 0f;
        if (damageDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(damageDirection);
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
            enemyHealth.OnDamagedFrom -= OnReceivedDamage;
    }
}
