using UnityEngine;

/// <summary>
/// Estado Sprinting: el jugador corre a mayor velocidad.
/// Consume stamina y tiene cooldown.
/// Transiciones:
/// - A Walking si deja de presionar sprint o se agota el sprint
/// - A Idle si no hay input de movimiento
/// - A Jumping si presiona salto
/// </summary>
public class SprintingPlayerState : IState
{
    private readonly PlayerController controller;
    
    public SprintingPlayerState(PlayerController controller)
    {
        this.controller = controller;
    }
    
    public void Enter()
    {
        // No hay setup especial
    }
    
    public void Execute()
    {
        // Obtener input
        Vector2 input = controller.GetMovementInput();
        
        // Si no hay input → Idle
        if (!controller.HasMovementInput())
        {
            controller.ChangeState(controller.IdleStateInstance);
            return;
        }
        
        // Si presiona salto → Jumping
        if (controller.Controller.isGrounded && controller.IsJumpKeyPressed())
        {
            controller.ChangeState(controller.JumpingStateInstance);
            return;
        }
        
        // Si no puede sprintear más o suelta la tecla → Walking
        if (!controller.Movement.CanSprint() || !controller.IsSprintKeyPressed())
        {
            controller.ChangeState(controller.WalkingStateInstance);
            return;
        }
        
        // Mover a velocidad de sprint
        Vector3 moveDir = controller.transform.right * input.x + controller.transform.forward * input.y;
        controller.Movement.Move(moveDir, true); // true = sprint
        
        // Aplicar gravedad
        controller.Movement.ApplyGravity();
    }
    
    public void Exit()
    {
        // No hay cleanup necesario
    }
}