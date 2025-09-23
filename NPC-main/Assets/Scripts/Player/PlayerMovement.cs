using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] [SerializeField] public float moveSpeed = 4f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Sprint")] [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private float sprintMaxDuration = 4f;
    [SerializeField] private float sprintCooldown = 2f;
    
    public bool IsSprintOnCooldown => sprintOnCooldown;
    public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
 

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Power-Up Effects")] [SerializeField]
    private float speedBoostMultiplier = 2f;

    [SerializeField] private float jumpBoostMultiplier = 2f;
    
    

    private PowerUpManager powerUpManager;


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
        powerUpManager = GetComponent<PowerUpManager>();
        if (powerUpManager == null)
        {
            powerUpManager = FindObjectOfType<PowerUpManager>();
        }
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

        // === MODIFICACIÓN: Sprint infinito ===
        bool hasInfiniteSprint = powerUpManager != null && powerUpManager.HasInfiniteSprint;

        if (sprintOnCooldown && !hasInfiniteSprint) // Solo aplicar cooldown si no tiene sprint infinito
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                sprintOnCooldown = false;
                sprintTimer = 0f;
            }
        }

        bool wantsSprint = Input.GetKey(sprintKey) && hasInput && (!sprintOnCooldown || hasInfiniteSprint);

        float currentSpeed = moveSpeed;

        // === MODIFICACIÓN: Speed boost ===
        if (powerUpManager != null && powerUpManager.IsSpeedBoostActive)
        {
            currentSpeed *= speedBoostMultiplier;
        }

        if (wantsSprint)
        {
            IsSprinting = true;
            currentSpeed *= sprintMultiplier;

            if (!hasInfiniteSprint) // Solo consumir stamina si no tiene sprint infinito
            {
                sprintTimer += Time.deltaTime;

                if (sprintTimer >= sprintMaxDuration)
                {
                    sprintOnCooldown = true;
                    cooldownTimer = sprintCooldown;
                    IsSprinting = false;
                    currentSpeed = moveSpeed;
                }
            }
        }
        else
        {
            IsSprinting = false;

            if (!sprintOnCooldown && (!Input.GetKey(sprintKey) || !hasInput) && !hasInfiniteSprint)
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
        float currentJumpHeight = jumpHeight;

        // === MODIFICACIÓN: Super salto ===
        if (powerUpManager != null && powerUpManager.HasSuperJump)
        {
            currentJumpHeight *= jumpBoostMultiplier;
        }

        velocity.y = Mathf.Sqrt(currentJumpHeight * -2f * gravity);
        
    }
    
    
}
