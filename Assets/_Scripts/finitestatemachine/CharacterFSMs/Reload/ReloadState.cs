using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadState : BaseState
{
    public override void EnterState(BaseFSMController controller)
    {
        command = (ReloadFSM)controller;
        if(command.playerController.inCover)
        {
            AnimatorStateInfo currentAnimState = command.anim.GetCurrentAnimatorStateInfo(0);
            if (command.playerController.cover.coverType.Equals(CoverFramework.TYPE.full))
            {
                if (currentAnimState.IsName("HiCover_L"))
                {
                    command.anim.SetTrigger("Cover_ReloadL");
                } else if(currentAnimState.IsName("Alert_Idle"))
                {
                    command.anim.SetTrigger("Reload");
                }
                else
                {
                    command.anim.SetTrigger("Cover_ReloadR");
                }
            } else
            {
                if (currentAnimState.IsName("LoCoverL_2"))
                {
                    command.anim.SetTrigger("LoCover_ReloadL");
                }
                else
                {
                    command.anim.SetTrigger("LoCover_ReloadR");
                }
            }
        } else
        {
            command.anim.SetTrigger("Reload");
        }
        float yDisplacement = 2.5f;
        if(command.playerController.inCover
            && CoverFramework.TYPE.half.Equals(command.playerController.cover.coverType))
        {
            yDisplacement -= .7F;
        }
        UIManager.Talk("!!Reloading!!", command.playerController.transform.position + (Vector3.up * yDisplacement) + (Vector3.left * -1f));
        reloadComplete = false;
        PlayerController.OnAnimationComplete += OnComplete;
    }

    bool reloadComplete;
    ReloadFSM command;

    public override void Update(BaseFSMController controller)
    {
        //Debug.Log("{0}AttackState.Update::Running");
        if (reloadComplete) {
            command.anim.ResetTrigger("Reload");
            command.anim.ResetTrigger("Crouch_Reload");
            command.isActivated = false;
            PlayerController.OnAnimationComplete -= OnComplete;
            command.complete = true;
        }
    }

    public void OnComplete(string name)
    {
        if ("reload".Equals(name)) 
        {
            command.playerController.TopUpAmmo();
            reloadComplete = true;
        }
    }
}
