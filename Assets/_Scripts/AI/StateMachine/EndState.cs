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
        //if(aim._controller.turnActive)
        //{
            aim._controller.EndTurn();
            aim._aiState.InitForNewTurn();
            aim.TransitionToState(aim.defaultState);
        //} 
    }

 
}
