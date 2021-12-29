using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UseItemFSM : Command
{
    public UseItemFSM(Animator anim, NavMeshAgent nav, PlayerController controller, CommandTemplate commandTemplate) : base(anim, nav, controller, commandTemplate)
    {
        currentState = new UseItemState();
        StateMap.Add(state.idle.ToString(), this.currentState);
        commandType = type.throwItem;
        invokeImmediate = true;
    }

    protected override void Activate(Transform enemyTransform, Vector3? destination)
    {
        if (playerController.playerMetaData.voice.responseOnUseItem != null && playerController.playerMetaData.voice.responseOnUseItem.Count > 0)
        {
            AudioManager.PlayHurtSound(playerController.playerMetaData.voice.responseOnUseItem, playerController.audioSource);
        }
        base.Activate(enemyTransform, destination);
        isActivated = true;
    }

    public override void ActivateWeapon()
    {
        commandTemplate.Launch(this, this.commandDataInstance);
        playerController.StartCoroutine(heal());
    }

    private IEnumerator heal()
    {
        for (int i = 1; i < 100; i++)
        {
            if (playerController.playerMetaData.maxHP - playerController.playerMetaData.Hp > 0)
            {
                playerController.playerMetaData.Hp += 1;
            }
            else
            {
                break;
            }
            playerController.healthBar.fillAmount = playerController.playerMetaData.NormalizedHealthRemaining();
            yield return new WaitForSeconds(.02f);
        }
    }

    protected override void Activated()
    {
    }

    protected override void BeforeActivate()
    {
    }

    protected override bool Validate(Transform enemyTransform, Vector3? destination)
    {
        return playerController.playerMetaData.CanUseItem();
    }
    public override void Complete()
    {
        base.Complete();
        for (int i = 0; i < commandTemplate.effectFlashPrefab.Length; i++)
        {
            ParticleSystem muzzleFlashInstance = GameObject.Instantiate(commandTemplate.effectFlashPrefab[i],
                playerController.transform.position + playerController.transform.up + playerController.transform.forward * -.6f, Quaternion.identity);
            muzzleFlashInstance.Play();
        }
        playerController.playerMetaData.IncrementItemUseCount();
        isActivated = false;
    }
}
