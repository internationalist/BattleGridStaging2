using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class HealGunState : BaseState
{
    public override void EnterState(BaseFSMController controller)
    {
        command = (HealGunFSM)controller;
        command.anim.SetTrigger("Beam_Shot");
        attackComplete = false;
        fireCounter = 0;
        command.playerController.OnAnimationComplete += OnComplete;
    }

    protected bool attackComplete;
    protected HealGunFSM command;
    protected int fireCounter;

    public override void Update(BaseFSMController controller)
    {
        if (attackComplete)
        {
            command.anim.ResetTrigger("Beam_Shot");
            command.TransitionToState(command.StateMap[Command.InternalState.idle.ToString()]);
            command.isActivated = false;
            command.playerController.OnAnimationComplete -= OnComplete;
            command.complete = true;
        }
    }

    public override void ExitState(BaseFSMController controller)
    {
        base.ExitState(controller);
    }

    /// <summary>
    /// The rifle attack animation is a looping one that auto repeats. In animation loop the OnAnimationComplete event is called.
    /// This eventually calls this method. We use the maxBurstFire variable to terminate the loop.
    /// Since the looping animation is handled in Unity and this loop can be multi-threaded, the update to the maxBurstFire variable
    /// can become dirty if not thread synchronized. For this reason we put this update inside an object lock.
    /// </summary>
    /// <param name="name"></param>

    public void OnComplete(string name)
    {
        if ("heal".Equals(name))
        {
            lock (this) //Thread synchronize.
            {
                ++fireCounter;
                HealGunCommand wt = (HealGunCommand)command.commandTemplate;
                if (wt.maxBurstFire == fireCounter)
                {
                    attackComplete = true;
                }
                wt.DeActivateEffects(command, command.commandDataInstance);
            }
        }
    }
}
