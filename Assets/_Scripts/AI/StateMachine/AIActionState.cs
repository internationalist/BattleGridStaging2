using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIActionState : ScriptableObject
{
    protected AIStateMachine aim;

    public AIActionState next;

    protected float commandStartTimeInSec;

    public float commandTimeOutInSecs = 15f;
    public virtual void EnterState(AIStateMachine aiMachine) { }

    public virtual void Update() { }

    /*protected void DiscardTargetAndBeginAILoop()
    {
        aim._aiState.targets.Remove(aim._aiState.target);
        aim._aiState.target = null;
        aim.TransitionToState(aim.defaultState);
    }*/
}
