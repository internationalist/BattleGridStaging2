using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class AIStateMachine : MonoBehaviour
{

    private void Start()
    {
        states = new Dictionary<string, AIActionState>();
        states["start"] = new BeginState();
        states["findtarget"] = new FindTarget();
        states["selectaction"] = new SelectAction();
        states["end"] = new EndState();
        states["action"] = (AIActionState)ScriptableObject.CreateInstance(attackState.GetType());
        
        states["findtarget"].next = states["start"];
        states["selectaction"].next = states["start"];

        _controller = GetComponent<PlayerController>();
        InitState();
        defaultState = states["start"];
        currentState = defaultState;
        currentState.EnterState(this);
        decisionMatrix = new List<Factor>();
        decisionMatrix.Add(new CoverFactor("Cover", 2));
        decisionMatrix.Add(new RangeFactor("Range", 1));
        decisionMatrix.Add(new HealthFactor("Health", 1));
    }

    public void InitState()
    {
        _aiState = new AIState(_controller);
    }


    public Dictionary<string, AIActionState> states;

    public AIActionState attackState;

    public PlayerController _controller;

    protected AIActionState currentState;

    public AIActionState defaultState;

    public AIState _aiState;

    public bool isCommandRunning;

    public RangedAttackState.Agression agressionLevel;

    public List<Factor> decisionMatrix;

    [Range(0,1)]
    public float specialAttackChance;

    public AIActionState CurrentState
    {
        get => currentState;
        set => currentState = value;
    }

    public virtual void Update()
    {
        currentState.Update();
    }

    public void TransitionToState(AIActionState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }
    [System.Serializable]
    public class StateContainer
    {
        public StateContainer(string name, AIActionState action)
        {
            this.name = name;
            this.aiActionState = action;
        }
        public string name;
        public AIActionState aiActionState;
    }

    public void DiscardTargetAndBeginAILoop()
    {
        _aiState.targets.Remove(_aiState.target);
        _aiState.target = null;
        TransitionToState(defaultState);
    }
}
