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
        fireCounter = 0;
        command.playerController.OnAnimationComplete += OnComplete;
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

    protected RifleAttackFSM command;
    protected int fireCounter;

    public override void Update(BaseFSMController controller)
    {
        /*if (command.complete) {
            command.anim.ResetTrigger("Single_Shot");
            command.TransitionToState(command.StateMap[Command.InternalState.idle.ToString()]);
            command.isActivated = false;
            command.playerController.OnAnimationComplete -= OnComplete;
            command.playerController.StartCoroutine(HideActionCam());
        }*/
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

    /// <summary>
    /// The rifle attack animation is a looping one that auto repeats. In animation loop the OnAnimationComplete event is called.
    /// This eventually calls this method. We use the maxBurstFire variable to terminate the loop.
    /// Since the looping animation is handled in Unity and this loop can be multi-threaded, the update to the maxBurstFire variable
    /// can become dirty if not thread synchronized. For this reason we put this update inside an object lock.
    /// </summary>
    /// <param name="name"></param>
    public void OnComplete(string name)
    {
        lock (this) { //Thread synchronize. 
            EndCommandIfPossible(name);
        }
    }

    private void EndCommandIfPossible(string name)
    {
        if ("attack".Equals(name))
        {
            ++fireCounter;
            CommandTemplate wt = command.commandTemplate;
            //Debug.LogFormat("Fire counter is  {0} Max burst fire is {1}", fireCounter, wt.maxBurstFire);
            if (wt.maxBurstFire == fireCounter)
            {
                //Debug.Log("Completing command since fire limit reached");
                command.complete = true;
                fireCounter = 0;
                command.anim.ResetTrigger("Single_Shot");
                command.TransitionToState(command.StateMap[Command.InternalState.idle.ToString()]);
                command.isActivated = false;
                command.playerController.OnAnimationComplete -= OnComplete;
                if (command.onCompleteCallback != null)
                {
                    command.onCompleteCallback();
                    command.onCompleteCallback = null;
                }
                command.playerController.StartCoroutine(HideActionCam());
            }
        }
    }
}
