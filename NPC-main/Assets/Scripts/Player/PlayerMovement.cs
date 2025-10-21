using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] 
    [SerializeField] public float moveSpeed = 4f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Sprint")] 
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float sprintMaxDuration = 4f;
    [SerializeField] private float sprintCooldown = 2f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Power-Up Effects")] 
    [SerializeField] private float speedBoostMultiplier = 2f;
    [SerializeField] private float jumpBoostMultiplier = 2f;

    private PowerUpManager powerUpManager;
    private CharacterController controller;
    private Vector3 velocity;

    // Variables de sprint
    private float sprintTimer = 0f;
    private float cooldownTimer = 0f;
    private bool sprintOnCooldown = false;

    // Propiedades públicas
    public bool IsSprinting { get; private set; }
    public bool IsSprintOnCooldown => sprintOnCooldown;
    public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
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
        // Solo manejar cooldowns aquí
        UpdateSprintCooldown();
    }

    /// <summary>
    /// Mueve al jugador en una dirección.
    /// Llamado por los estados.
    /// </summary>
    /// <param name="direction">Dirección de movimiento (ya normalizada)</param>
    /// <param name="isSprinting">Si debe aplicar velocidad de sprint</param>
    public void Move(Vector3 direction, bool isSprinting)
    {
        direction = direction.normalized;
        
        float currentSpeed = moveSpeed;

        // Aplicar boost de velocidad si está activo
        if (powerUpManager != null && powerUpManager.IsSpeedBoostActive)
        {
            currentSpeed *= speedBoostMultiplier;
        }

        // Aplicar sprint si es válido
        if (isSprinting && CanSprint())
        {
            IsSprinting = true;
            currentSpeed *= sprintMultiplier;
            ConsumeSprint();
        }
        else
        {
            IsSprinting = false;
        }

        // Mover el character controller
        controller.Move(direction * currentSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Detiene el movimiento del jugador.
    /// </summary>
    public void StopMovement()
    {
        IsSprinting = false;
        // No reseteamos velocity.y para mantener la gravedad
    }

    /// <summary>
    /// Aplica la gravedad al jugador.
    /// Debe ser llamado cada frame por el estado actual.
    /// </summary>
    public void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f; // Pequeña fuerza hacia abajo para mantenerlo pegado al suelo
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Ejecuta un salto.
    /// Llamado cuando el estado Jumping entra.
    /// </summary>
    public void Jump()
    {
        float currentJumpHeight = jumpHeight;

        // Aplicar boost de salto si está activo
        if (powerUpManager != null && powerUpManager.HasSuperJump)
        {
            currentJumpHeight *= jumpBoostMultiplier;
        }

        velocity.y = Mathf.Sqrt(currentJumpHeight * -2f * gravity);
    }

    /// <summary>
    /// Verifica si el jugador puede sprintear.
    /// Considera cooldown, duración máxima y power-ups.
    /// </summary>
    public bool CanSprint()
    {
        // Si tiene sprint infinito, siempre puede
        bool hasInfiniteSprint = powerUpManager != null && powerUpManager.HasInfiniteSprint;
        if (hasInfiniteSprint)
            return true;

        // Si está en cooldown, no puede
        if (sprintOnCooldown)
            return false;

        // Si ya usó toda su duración, no puede
        if (sprintTimer >= sprintMaxDuration)
            return false;

        return true;
    }

    /// <summary>
    /// Consume stamina de sprint.
    /// </summary>
    private void ConsumeSprint()
    {
        // Si tiene sprint infinito, no consume
        bool hasInfiniteSprint = powerUpManager != null && powerUpManager.HasInfiniteSprint;
        if (hasInfiniteSprint)
            return;

        sprintTimer += Time.deltaTime;

        // Si se acabó el sprint, activar cooldown
        if (sprintTimer >= sprintMaxDuration)
        {
            sprintOnCooldown = true;
            cooldownTimer = sprintCooldown;
        }
    }

    /// <summary>
    /// Actualiza los cooldowns de sprint.
    /// </summary>
    private void UpdateSprintCooldown()
    {
        bool hasInfiniteSprint = powerUpManager != null && powerUpManager.HasInfiniteSprint;

        if (sprintOnCooldown && !hasInfiniteSprint)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                sprintOnCooldown = false;
                sprintTimer = 0f;
            }
        }

        // Si no está sprinteando ni en cooldown, resetear timer
        if (!IsSprinting && !sprintOnCooldown && !hasInfiniteSprint)
        {
            sprintTimer = 0f;
        }
    }
}