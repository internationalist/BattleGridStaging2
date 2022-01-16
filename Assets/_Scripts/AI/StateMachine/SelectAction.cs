using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectAction : AIActionState
{
    public override void EnterState(AIStateMachine aiMachine)
    {
        aim = aiMachine;
    }

    public override void Update()
    {
        if (aim._controller.turnActive)
        {
            AIUtils.SelectAttack(aim);
            aim.TransitionToState(next);
        }
    }
}
