using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyDamage : LightDamageState
{
    public override void EnterState(BaseFSMController controller)
    {
        Command characterPlayer = (Command)controller;
        characterPlayer.anim.CrossFade("front_damage", 0.2f);
        complete = false;
        characterPlayer.playerController.OnAnimationComplete += OnComplete;
    }
}
