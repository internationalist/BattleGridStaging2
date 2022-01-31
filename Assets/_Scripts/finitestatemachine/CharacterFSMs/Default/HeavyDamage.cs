using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyDamage : LightDamageState
{
    public override void EnterState(BaseFSMController controller)
    {
        command = (Command)controller;
        command.anim.CrossFade("front_damage", 0.2f);
        complete = false;
        command.playerController.OnAnimationComplete += OnComplete;
    }
}
