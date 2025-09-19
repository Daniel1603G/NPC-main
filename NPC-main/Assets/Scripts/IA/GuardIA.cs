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

    private IdleState idleState;
    private PatrolState patrolState;
    private ChaseState chaseState;
    private AttackState attackState;

    private ITreeNode _rootNode;

    public IdleState IdleStateInstance => idleState;
    public PatrolState PatrolStateInstance => patrolState;
    public ChaseState ChaseStateInstance => chaseState;
    public AttackState AttackStateInstance => attackState;

    [SerializeField] private LineOfSight lineOfSight;

    private Transform[] patrolPoints;
    private int patrolIndex;
    private float lastSawPlayerTime;

    public Transform Player => player;
    public float AttackRange => attackRange;
    public float IdleDuration => idleDuration;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float ArriveThreshold => arriveThreshold;
    public IState CurrentState => currentState;

    private void Awake()
    {
        lastSawPlayerTime = -Mathf.Infinity;
        controller = GetComponent<CharacterController>();

        if (lineOfSight == null)
            lineOfSight = GetComponent<LineOfSight>();

        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            patrolPoints = (Transform[])patrolWaypoints.Clone();
        else
            patrolPoints = new Transform[0];

        patrolIndex = 0;

        idleState = new IdleState(this);
        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
    }

    private void Start() => ChangeState(idleState);

    private void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && yVel < 0) yVel = -2f;

        yVel += gravity * Time.deltaTime;
        currentState?.Execute();

    }

    public void ChangeState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
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
        if (finalDir.sqrMagnitude > 0.0001f) finalDir = finalDir.normalized;

        controller.Move(finalDir * speed * Time.deltaTime);

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
}
