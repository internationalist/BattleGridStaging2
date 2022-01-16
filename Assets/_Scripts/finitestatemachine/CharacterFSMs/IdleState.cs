using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseState
{

    public override void EnterState(BaseFSMController player)
    {
        //Debug.LogFormat("IdleState.EnterState->Set trigger Idle");
        Command characterPlayer = (Command)player;
        //characterPlayer.anim.CrossFade("Idle", 0.2f);
        characterPlayer.anim.SetTrigger("Idle");
    }

    public override void Update(BaseFSMController controller)
    {
        Command command = (Command)controller;
        if(command.isActivated)
        {
            //Debug.LogFormat("{0}IdleState.Update::cmd is activated", command.playerController.name);
            if (command.Destination.HasValue)
            {
                //Debug.Log("Idle::dest has value");

                Command.InternalState turnDir = GeneralUtils.GetTurnDirection(command.playerTransform,
                                                                        command.Destination.Value,
                                                                        out command.targetDirection);
                //Debug.LogFormat("{0}IdleState.Update::going to reset Idle trigger", command.playerController.name);
                command.anim.ResetTrigger("Idle");
                //Debug.LogFormat("{0}IdleState.Update::Transitioning to {1}", command.playerController.name, turnDir.ToString());
                command.TransitionToState(command.StateMap[turnDir.ToString()]);
            }
        }
    }

}
