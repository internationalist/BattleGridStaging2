using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseFSMController
{

    public BaseState currentState;
    public BaseState defaultState;


    public Dictionary<string, BaseState> StateMap { get => stateMap; set => stateMap = value; }

    private Dictionary<string, BaseState> stateMap;

    protected BaseFSMController()
    {
        this.StateMap = new Dictionary<string, BaseState>();
    }



    public virtual void Update()
    {
        currentState.Update(this);
    }

    public void TransitionToState(BaseState state)
    {
        if(currentState != null)
        {
            currentState.ExitState(this);
        }
        currentState = state;
        currentState.EnterState(this);
    }
}
