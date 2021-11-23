using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachState : BaseState
{
    public float normalizedSpeed;
    private MovementFSM command;
    public override void EnterState(BaseFSMController controller)
    {
        command = (MovementFSM)controller;
        command.anim.CrossFade("Idle", 0.2f);
        command.nav.SetDestination(command.Destination.Value);
    }

    public override void Update(BaseFSMController controller)
    {

        //normalizedSpeed = (command.nav.velocity.sqrMagnitude)/Mathf.Pow(command.nav.speed, 2);
        normalizedSpeed = Mathf.Clamp(command.nav.velocity.sqrMagnitude, 0, 1);
        command.anim.SetFloat("Blend", normalizedSpeed);
        if (!command.nav.pathPending)
        {
            if (command.nav.remainingDistance <= command.nav.stoppingDistance)
            {
                if (!command.nav.hasPath || command.nav.velocity.sqrMagnitude == 0f)
                {
                    command.TransitionToState(command.StateMap[Command.state.idle.ToString()]);
                    if (command.playerController.inCover)
                    {
                        //Debug.LogFormat("{0} Reached cover {1} of type {2}", command.playerController.name, command.playerController.cover.name, command.playerController.cover.coverType);
                    }
                    command.complete = true;
                }
            }
        }
    }
}
