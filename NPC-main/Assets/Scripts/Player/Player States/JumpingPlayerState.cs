using UnityEngine;

/// <summary>
/// Estado Jumping: el jugador está en el aire después de saltar.
/// Permite control aéreo limitado.
/// Transiciones:
/// - A Idle cuando toca el suelo sin input
/// - A Walking cuando toca el suelo con input
/// </summary>
public class JumpingPlayerState : IState
{
    private readonly PlayerController controller;
    
    public JumpingPlayerState(PlayerController controller)
    {
        this.controller = controller;
    }
    
    public void Enter()
    {
        // Ejecutar el salto al entrar al estado
        controller.Movement.Jump();
    }
    
    public void Execute()
    {
        // Permitir movimiento en el aire (control aéreo)
        Vector2 input = controller.GetMovementInput();
        if (input.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDir = controller.transform.right * input.x + controller.transform.forward * input.y;
            
            // Verificar si estaba sprinteando antes de saltar
            // Para mantener momentum en el aire
            bool wasSprinting = controller.IsSprintKeyPressed() && controller.Movement.CanSprint();
            controller.Movement.Move(moveDir, wasSprinting);
        }
        
        // Aplicar gravedad
        controller.Movement.ApplyGravity();
        
        // Verificar si tocó el suelo
        if (controller.Controller.isGrounded)
        {
            // Determinar a qué estado ir según el input
            if (!controller.HasMovementInput())
            {
                controller.ChangeState(controller.IdleStateInstance);
            }
            else if (controller.IsSprintKeyPressed() && controller.Movement.CanSprint())
            {
                controller.ChangeState(controller.SprintingStateInstance);
            }
            else
            {
                controller.ChangeState(controller.WalkingStateInstance);
            }
        }
    }
    
    public void Exit()
    {
        // No hay cleanup necesario
    }
}