using UnityEngine;

/// <summary>
/// Estado Idle: el jugador está parado sin moverse.
/// Transiciones:
/// - A Walking si hay input de movimiento
/// - A Jumping si presiona salto
/// </summary>
public class IdlePlayerState : IState
{
    private readonly PlayerController controller;
    
    public IdlePlayerState(PlayerController controller)
    {
        this.controller = controller;
    }
    
    public void Enter()
    {
        // Detener el movimiento cuando entramos a Idle
        controller.Movement.StopMovement();
    }
    
    public void Execute()
    {
        // Siempre aplicar gravedad
        controller.Movement.ApplyGravity();
        
        // Verificar transiciones
        
        // Si está en el suelo y presiona salto → Jumping
        if (controller.Controller.isGrounded && controller.IsJumpKeyPressed())
        {
            controller.ChangeState(controller.JumpingStateInstance);
            return;
        }
        
        // Si hay input de movimiento → Walking
        if (controller.HasMovementInput())
        {
            controller.ChangeState(controller.WalkingStateInstance);
            return;
        }
    }
    
    public void Exit()
    {
        // No hay cleanup necesario
    }
}