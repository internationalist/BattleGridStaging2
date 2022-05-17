using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDamageState : BaseState
{
    PlayerController pc = null;
    Command command;
    public override void EnterState(BaseFSMController controller)
    {
        lock (this) // For damage events from multiple threads, we have to use synchronization
        {
            command = (Command)controller;
            command.anim.CrossFade("LightDamage", 0.2f);
            complete = false;
            command.playerController.OnAnimationComplete += OnComplete;
            pc = command.playerController;
        }
    }

    protected bool complete;

    public override void Update(BaseFSMController controller)
    {

    }

    public void OnComplete(string name)
    {
        lock (this) // For damage events from multiple threads, we have to use synchronization
        {
            if ("damage".Equals(name))
            {   
                command.playerController.OnAnimationComplete -= OnComplete;
            }
        }
    }
}
