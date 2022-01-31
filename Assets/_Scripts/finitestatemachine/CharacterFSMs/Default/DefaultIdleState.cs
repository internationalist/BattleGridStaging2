using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultIdleState : BaseState
{
    PlayerController pc;
    bool coverAnimComplete;
    Vector3 previousEnemyPosition = Vector3.zero;
    Vector3 previousClosestCoverPosition = Vector3.zero;
    private bool enemyDetectionBusy = false;

    private enum CoverAnimType { LEFT, RIGHT }
    public override void EnterState(BaseFSMController controller)
    {
        IdleFSM command = (IdleFSM)controller;
        pc = command.playerController;
        pc.OnDamage += TakeDamage;
        command.anim.SetTrigger("Idle");
    }

    public override void Update(BaseFSMController controller)
    {   
        IdleFSM cmd = (IdleFSM)controller;
        if (pc.isDead)
        {
            //Debug.Log("Character is dead moving to death state");
            ResetIdleAnims(cmd);
            cmd.TransitionToState(cmd.StateMap[Command.InternalState.death.ToString()]);
        }
        else if (takeDamage)
        {
            ResetIdleAnims(cmd);
            if (damageAmt < 40)
            {
                cmd.TransitionToState(cmd.StateMap[Command.InternalState.lightdamage.ToString()]);
            }
            else
            {
                cmd.TransitionToState(cmd.StateMap[Command.InternalState.heavydamage.ToString()]);
            }
            takeDamage = false;
            pc.OnDamage -= TakeDamage;
        } else
        {
            if(!enemyDetectionBusy)
            {
                pc.StartCoroutine(DetectEnemy(cmd));
            }
        }
    }

    private IEnumerator DetectEnemy(IdleFSM cmd)
    {
        enemyDetectionBusy = true;
        yield return new WaitForSeconds(.3f);
        Vector3 enemyPosition;
        if (DetectEnemyCloseby(cmd, out enemyPosition))
        {
            TurnToEnemy(cmd, enemyPosition);
        }
        else
        {
            AnimatorStateInfo currentAnimState = cmd.anim.GetCurrentAnimatorStateInfo(0);
            if (currentAnimState.IsName("Idle") && cmd.playerController.InCover
                && !transitioning)
            {
                ActivateCoverAnimation(cmd);
            }
            else if (!currentAnimState.IsName("Idle"))
            {
                transitioning = false;
            }
        }
        enemyDetectionBusy = false;
    }

    private static void ResetIdleAnims(IdleFSM cmd)
    {
        cmd.anim.ResetTrigger("Idle");
        cmd.anim.ResetTrigger("Alert_Idle");
        cmd.anim.ResetTrigger("Crouch_Idle");
        cmd.anim.ResetTrigger("LoCoverL");
        cmd.anim.ResetTrigger("LoCoverR");
    }

    private void TurnToEnemy(IdleFSM command, Vector3 enemyPosition)
    {
        //Debug.LogFormat("{0}::Enemy {1} detected close by and turn to enemy is {2}", pc.name, enemyPosition, command.turnToEnemy);
        //Debug.LogFormat("{0}::Enemy position same as previous position:{1}", pc.name, enemyPosition.Equals(previousEnemyPosition));
        bool enemyPositionChange = !previousEnemyPosition.Equals(enemyPosition);
        previousEnemyPosition = enemyPosition;
        
        if (enemyPositionChange)
        {
            command.turnToEnemy = true;
            Command.InternalState turnDir = GeneralUtils.GetTurnDirection(command.playerTransform,
                                                                    enemyPosition,
                                                                    out command.targetDirection);
            command.TransitionToState(command.StateMap[turnDir.ToString()]);
        }
        else
        {
            if (pc.InCover)
            {
                if (CoverFramework.TYPE.full.Equals(pc.cover.coverType))
                {
                    GeneralUtils.SetAnimationTrigger(command.anim, "Alert_Idle");
                }
                else
                {
                    GeneralUtils.SetAnimationTrigger(command.anim, "Crouch_Idle");
                }
            }
            else
            {
                GeneralUtils.SetAnimationTrigger(command.anim, "Alert_Idle");
            }
        }
    }

    private void ActivateCoverAnimation(IdleFSM command)
    {
        Vector3 closestCoverPoint = pc.cover.coverCollider.ClosestPoint(command.playerController.transform.position);
        bool coverChange = !previousClosestCoverPosition.Equals(closestCoverPoint);
        previousClosestCoverPosition = closestCoverPoint;
        if(coverChange)
        {
            Command.InternalState turnDir = GeneralUtils.GetTurnDirection(command.playerTransform,
                                                                    closestCoverPoint,
                                                                    out command.targetDirection);
            command.TransitionToState(command.StateMap[turnDir.ToString()]);
        } else
        {
            transitioning = true;
            CoverAnimType coverAnimType = CalculateCoverAnimOrientation();

            if (CoverFramework.TYPE.full.Equals(pc.cover.coverType))
            {
                switch (coverAnimType)
                {
                    case CoverAnimType.LEFT:
                        command.anim.SetTrigger("HiCoverL");
                        break;
                    case CoverAnimType.RIGHT:
                        command.anim.SetTrigger("HiCoverR");
                        break;
                }
            }
            else
            {
                /*
                 * Unlike the high cover animation the low cover animation is made up of two distinct animation states run one after the other.
                 * This means if the command is reset because of user right-click the idle command runs again and the animation state is
                 * re-evaluated. This results in a weird movement of the character. For this we check the current state of the animation
                 * and if the character is already in a low cover state we keep it that way.
                */
                AnimatorStateInfo currentAnimState = command.anim.GetCurrentAnimatorStateInfo(0);
                if (!currentAnimState.IsName("LoCoverL_2") && !currentAnimState.IsName("LoCoverR_2"))
                {
                    switch (coverAnimType)
                    {
                        case CoverAnimType.LEFT:
                            command.anim.SetTrigger("LoCoverL");
                            break;
                        case CoverAnimType.RIGHT:
                            command.anim.SetTrigger("LoCoverR");
                            break;
                    }
                }
            }
            coverAnimComplete = true;
        }
    }

    private bool DetectEnemyCloseby(Command command, out Vector3 enemyPos)
    {
        float distanceToClosestEnemy = float.MaxValue;
        PlayerController closest = null;
        enemyPos = Vector3.zero;

        if(TurnBasedSystem.I != null
            && TurnBasedSystem.I.teams != null
            && TurnBasedSystem.I.teams.Count > 0)
        {
            foreach (Team t in TurnBasedSystem.I.teams)
            {
                if (!t.teamID.Equals(command.playerController.teamID))
                {
                    closest = GeneralUtils.FindClosest(command.playerController,
                                             t,
                                             out distanceToClosestEnemy);
                    if (closest != null)
                    {
                        break;
                    }
                }
            }
        }

        if(closest == null)
        {
            return false;
        }
        //Debug.LogFormat("{0} Closest enemy is {1} running command {2}", pc.name, closest.name, closest.CurrentCommand.ToString());
        Command.type enemyCommand = closest.getCurrentCommand().commandType;
        enemyPos = closest.transform.position;

        //if (distanceToClosestEnemy <= 5 && !enemyCommand.Equals(Command.type.move))
        if (!enemyCommand.Equals(Command.type.move))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// <para>When triggering the cover animation, the character can face either right or left depending upon which edge of cover is closer.
    /// This would involve triggering either one animation or the other.</para><para>This is determined by getting the nearest corner of the cover
    /// and then performing a dot product with the character position to determine if the corner is to the left or right of the character.</para>
    /// </summary>
    /// <returns></returns>
    private CoverAnimType CalculateCoverAnimOrientation()
    {
        CoverAnimType coverAnimType;
        Vector3? closetCorner = null;
        float distance = float.MaxValue;

        foreach (Vector3 corner in pc.cover.cornerPoints)
        {
            float thisdist = Vector3.Distance(pc.transform.position, corner);
            if (thisdist <= distance)
            {
                distance = thisdist;
                closetCorner = corner;
            }
        }

        Vector3 dirToCorner = (closetCorner.Value - pc.transform.position).normalized;

        float dotResult = Vector3.Dot(pc.transform.right, dirToCorner);
        if (dotResult <= 0)
        {
            coverAnimType = CoverAnimType.LEFT;
        }
        else
        {
            coverAnimType = CoverAnimType.RIGHT;
        }

        return coverAnimType;
    }

    bool takeDamage;
    float damageAmt;
    bool dead;
    bool transitioning = false;

    void TakeDamage(float damageAmt)
    {
        this.damageAmt = damageAmt;
        takeDamage = true;
    }
}
