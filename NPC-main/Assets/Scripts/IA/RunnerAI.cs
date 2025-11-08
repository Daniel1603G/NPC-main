using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RunnerAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player's Transform. The runner will flee from this target when detected.")]
    [SerializeField] private Transform player;
    [Tooltip("Line of sight component used to determine if the player is visible.")]
    [SerializeField] private LineOfSight lineOfSight;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyHealth enemyHealth; // 🧩 igual que GuardAI

    [Header("Run Settings")]
    [SerializeField] private float fleeSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float lostSightDuration = 1.25f;

    [Header("Steering / Obstacle Avoidance")]
    [SerializeField] private LayerMask movementAvoidanceMask;
    [SerializeField] private float avoidDistance = 2f;
    [SerializeField] private float avoidStrength = 2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    private float yVel;
    private bool isGrounded;

    private CharacterController controller;
    private float lastSawPlayerTime;
    private IState currentState;
    private IdleRunnerState idleState;
    private RunAwayState runAwayState;
    private Vector3 lastPosition;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");

    public Transform Player => player;
    public float FleeSpeed => fleeSpeed;
    public float RotationSpeed => rotationSpeed;
    public float LostSightDuration => lostSightDuration;
    public IdleRunnerState IdleStateInstance => idleState;
    public RunAwayState RunAwayStateInstance => runAwayState;

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
        {
            enemyHealth.OnDamagedFrom += OnReceivedDamage;
        }

        lastSawPlayerTime = -Mathf.Infinity;
        lastPosition = transform.position;

        idleState = new IdleRunnerState(this);
        runAwayState = new RunAwayState(this);
    }

    private void Start() => ChangeState(idleState);

    private void Update()
    {
        // 🔹 Si está muerto, destruirlo
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        // 🔹 Gravedad
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

        Vector3 horizontalMove = transform.position - lastPosition;
        horizontalMove.y = 0f;
        float speed = horizontalMove.magnitude / Time.deltaTime;

        animator.SetFloat(SpeedParam, speed);

        lastPosition = transform.position;
    }

    public void ChangeState(IState next)
    {
        if (currentState == next) return;
        currentState?.Exit();
        currentState = next;
        currentState?.Enter();
    }

    public bool CanSeePlayerWithMemory()
    {
        if (player == null) return false;

        bool visible = (lineOfSight != null)
            ? lineOfSight.CanSeeTarget(player)
            : Vector3.Distance(transform.position, player.position) <= fleeSpeed * 2f;

        if (visible)
        {
            lastSawPlayerTime = Time.time;
            return true;
        }

        return (Time.time - lastSawPlayerTime) <= lostSightDuration;
    }

    public void MoveInDirection(Vector3 worldDir, float speed)
    {
        worldDir.y = 0f;
        Vector3 move = Vector3.zero;

        if (worldDir.sqrMagnitude >= 0.0001f)
        {
            Vector3 desiredDir = worldDir.normalized;
            Vector3 avoidance = ComputeObstacleAvoidance(desiredDir);

            Vector3 finalDir = desiredDir + avoidance * avoidStrength;
            finalDir.y = 0f;

            if (Vector3.Dot(finalDir, desiredDir) < 0f)
                finalDir = desiredDir - avoidance * avoidStrength;

            if (finalDir.sqrMagnitude > 0.0001f)
                finalDir = finalDir.normalized;

            move = finalDir * speed;

            Quaternion targetRotation = Quaternion.LookRotation(finalDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        move.y = yVel;
        controller.Move(move * Time.deltaTime);
    }

    private Vector3 ComputeObstacleAvoidance(Vector3 desiredDirection)
    {
        if (movementAvoidanceMask == 0) return Vector3.zero;

        float radius = controller != null ? controller.radius : 0.5f;
        float height = controller != null ? controller.height : 1f;

        Vector3 origin = transform.position + Vector3.up * height * 0.5f;
        Ray ray = new Ray(origin, desiredDirection);

        if (Physics.SphereCast(ray, radius, out RaycastHit hit, avoidDistance, movementAvoidanceMask))
        {
            Vector3 avoidDir = Vector3.Cross(hit.normal, Vector3.up);
            avoidDir.y = 0f;
            return avoidDir.normalized;
        }

        return Vector3.zero;
    }

    // 🩸 Manejo de daño
    private void OnReceivedDamage(Vector3 damageDirection)
    {
        Debug.Log($"{gameObject.name}: ¡Recibí daño desde {damageDirection}!");
        // En el Runner no hay Alert ni AttackState, así que solo miramos al atacante
        Vector3 targetDir = damageDirection;
        targetDir.y = 0f;
        if (targetDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(targetDir);
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
            enemyHealth.OnDamagedFrom -= OnReceivedDamage;
    }
}
