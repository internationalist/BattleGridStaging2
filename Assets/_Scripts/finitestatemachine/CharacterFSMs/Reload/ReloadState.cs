using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadState : BaseState
{
    public override void EnterState(BaseFSMController controller)
    {
        command = (ReloadFSM)controller;
        if(command.playerController.InCover)
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

        command.playerController.infoPanel.SetDialog("!!Reloading!!");
        command.playerController.StartCoroutine(command.playerController.infoPanel.ShowDialog());
        reloadComplete = false;
        command.playerController.OnAnimationComplete += OnComplete;
    }

    bool reloadComplete;
    ReloadFSM command;

    public override void Update(BaseFSMController controller)
    {
    }

    public void OnComplete(string name)
    {
        if ("reload".Equals(name)) 
        {
            command.playerController.TopUpAmmo();
            command.anim.ResetTrigger("Reload");
            command.anim.ResetTrigger("Crouch_Reload");
            command.isActivated = false;
            command.playerController.OnAnimationComplete -= OnComplete;
            if (command.onCompleteCallback != null)
            {
                command.onCompleteCallback();
                command.onCompleteCallback = null;
            }
            command.complete = true;
        }
    }
}
