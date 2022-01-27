using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : BaseState
{
    public override void EnterState(BaseFSMController controller)
    {
        Command characterPlayer = (Command)controller;
        Dismemberment dsm = characterPlayer.playerController.GetComponent<Dismemberment>();
        float decimateChance = characterPlayer.playerController.playerMetaData.decimateChance;
        if (Random.value <= decimateChance && dsm != null)
        {
            dsm.decimate = true;
            characterPlayer.playerController.StartCoroutine(DestroyLater());
        } else
        {
            characterPlayer.anim.SetTrigger("Death");
            PlayerController.OnAnimationComplete += OnComplete;
        }
    }
    protected bool complete;
    public override void Update(BaseFSMController controller)
    {
        Command characterPlayer = (Command)controller;
        
        if (complete)
        {
            characterPlayer.playerController.Die();
            PlayerController.OnAnimationComplete -= OnComplete;
        }
    }

    private IEnumerator DestroyLater()
    {
        yield return new WaitForSeconds(10f);
        complete = true;
    }

    public void OnComplete(string name)
    {
        lock (this) //Thread synchronize.
        {
            Debug.Log("DeathState::OnComplete::Character just died");
            if ("death".Equals(name))
            {
                complete = true;
            }
        }
    }
}
