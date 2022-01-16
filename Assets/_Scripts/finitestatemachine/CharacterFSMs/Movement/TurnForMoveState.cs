using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnForMoveState : BaseState
{
    PlayerController pc;
    public enum Dir { left, right }
    public Dir turnDir;
    private Command.InternalState nextState;

    public TurnForMoveState(Dir turnDir)
    {
        this.turnDir = turnDir;
        nextState = Command.InternalState.approach;
    }

    public TurnForMoveState(Dir turnDir, Command.InternalState state)
    {
        this.turnDir = turnDir;
        nextState = state;
    }
    public override void EnterState(BaseFSMController player)
    {
        Command command = (Command)player;
        pc = command.playerController;
        switch (turnDir)
        {
            case Dir.left:
                GeneralUtils.SetAnimationTrigger(command.anim, ("Turn_Left"));
                //command.anim.SetTrigger("Turn_Left");
                break;
            case Dir.right:
                GeneralUtils.SetAnimationTrigger(command.anim, ("Turn_Right"));
                //command.anim.SetTrigger("Turn_Right");
                break;
        }
    }

    public override void Update(BaseFSMController player)
    {
        Command command = (Command)player;
        float distance = Vector3.Distance(command.playerTransform.position, command.Destination.Value);
        float angleLimit = 2;

        angleLimit = Mathf.Clamp(10f / distance, 2, 20);
        float stepWeight = 5.0f;
        float singleStep = stepWeight * Time.deltaTime;
        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(command.playerTransform.forward, command.targetDirection, singleStep, 0.0f);
        Debug.DrawRay(command.playerTransform.position, newDirection, Color.red);

        command.playerTransform.rotation = Quaternion.LookRotation(newDirection);

        float angleLeft = Vector3.Angle(command.playerTransform.forward, command.targetDirection);

        //Debug.LogFormat("Angle left is {0}", angleLeft);

        if (angleLeft <= angleLimit)
        {
            //Logger.Trace("TurnForMove.cs", "{0}-> Angle left is {1} Angle limit is {2}", command.playerController.name, angleLeft, angleLimit);
            command.anim.ResetTrigger("Turn_Left");
            command.anim.ResetTrigger("Turn_Right");
            player.TransitionToState(player.StateMap[nextState.ToString()]);
        }
    }

    public override void ExitState(BaseFSMController controller)
    {
        base.ExitState(controller);
        Command command = (Command)controller;
        command.anim.ResetTrigger("Turn_Left");
        command.anim.ResetTrigger("Turn_Right");
    }
}
