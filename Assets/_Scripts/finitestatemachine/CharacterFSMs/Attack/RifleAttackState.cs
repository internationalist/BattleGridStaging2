using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class RifleAttackState : BaseState
{
    public override void EnterState(BaseFSMController controller)
    {
        command = (RifleAttackFSM)controller;
        if(command.actionCam)
        {
            Transform actionCamHook = command.playerController.actionCamHook;
            UIManager.MoveActioncam(actionCamHook);
            TriggerActionCam();
        }
        else
        {
            command.anim.SetTrigger("Single_Shot");
        }
        
        attackComplete = false;
        fireCounter = 0;
        PlayerController.OnAnimationComplete += OnComplete;
    }

    private void TriggerActionCam()
    {
        if (!UIManager.ActionCamObstructed(command.playerController.transform))
        {
            command.anim.SetTrigger("Idle");
            UIManager.FadeToBlack(command.playerController.actionCamHook,
                (Transform t) =>
                {//OnBlack
                    UIManager.ShowActionCam();
                },
                (Transform t) =>
                {//OnComplete
                    command.anim.SetTrigger("Single_Shot");
                });
        }
        else
        {
            command.anim.SetTrigger("Single_Shot");
        }
    }

    bool attackComplete;
    RifleAttackFSM command;
    int fireCounter;

    public override void Update(BaseFSMController controller)
    {
        if (attackComplete) {
            command.anim.ResetTrigger("Single_Shot");
            command.TransitionToState(command.StateMap[Command.state.idle.ToString()]);
            command.isActivated = false;
            PlayerController.OnAnimationComplete -= OnComplete;
            command.complete = true;
            command.playerController.StartCoroutine(HideActionCam());
        }
    }

    public override void ExitState(BaseFSMController controller)
    {
        base.ExitState(controller);
    }

    private IEnumerator HideActionCam()
    {
        yield return new WaitForSeconds(3f);
        UIManager.HideActionCam();
        Time.timeScale = 1;
    }

    public void OnComplete(string name)
    {
        if ("attack".Equals(name)) 
        {
            ++fireCounter;
            CommandTemplate wt = command.commandTemplate;
            if(wt.maxBurstFire == fireCounter)
            {
                attackComplete = true;
            }
        }
    }
}
