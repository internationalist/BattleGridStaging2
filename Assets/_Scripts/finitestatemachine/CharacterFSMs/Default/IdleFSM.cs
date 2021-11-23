using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IdleFSM : Command
{
    public bool turnToEnemy = false;
    public IdleFSM(Animator anim, NavMeshAgent nav, PlayerController controller, CommandTemplate weaponData) : base(anim, nav, controller, weaponData)
    {
        currentState = new DefaultIdleState();
        currentState.EnterState(this);
        StateMap.Add(state.idle.ToString(), this.currentState);
        StateMap.Add(state.lightdamage.ToString(), new LightDamageState());
        StateMap.Add(state.heavydamage.ToString(), new HeavyDamage());
        StateMap.Add(state.death.ToString(), new DeathState());
        StateMap.Add(state.turnLeft.ToString(), new TurnForMoveState(TurnForMoveState.Dir.left, state.idle));
        StateMap.Add(state.turnRight.ToString(), new TurnForMoveState(TurnForMoveState.Dir.right, state.idle));
        commandType = type.idle;
    }
    protected override void Activate(Transform enemyTransform, Vector3? destination)
    {
        base.Activate(enemyTransform, destination);
    }

    public override void ActivateWeapon()
    {
    }

    protected override void BeforeActivate()
    {
    }

    protected override void Activated()
    {
    }

    protected override bool Validate(Transform enemyTransform, Vector3? destination)
    {
        return true;
    }


    public override void Cancel()
    {       
        base.Cancel();
        turnToEnemy = false;
        anim.ResetTrigger("LoCoverL");
        anim.ResetTrigger("LoCoverR");
    }
}
