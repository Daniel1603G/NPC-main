using UnityEngine;

/// <summary>
/// Estado Walking: el jugador se mueve a velocidad normal.
/// Transiciones:
/// - A Idle si no hay input de movimiento
/// - A Sprinting si presiona sprint y puede correr
/// - A Jumping si presiona salto
/// </summary>
public class WalkingPlayerState : IState
{
    private readonly PlayerController controller;
    
    public WalkingPlayerState(PlayerController controller)
    {
        this.controller = controller;
    }
    
    public void Enter()
    {
        // No hay setup especial al entrar
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
        
        // Si presiona sprint y puede sprintear → Sprinting
        if (controller.IsSprintKeyPressed() && controller.Movement.CanSprint())
        {
            controller.ChangeState(controller.SprintingStateInstance);
            return;
        }
        
        // Mover a velocidad normal
        Vector3 moveDir = controller.transform.right * input.x + controller.transform.forward * input.y;
        controller.Movement.Move(moveDir, false); // false = no sprint
        
        // Aplicar gravedad
        controller.Movement.ApplyGravity();
    }
    
    public void Exit()
    {
        // No hay cleanup necesario
    }
}