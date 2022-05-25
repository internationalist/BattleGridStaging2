using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowItemState : BaseState
{
    bool complete;
    ThrowItemFSM command;

    //placeholder variable to track time will be removed when implementation is complete.
    float timeStarted;
    public override void EnterState(BaseFSMController controller)
    {
        command = (ThrowItemFSM)controller;
        command.anim.SetTrigger("grenade_throw");
        timeStarted = Time.time;
        command.playerController.OnAnimationComplete += OnComplete;
    }

    public override void Update(BaseFSMController controller)
    {
        
        //float timeElapsed = Time.time - timeStarted;
        //if (timeElapsed >= 1)
        if(command.complete)
        {
            command.playerController.OnAnimationComplete -= OnComplete;
            command.anim.ResetTrigger("grenade_throw");
            command.TransitionToState(command.StateMap[Command.InternalState.idle.ToString()]);
        }
    }

    public void OnComplete(string name)
    {
        if ("grenade".Equals(name))
        {
            lock (this) //Thread synchronize.
            {
                command.complete = true;
            }
        }
    }
}
