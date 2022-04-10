using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnForMoveState : BaseState
{
    PlayerController pc;
    public enum Dir { left, right }
    public Dir turnDir;
    private Command.InternalState nextState;
    float startTime;
    float turnSpeed = 2f;
    Quaternion toRotation;
    Quaternion fromRotation;
    float timeCount;

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
        Debug.Log("Entering turn state");
        startTime = Time.time;
        Command command = (Command)player;
        toRotation = Quaternion.LookRotation(command.targetDirection);
        fromRotation = command.playerTransform.rotation;
        timeCount = 0;
        pc = command.playerController;
        switch (turnDir)
        {
            case Dir.left:
                GeneralUtils.SetAnimationTrigger(command.anim, ("Turn_Left"));
                break;
            case Dir.right:
                GeneralUtils.SetAnimationTrigger(command.anim, ("Turn_Right"));
                break;
        }
    }

    public override void Update(BaseFSMController player)
    {
        RotateV2(player);
    }

    private void RotateV2(BaseFSMController player)
    {
        float currentTime = Time.time;
        Command command = (Command)player;

        //float duration = (currentTime - startTime)/rotationDuration;
        timeCount += Time.deltaTime * turnSpeed;
        Debug.LogFormat("timecount is {0}", timeCount);

        if (timeCount >= .8f)
        {
            command.playerTransform.rotation = toRotation;
            command.anim.ResetTrigger("Turn_Left");
            command.anim.ResetTrigger("Turn_Right");
            player.TransitionToState(player.StateMap[nextState.ToString()]);
        } else
        {
            command.playerTransform.rotation = Quaternion.Slerp(command.playerTransform.rotation, toRotation, timeCount);
        }
        
        //Debug.LogFormat("Angle left is {0}", angleLeft);

        /* (timeCount >= 1)
        {
            command.anim.ResetTrigger("Turn_Left");
            command.anim.ResetTrigger("Turn_Right");
            player.TransitionToState(player.StateMap[nextState.ToString()]);
        }*/
    }

    private void Rotate(BaseFSMController player)
    {
        Command command = (Command)player;
        float distance = Vector3.Distance(command.playerTransform.position, command.Destination.Value);
        float angleLimit = 2;

        angleLimit = Mathf.Clamp(10f / distance, 2, 20);
        float stepWeight = 5.0f;
        float singleStep = stepWeight * Time.deltaTime;
        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(command.playerTransform.forward, command.targetDirection, singleStep, 0.0f);

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
