using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTarget : AIActionState
{

    public override void EnterState(AIStateMachine aiMachine)
    {
        //Debug.LogFormat("{0} FindTarget:EnterState->AI finding target`", aiMachine._controller.name);
        aim = aiMachine;
    }

    public override void Update()
    {
        //if (aim._controller.turnActive)
        //{
            AIUtils.ChooseEnemyV2(aim._aiState, aim._controller, aim.decisionMatrix);
            aim.TransitionToState(next);
        //}
    }
}
