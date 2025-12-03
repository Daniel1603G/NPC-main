using UnityEngine;


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
    
  
    public PlayerMovement Movement => playerMovement;
    public CharacterController Controller => characterController;
    
    
    public IdlePlayerState IdleStateInstance => idleState;
    public WalkingPlayerState WalkingStateInstance => walkingState;
    public SprintingPlayerState SprintingStateInstance => sprintingState;
    public JumpingPlayerState JumpingStateInstance => jumpingState;
    
 
    public KeyCode JumpKey => jumpKey;
    public KeyCode SprintKey => sprintKey;
    public IState CurrentState => currentState;
    
    private void Awake()
    {
        
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
            
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        
      
        idleState = new IdlePlayerState(this);
        walkingState = new WalkingPlayerState(this);
        sprintingState = new SprintingPlayerState(this);
        jumpingState = new JumpingPlayerState(this);
    }
    
    private void Start()
    {
        
        ChangeState(idleState);
    }
    
    private void Update()
    {
      
        currentState?.Execute();
    }
    
  
    public void ChangeState(IState newState)
    {
        if (currentState == newState) return;
        
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
    
   
    public Vector2 GetMovementInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        return new Vector2(h, v);
    }
    
   
    public bool IsSprintKeyPressed()
    {
        return Input.GetKey(sprintKey);
    }
    
   
    public bool IsJumpKeyPressed()
    {
        return Input.GetKeyDown(jumpKey);
    }
    
  
    public bool HasMovementInput()
    {
        Vector2 input = GetMovementInput();
        return input.sqrMagnitude > 0.0001f;
    }
}