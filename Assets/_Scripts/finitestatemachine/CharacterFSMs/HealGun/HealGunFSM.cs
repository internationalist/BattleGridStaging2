using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class HealGunFSM : RifleAttackFSM
{

    public HealGunFSM(Animator anim, NavMeshAgent nav, PlayerController controller, CommandTemplate weaponData) : base(anim, nav, controller, weaponData)
    {
        StateMap.Remove(state.attack.ToString());
        StateMap.Add(state.attack.ToString(), new HealGunState());
        actionData = Resources.Load<ScriptableObject>("ScriptableObjects/AttackData");
        commandType = type.primaryattack;
        attackAmounts = new Queue<ApplicableDamage>();
        TAG = "Friendly";
        commandType = type.buff;
    }

    protected override void Activate(Transform enemyTransform, Vector3? destination)
    {
        if (playerController.playerMetaData.voice.responseOnAction != null && playerController.playerMetaData.voice.responseOnAction.Count > 0)
        {
            AudioManager.PlayVoice(playerController.playerMetaData.voice.responseOnAction, playerController.audioSource);
        }
        base.Activate(enemyTransform, destination);
        if (enemyTransform != null && destination.HasValue)
        {
            this.Destination = destination.Value;
            this.EnemyTransform = enemyTransform;
            isActivated = true;
            CommandTemplate wt = this.commandTemplate;
            int enemyHp = this.enemyController.playerMetaData.Hp;
            int totalDamage = 0;
            for (int i = 0; i < wt.maxBurstFire; i++)
            {
                float distanceCovered, damageReductionPct;
                CoverMeta cm = GeneralUtils.CalculateDmgPercent(this, out distanceCovered, out damageReductionPct);
                int damageAmt = 0;
                bool critical;

                GeneralUtils.CalculateDamage(wt.damageParameters, distanceCovered,
                    damageReductionPct, out damageAmt, out critical);
                attackAmounts.Enqueue(new ApplicableDamage(damageAmt, critical, cm));
                totalDamage += damageAmt;
            }
            float actionCamRandom = Random.Range(0f, 1f);
        }
        else
        {
            Destination = destination.Value;
            this.EnemyTransform = null;
            isActivated = false;
        }
    }

    public HealGunFSM() { }
}