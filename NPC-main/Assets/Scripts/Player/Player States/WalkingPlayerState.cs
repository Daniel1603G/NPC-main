using UnityEngine;


public class WalkingPlayerState : IState
{
    private readonly PlayerController controller;
    
    public WalkingPlayerState(PlayerController controller)
    {
        this.controller = controller;
    }
    
    public void Enter()
    {
     
    }
    
    public void Execute()
    {
        
        Vector2 input = controller.GetMovementInput();
        
       
        if (!controller.HasMovementInput())
        {
            controller.ChangeState(controller.IdleStateInstance);
            return;
        }
        
       
        if (controller.Controller.isGrounded && controller.IsJumpKeyPressed())
        {
            controller.ChangeState(controller.JumpingStateInstance);
            return;
        }
        
        
        if (controller.IsSprintKeyPressed() && controller.Movement.CanSprint())
        {
            controller.ChangeState(controller.SprintingStateInstance);
            return;
        }
        
     
        Vector3 moveDir = controller.transform.right * input.x + controller.transform.forward * input.y;
        controller.Movement.Move(moveDir, false); // false = no sprint
       
        controller.Movement.ApplyGravity();
    }
    
    public void Exit()
    {
        // No hay cleanup necesario
    }
}