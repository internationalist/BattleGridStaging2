using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndState : AIActionState
{
    public override void EnterState(AIStateMachine aiMachine)
    {
        //Debug.LogFormat("{0} EndState:EnterState->Ending Command for AI decision machine", aiMachine._controller.name);
        aim = aiMachine;
    }

    public override void Update()
    {
        if(aim._controller.turnActive)
        {
            //Debug.LogFormat("{0} EndState:Update->Command Ending", aim._controller.name);
            aim._controller.EndTurn();
            aim._aiState.InitForCommand();
            aim.TransitionToState(aim.defaultState);
        } 
    }

 
}
