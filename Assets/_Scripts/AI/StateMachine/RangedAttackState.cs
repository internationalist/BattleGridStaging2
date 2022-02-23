using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RangedAttackState", menuName = "AI/RangedAI", order = 1)]
public class RangedAttackState : AIActionState
{

    public enum Agression { HIGH, MODERATEA, MODERATEB, LOW }

    public Agression agressionLevel;

    //bool aggBeforeAttack, aggAfterAttack;
    float camp, rush;

    protected bool isRunning;
    protected CoverFramework cf;
    private float commandStartTimeInSec=-1f;
    private float commandTimeOutInSecs=15;

    public override void EnterState(AIStateMachine aiMachine)
    {
        aim = aiMachine;
        Initialize();
        //Debug.LogFormat("{0} BeginState:AIAttackState->Start AI decision machine", aiMachine._controller.name);
    }

    private void Initialize()
    {
        this.agressionLevel = aim.agressionLevel;
        switch (this.agressionLevel)
        {
            case Agression.HIGH:
                rush = .9f;
                camp = .1f;
                break;
            case Agression.MODERATEA:
                rush = .5f;
                camp = .2f;
                break;
            case Agression.MODERATEB:
                rush = .5f;
                camp = .3f;
                break;
            case Agression.LOW:
                rush = 0;
                camp = .5f;
                break;
        }
    }

    public override void Update()
    {

        if(commandStartTimeInSec != -1  //Check if command has been running past the timeout specified.
           && Time.realtimeSinceStartup - commandStartTimeInSec > commandTimeOutInSecs)
        {
            isRunning = false; //terminate command.
            commandStartTimeInSec = -1f;
        }

        if (aim._controller.turnActive && !isRunning)
        {
            if(aim._controller.playerMetaData.CanRunCommand())
            {
                if (Command.type.primaryaction.Equals(aim._aiState.attackType))
                {
                    NewAILogic();
                }
                else if (Command.type.specialaction.Equals(aim._aiState.attackType))
                {
                    ThrowItem();
                }
            } else
            {
                aim.TransitionToState(aim.states["end"]); // End turn
            }

        }
    }

    protected virtual void NewAILogic()
    {
        AIUtils.DirectionAndDistanceToTarget(aim._aiState, aim._aiState.agent);
        //Debug.LogFormat("{0} AIAttackState:Update->turn active", aim._controller.name);
        if (aim._aiState.target == null) //Enemy has already died, start over turn
        {
            aim.TransitionToState(aim.states["start"]);
        } else if(aim._controller.playerMetaData.CanAttack() && !aim._aiState.weaponInstance.isAmmoLeft())
        {
            aim._aiState.cmdType = Command.type.reload;
            TriggerCommand(aim._aiState, aim._controller);
        }
        else if (aim._controller.playerMetaData.CanAttack())
        {
            if ((cf = isEnemyBehindHighCover()) != null)
            {
                if (aim._controller.playerMetaData.CanMove())
                {
                    //Intelligently flank enemy if they are behind high cover. Otherwise rush them.
                    if(!AIUtils.FlankEnemyFromCover(aim._aiState, aim._controller, this.agressionLevel))
                    {
                        AIUtils.RushEnemy(aim._aiState, aim._controller, cf);
                    }
                    TriggerCommand(aim._aiState, aim._controller);
                }
                else
                {
                    //Discard this target and start from begin of AI loop
                    aim.DiscardTargetAndBeginAILoop();
                }
            }
            else if (Mathf.Round(aim._aiState.distanceToTarget) > aim._aiState.weaponTemplate.damageParameters.optimalRange) //Attack command is out of range
            {
                if (aim._controller.playerMetaData.CanMove())
                {
                    if(!MoveToAttackingPosition(this.agressionLevel))
                    {//did not find any cover to shoot from. Just move as close as possible to optimal range.
                        AIUtils.ApproachEnemy(aim._aiState, aim._controller);
                    }
                    TriggerCommand(aim._aiState, aim._controller);
                }
                else //Enemy might not be in optimal range and move command spent.
                {
                    //just shoot
                    aim._aiState.cmdType = Command.type.primaryaction;
                    TriggerCommand(aim._aiState, aim._controller);
                }
            }
            else //Enemy in optimal range.
            {
                //shoot
                aim._aiState.cmdType = Command.type.primaryaction;
                TriggerCommand(aim._aiState, aim._controller);
            }
        }
        else if (aim._controller.playerMetaData.CanMove()
                                    && !aim._aiState.achievedCover) //Attack command used up but can still move. 
        {
            if(aim._aiState.agent.InCover) //already in cover
            {
                string coverName = aim._aiState.agent.cover.name;
                if(GeneralUtils.CheckCoverBetweenPointsByName(aim._aiState.agent.transform.position, aim._aiState.target.transform.position, coverName))
                {
                    float effectiveCamp = camp + rush;
                    float chance = Random.value;
                    /**
                     * Run a probability check between three options:
                     * 
                     *  ++ Camp here since already in cover.
                     *  ++ Attempt to move to an attacking position.
                     *  ++ Attempt tp retreat to a defensive position.
                     * 
                    **/
                    if(chance <= rush)
                    {
                        MoveToAttackingPosition(this.agressionLevel);
                    } else if (chance < effectiveCamp)
                    {
                        //End turn. Camp here
                        aim.TransitionToState(aim.states["end"]);
                    } else
                    {
                        Retreat();
                    }
                } else //In cover but not aligned to enemy. In other words, enemy has clear shot.
                {
                    //Escape to cover.
                    Retreat();
                }
            } else
            {
                //Escape to cover.
                Retreat();
            }
            
            TriggerCommand(aim._aiState, aim._controller);
        }
        else // End turn
        {
            aim.TransitionToState(aim.states["end"]);
        }
    }

    protected virtual void ThrowItem()
    {
        AIUtils.DirectionAndDistanceToTarget(aim._aiState, aim._aiState.agent);
        //Debug.LogFormat("{0} AIAttackState:Update->turn active", aim._controller.name);
        if (aim._aiState.target == null) //Enemy has already died, start over turn
        {
            aim.TransitionToState(aim.states["start"]);
        } else if (aim._controller.playerMetaData.CanUseItem())
        {
            if ((cf = isEnemyBehindHighCover()) != null)
            {
                if (aim._controller.playerMetaData.CanMove())
                {
                    //Intelligently flank enemy if they are behind high cover. Otherwise rush them.
                    if (!AIUtils.FlankEnemyFromCover(aim._aiState, aim._controller, this.agressionLevel))
                    {
                        AIUtils.RushEnemy(aim._aiState, aim._controller, cf);
                    }
                    TriggerCommand(aim._aiState, aim._controller);
                }
                else
                {
                    //Discard this target and start from begin of AI loop
                    aim.DiscardTargetAndBeginAILoop();
                }
            }
            else if (Mathf.Round(aim._aiState.distanceToTarget) > aim._aiState.weaponTemplate.damageParameters.optimalRange) //Attack command is out of range
            {
                if (aim._controller.playerMetaData.CanMove())
                {
                    if (!MoveToAttackingPosition(this.agressionLevel))
                    {//did not find any cover to shoot from. Just move as close as possible to optimal range.
                        AIUtils.ApproachEnemy(aim._aiState, aim._controller);
                    }
                    TriggerCommand(aim._aiState, aim._controller);
                }
                else //Enemy might not be in optimal range and move command spent.
                {
                    //just shoot
                    /*aim._aiState.cmdType = Command.type.primaryaction;
                    TriggerCommand(aim._aiState, aim._controller);*/
                    aim.DiscardTargetAndBeginAILoop();
                }
            }
            else //Enemy in optimal range.
            {
                //throw item
                aim._aiState.cmdType = Command.type.specialaction;
                TriggerCommand(aim._aiState, aim._controller);
            }
        } else if(aim._controller.playerMetaData.CanAttack())
        {
            aim.DiscardTargetAndBeginAILoop();

        } else // End turn
        {
            aim.TransitionToState(aim.states["end"]);
        }
    }

    private void PerformRetreat()
    {
        if (aim._aiState.agent.InCover) //already in cover
        {
            string coverName = aim._aiState.agent.cover.name;
            if (GeneralUtils.CheckCoverBetweenPointsByName(aim._aiState.agent.transform.position, aim._aiState.target.transform.position, coverName))
            {
                float effectiveCamp = camp + rush;
                float chance = Random.value;
                /**
                 * Run a probability check between three options:
                 * 
                 *  ++ Camp here since already in cover.
                 *  ++ Attempt to move to an attacking position.
                 *  ++ Attempt tp retreat to a defensive position.
                 * 
                **/
                if (chance <= rush)
                {
                    MoveToAttackingPosition(this.agressionLevel);
                }
                else if (chance < effectiveCamp)
                {
                    //End turn. Camp here
                    aim.TransitionToState(aim.states["end"]);
                }
                else
                {
                    Retreat();
                }
            }
            else //In cover but not aligned to enemy. In other words, enemy has clear shot.
            {
                //Escape to cover.
                Retreat();
            }
        }
        else
        {
            //Escape to cover.
            Retreat();
        }
    }

    protected void Retreat()
    {
        //Debug.LogFormat("AP still left is {0}", aim._controller.playerMetaData.PointsRemaining());
        aim._aiState.cmdType = Command.type.move;
        AIUtils.Retreat(aim._aiState, aim._controller);
    }


   protected bool MoveToAttackingPosition(Agression aggressive)
    {
        aim._aiState.cmdType = Command.type.move;
        return AIUtils.FlankEnemyFromCover(aim._aiState, aim._controller, aggressive);
    }

    protected CoverFramework isEnemyBehindHighCover()
    {
        //Debug.Log("Enemy target is " + aim._aiState.target);
        CoverFramework cf = GeneralUtils.GetHighCoverBetweenPoints(aim._controller.transform.position, aim._aiState.target.transform.position);
        return cf;
    }

    /// <summary>
    /// For AI: Slot 0 for reload, 1 for move and 2 for attack.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="agent"></param>
    protected void TriggerCommand(AIState state, PlayerController agent)
    {   
        commandStartTimeInSec = Time.realtimeSinceStartup; // We will enforce a timeout on the command due to a vague problem of commands not finishing.

        isRunning = true;
        switch (state.cmdType)
        {
            case Command.type.move:
                GameManager.AssignCommand(GeneralUtils.MOVESLOT);
                //Debug.LogFormat("{0} AIAttackState:TriggerCommand->Running move command", aim._controller.name);
                //Debug.Log("TriggerCommand::Running move command");
                Command cmd = GameManager.ActivateCommand(null, state.moveLocation, () =>
                {
                    //Debug.LogFormat("{0} AIAttackState:TriggerCommand->Command done", aim._controller.name);
                    AIUtils.DirectionAndDistanceToLocation(state, agent);
                    isRunning = false;
                });
                break;
            case Command.type.primaryaction:
                GameManager.AssignCommand(GeneralUtils.ATTACKSLOT);
                //Debug.Log("TriggerCommand::Running attack command and  setting attack achieved flag to true");
                state.achievedAttack = true;
                GameManager.ActivateCommand(state.target.transform, state.target.transform.position, () =>
                {
                    isRunning = false;
                });
                break;
            case Command.type.specialaction:
                GameManager.AssignCommand(GeneralUtils.ITEMSLOT);
                //Debug.Log("TriggerCommand::Running special attack command and  setting attack achieved flag to true");
                state.achievedAttack = true;
                GameManager.ActivateCommand(state.target.transform, state.target.transform.position, () =>
                {
                    isRunning = false;
                });
                break;
            case Command.type.reload:
                GameManager.AssignCommand(GeneralUtils.RELOADSLOT);
                GameManager.ActivateCommand(null, null, () =>
                {
                    //Debug.LogFormat("{0} AIAttackState:TriggerCommand->Command done", aim._controller.name);
                    isRunning = false;
                });
                break;
        }
    }
}
