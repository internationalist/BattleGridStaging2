using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemState : BaseState
{
    bool commandComplete;
    UseItemFSM command;
    public override void EnterState(BaseFSMController controller)
    {
        command = (UseItemFSM)controller;
        command.anim.SetTrigger("useitem");
        commandComplete = false;
        PlayerController.OnAnimationComplete += OnComplete;
    }

    public override void Update(BaseFSMController controller)
    {
        if(commandComplete)
        {
            command.anim.ResetTrigger("useitem");
            command.isActivated = false;
            PlayerController.OnAnimationComplete -= OnComplete;
            command.complete = true;
        }
    }

    public void OnComplete(string name)
    {
        if ("useitem".Equals(name))
        {
            commandComplete = true;
        }
    }
}
