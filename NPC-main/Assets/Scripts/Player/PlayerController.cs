using UnityEngine;

/// <summary>
/// Controlador principal del jugador que maneja la FSM y coordina los diferentes componentes.
/// Similar a GuardAI pero para el player.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CharacterController characterController;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    
    [Header("State References")]
    private IState currentState;
    private IdlePlayerState idleState;
    private WalkingPlayerState walkingState;
    private SprintingPlayerState sprintingState;
    private JumpingPlayerState jumpingState;
    
    // Propiedades públicas para que los estados accedan
    public PlayerMovement Movement => playerMovement;
    public CharacterController Controller => characterController;
    
    // Referencias a instancias de estados (para que puedan cambiar entre ellos)
    public IdlePlayerState IdleStateInstance => idleState;
    public WalkingPlayerState WalkingStateInstance => walkingState;
    public SprintingPlayerState SprintingStateInstance => sprintingState;
    public JumpingPlayerState JumpingStateInstance => jumpingState;
    
    // Propiedades de input
    public KeyCode JumpKey => jumpKey;
    public KeyCode SprintKey => sprintKey;
    public IState CurrentState => currentState;
    
    private void Awake()
    {
        // Obtener referencias si no están asignadas
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
            
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        
        // Inicializar estados
        idleState = new IdlePlayerState(this);
        walkingState = new WalkingPlayerState(this);
        sprintingState = new SprintingPlayerState(this);
        jumpingState = new JumpingPlayerState(this);
    }
    
    private void Start()
    {
        // Comenzar en Idle
        ChangeState(idleState);
    }
    
    private void Update()
    {
        // Ejecutar el estado actual
        currentState?.Execute();
    }
    
    /// <summary>
    /// Cambia el estado actual de la FSM.
    /// Llama a Exit() del estado anterior y Enter() del nuevo estado.
    /// </summary>
    public void ChangeState(IState newState)
    {
        if (currentState == newState) return;
        
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
    
    /// <summary>
    /// Obtiene el input horizontal y vertical del jugador.
    /// </summary>
    public Vector2 GetMovementInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        return new Vector2(h, v);
    }
    
    /// <summary>
    /// Verifica si el jugador está presionando el botón de sprint.
    /// </summary>
    public bool IsSprintKeyPressed()
    {
        return Input.GetKey(sprintKey);
    }
    
    /// <summary>
    /// Verifica si el jugador presionó el botón de salto.
    /// </summary>
    public bool IsJumpKeyPressed()
    {
        return Input.GetKeyDown(jumpKey);
    }
    
    /// <summary>
    /// Verifica si hay input de movimiento.
    /// </summary>
    public bool HasMovementInput()
    {
        Vector2 input = GetMovementInput();
        return input.sqrMagnitude > 0.0001f;
    }
}