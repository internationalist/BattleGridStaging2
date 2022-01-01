using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ReloadFSM : Command
{
    public bool turnToEnemy = false;
    public ReloadFSM(Animator anim, NavMeshAgent nav, PlayerController controller, CommandTemplate weaponData) : base(anim, nav, controller, weaponData)
    {
        currentState = new ReloadState();
        StateMap.Add(state.idle.ToString(), this.currentState);
        commandType = type.reload;
        invokeImmediate = true;
    }
    protected override void Activate(Transform enemyTransform, Vector3? destination)
    {
        if (playerController.playerMetaData.voice.responseOnReload != null && playerController.playerMetaData.voice.responseOnReload.Count > 0)
        {
            AudioManager.PlayVoice(playerController.playerMetaData.voice.responseOnReload, playerController.audioSource);
            //int audioIndex = Random.Range(0, playerController.playerMetaData.responseOnReload.Count);
            //playerController.audioSource.PlayOneShot(playerController.playerMetaData.responseOnReload[audioIndex]);
        }
        base.Activate(enemyTransform, destination);
    }


    public override void ActivateWeapon()
    {
        commandTemplate.Launch(this, this.commandDataInstance);
    }

    protected override void BeforeActivate()
    {
    }

    protected override void Activated()
    {
    }

    protected override bool Validate(Transform enemyTransform, Vector3? destination)
    {
        return playerController.playerMetaData.CanReload();
    }


    public override void Cancel()
    {

    }

    public override void Complete()
    {
        base.Complete();
        playerController.playerMetaData.IncrementReloadCount();
    }
}
