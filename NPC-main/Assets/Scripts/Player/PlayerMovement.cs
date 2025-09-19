using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Sprint")]
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private float sprintMaxDuration = 4f;   
    [SerializeField] private float sprintCooldown = 2f;      

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;

    private CharacterController controller;
    private Vector3 velocity;

    private float sprintTimer = 0f;
    private float cooldownTimer = 0f;
    private bool sprintOnCooldown = false;

    public bool IsSprinting { get; private set; }
    public float SprintCooldownDuration => sprintCooldown;
    public float SprintMaxDuration => sprintMaxDuration;
    public float SprintRemaining01 => 1f - Mathf.Clamp01(sprintTimer / Mathf.Max(0.0001f, sprintMaxDuration));


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (controller.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }
            else if (velocity.y < 0f)
            {
                velocity.y = -2f; 
            }
        }

        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * inputX + transform.forward * inputZ;

        bool hasInput = move.sqrMagnitude > 0.0001f;

        if (sprintOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                sprintOnCooldown = false;
                sprintTimer = 0f;
            }
        }

        bool wantsSprint = Input.GetKey(sprintKey) && hasInput && !sprintOnCooldown;

        float currentSpeed = moveSpeed;

        if (wantsSprint)
        {
            IsSprinting = true;
            currentSpeed *= sprintMultiplier;
            sprintTimer += Time.deltaTime;

            if (sprintTimer >= sprintMaxDuration)
            {
                sprintOnCooldown = true;
                cooldownTimer = sprintCooldown;
                IsSprinting = false;        
                currentSpeed = moveSpeed;   
            }
        }
        else
        {
            IsSprinting = false;

            if (!sprintOnCooldown && (!Input.GetKey(sprintKey) || !hasInput))
            {
                sprintTimer = 0f; 
            }
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public bool IsSprintOnCooldown => sprintOnCooldown;
    public float SprintTime => sprintTimer;
    public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
}
