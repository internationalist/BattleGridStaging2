using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackState", menuName = "AI/MeleeAI", order = 1)]
public class MeleeAttackState : RangedAttackState
{
    protected override void NewAILogic()
    {
        AIUtils.DirectionAndDistanceToTarget(aim._aiState, aim._aiState.agent);
        if (aim._aiState.target == null) //Enemy has already died, start over turn
        {
            aim.TransitionToState(aim.states["start"]);
        }
        else if (aim._controller.playerMetaData.CanAttack() && !aim._aiState.weaponInstance.isAmmoLeft())
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
                    AIUtils.RushEnemy(aim._aiState, aim._controller, cf, true);
                    TriggerCommand(aim._aiState, aim._controller);
                }
                else
                {
                    aim.TransitionToState(aim.states["end"]);
                }
            }
            else if (Mathf.Round(aim._aiState.distanceToTarget) > aim._aiState.weaponTemplate.damageParameters.optimalRange) //Attack command is out of range
            {
                if (aim._controller.playerMetaData.CanMove())
                {
                    //check if target behind partial cover.
                    if(aim._aiState.target.inCover)
                    {
                        CoverFramework cf = aim._aiState.target.cover;
                        AIUtils.RushEnemy(aim._aiState, aim._controller, cf); //If enemy is in partial cover then flank them close.
                    } else
                    {
                        //if not in cover then simply approach
                        AIUtils.ApproachEnemy(aim._aiState, aim._controller);
                        
                    }
                    TriggerCommand(aim._aiState, aim._controller);
                }
                else //Enemy might not be in optimal range and move command spent.
                {
                    //just shoot
                    aim._aiState.cmdType = Command.type.primaryattack;
                    TriggerCommand(aim._aiState, aim._controller);
                }
            }
            else //Enemy in optimal range.
            {
                if(aim._controller.playerMetaData.CanMove())
                {
                    
                    if (aim._aiState.target.inCover) //if enemy in cover and can move then rush
                    {
                        CoverFramework cf = aim._aiState.target.cover;
                        AIUtils.RushEnemy(aim._aiState, aim._controller, cf, true);
                    } else //else just shoot
                    {
                        aim._aiState.cmdType = Command.type.primaryattack;
                    }
                    TriggerCommand(aim._aiState, aim._controller);
                } else 
                {
                    //if move command spent then just shoot.
                    aim._aiState.cmdType = Command.type.primaryattack;
                    TriggerCommand(aim._aiState, aim._controller);
                }
            }
        }
        else if (aim._controller.playerMetaData.CanMove()
                                    && !aim._aiState.achievedCover) //Attack command used up but can still move. 
        {
            Retreat();
            TriggerCommand(aim._aiState, aim._controller);
        } else
        {
            aim.TransitionToState(aim.states["end"]);
        }
    }
}
