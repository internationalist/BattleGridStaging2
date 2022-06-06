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
    float turnSpeed = 4f;
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
        startTime = Time.time;
        Command command = (Command)player;
        if(command.targetDirection.Equals(Vector3.zero))
        {
            command.targetDirection = command.playerTransform.forward;
        }
        toRotation = Quaternion.LookRotation(command.targetDirection);
        fromRotation = command.playerTransform.rotation;
        timeCount = 0;
        pc = command.playerController;
        switch (turnDir)
        {
            case Dir.left:
                ActivateTurnAnimation(command, "Turn_Left", "Crouched_Turn_L");
                //GeneralUtils.SetAnimationTrigger(command.anim, ("Turn_Left"));
                break;
            case Dir.right:
                ActivateTurnAnimation(command, "Turn_Right", "Crouched_Turn_R");
                //GeneralUtils.SetAnimationTrigger(command.anim, ("Turn_Right"));
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
    }

    private void ActivateTurnAnimation(Command command, string defaultAnim, string crouchedAnim)
    {
        PlayerController pc = command.playerController;
        if (pc.InCover)
        {
            if (CoverFramework.TYPE.full.Equals(pc.cover.coverType))
            {
                GeneralUtils.SetAnimationTrigger(command.anim, defaultAnim);
            }
            else
            {
                GeneralUtils.SetAnimationTrigger(command.anim, crouchedAnim);
            }
        }
        else
        {
            GeneralUtils.SetAnimationTrigger(command.anim, defaultAnim);
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
