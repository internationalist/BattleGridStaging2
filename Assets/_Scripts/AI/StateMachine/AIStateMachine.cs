using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class AIStateMachine : MonoBehaviour
{

    #region State
    public Dictionary<string, AIActionState> states;

    public AIActionState attackState;

    public PlayerController _controller;

    protected AIActionState currentState;

    public AIActionState defaultState;

    public AIState _aiState;

    public bool isCommandRunning;

    public RangedAttackState.Agression agressionLevel;

    public List<Factor> decisionMatrix;

    public float postCommandPauseInSecs;

    [Range(0, 1)]
    public float specialAttackChance;

    #endregion


    #region Unity messages

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
        //InitState();
        TurnBasedSystem.enemySpawnComplete += EnemySpawnCompleteListener;
        defaultState = states["start"];
        currentState = defaultState;
        currentState.EnterState(this);
        decisionMatrix = new List<Factor>();
        decisionMatrix.Add(new CoverFactor("Cover", 2));
        decisionMatrix.Add(new RangeFactor("Range", 1));
        decisionMatrix.Add(new HealthFactor("Health", 1));
    }

    public virtual void Update()
    {
        if(!GameManager.I.levelComplete)
        {
            currentState.Update();
        }
    }

    #endregion

    #region AI State Machine methods
    public void InitState()
    {
        _aiState = new AIState(_controller);
    }

    public void EnemySpawnCompleteListener(string teamName)
    {
        InitState();
    }


    public void TransitionToState(AIActionState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    public void DiscardTargetAndBeginAILoop()
    {
        _aiState.targets.Remove(_aiState.target);
        _aiState.target = null;
        _aiState.weaponTemplate = null;
        TransitionToState(defaultState);
    }
    #endregion
}
