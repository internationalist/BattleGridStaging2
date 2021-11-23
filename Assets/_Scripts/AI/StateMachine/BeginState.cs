using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginState : AIActionState
{
    public override void EnterState(AIStateMachine aiMachine)
    {
        //Debug.LogFormat("{0} BeginState:EnterState->Start AI decision machine", aiMachine._controller.name);
        aim = aiMachine;
    }

    public override void Update()
    {
        if(aim._controller.turnActive)
        {
            //Debug.LogFormat("{0} BeginState:Update->turn active", aim._controller.name);
            //aim.isCommandRunning = true;
            if(aim._aiState.target == null)
            {
                aim.TransitionToState(aim.states["findtarget"]);
            } else if(aim._aiState.weaponTemplate == null)
            {
                aim.TransitionToState(aim.states["selectaction"]);
            } else
            {
                switch(aim._aiState.cmdType)
                {
                    case Command.type.primaryattack:
                        aim.TransitionToState(aim.states["action"]);
                        break;
                }
            }
        } 
    }

 
}
