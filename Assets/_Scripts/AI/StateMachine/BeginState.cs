using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginState : AIActionState
{
    public override void EnterState(AIStateMachine aiMachine)
    {
        aim = aiMachine;
    }

    public override void Update()
    {
        if(aim._controller.turnActive)
        {
            if(aim._aiState.targets.Count == 0) //All targets have been exhausted. End turn
            {
                aim.TransitionToState(aim.states["end"]);
            } else if(aim._aiState.target == null)
            {
                aim.TransitionToState(aim.states["findtarget"]);
            } else if(aim._aiState.weaponTemplate == null)
            {
                aim.TransitionToState(aim.states["selectaction"]);
            } else
            {
                aim.TransitionToState(aim.states["action"]);
            }
        } 
    }

 
}
