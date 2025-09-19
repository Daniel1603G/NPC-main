using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RunnerAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player's Transform. The runner will flee from this target when detected.")]
    [SerializeField] private Transform player;
    [Tooltip("Line of sight component used to determine if the player is visible.")]
    [SerializeField] private LineOfSight lineOfSight;

    [Header("Run Settings")]
    [Tooltip("Speed at which the runner flees when the player is detected.")]
    [SerializeField] private float fleeSpeed = 3.5f;
    [Tooltip("Maximum rate of rotation in degrees per second when turning to face the movement direction.")]
    [SerializeField] private float rotationSpeed = 360f;
    [Tooltip("Number of seconds to continue fleeing after losing sight of the player.")]
    [SerializeField] private float lostSightDuration = 1.25f;

    [Header("Steering / Obstacle Avoidance")]
    [Tooltip("Layer mask defining which colliders should be treated as obstacles during movement.\nThis mask is separate from the LineOfSight obstruction mask.")]
    [SerializeField] private LayerMask movementAvoidanceMask;
    [Tooltip("How far ahead the runner will probe for obstacles when avoiding them.")]
    [SerializeField] private float avoidDistance = 2f;
    [Tooltip("Influence of the avoidance steering vector relative to the desired flee direction.")]
    [SerializeField] private float avoidStrength = 2f;

    private CharacterController controller;
    private float lastSawPlayerTime;
    private IState currentState;
    private IdleRunnerState idleState;
    private RunAwayState runAwayState;
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
        {
            lineOfSight = GetComponent<LineOfSight>();
        }

        lastSawPlayerTime = -Mathf.Infinity;

        idleState = new IdleRunnerState(this);
        runAwayState = new RunAwayState(this);
    }
    private void Start()
    {
        ChangeState(idleState);
    }

    private void Update()
    {
        currentState?.Execute();
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
        bool visible;
        if (lineOfSight != null)
        {
            visible = lineOfSight.CanSeeTarget(player);
        }
        else
        {
            visible = Vector3.Distance(transform.position, player.position) <= fleeSpeed * 2f;
        }
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

        if (worldDir.sqrMagnitude < 0.0001f) return;
        Vector3 desiredDir = worldDir.normalized;
        Vector3 avoidance = ComputeObstacleAvoidance(desiredDir);

        Vector3 finalDir = desiredDir + avoidance * avoidStrength;
        finalDir.y = 0f;
        if (Vector3.Dot(finalDir, desiredDir) < 0f)
        {
            finalDir = desiredDir - avoidance * avoidStrength;
        }
        if (finalDir.sqrMagnitude > 0.0001f)
        {
            finalDir = finalDir.normalized;
        }
        controller.Move(finalDir * speed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(finalDir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private Vector3 ComputeObstacleAvoidance(Vector3 desiredDirection)
    {
        if (movementAvoidanceMask == 0) return Vector3.zero;
        float radius = 0.5f;
        float height = 1f;

        if (controller != null)
        {
            radius = controller.radius;
            height = controller.height;
        }

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
}