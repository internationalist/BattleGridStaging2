using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "HealGunCommand", menuName = "Commands/HealGun", order = 1)]
public class HealGunCommand : RifleCommand
{
    public override Command getCommand(Animator anim, NavMeshAgent nav, PlayerController controller)
    {
        return new HealGunFSM(anim, nav, controller, this);
    }

    protected override void TakeDamage(Command command, int damageAmt, bool critical, CoverMeta cm)
    {
        command.enemyController.Heal(damageAmt, critical);
        //Create damage notification floating text.
        UIManager.DamageNotification(damageAmt, critical, command.enemyController.transform.position + Vector3.up * 1.8f);
        if (damageAmt > 0)
        {
            Quaternion lookRot = OrientParticleEffect(impactEffect, command, command.enemyController.transform.position);
            /*int cnt = command.enemyController.bloodEffects.Count;
            ParticleSystem bloodEffectPrfab = command.enemyController.bloodEffects[Random.Range(0, cnt)];
            ParticleSystem bloodInstance = Instantiate(bloodEffectPrfab, command.enemyController.transform.position, lookRot);
            ParticleSystem bloodMist = Instantiate(command.enemyController.bloodMist, command.enemyController.transform.position, lookRot);
            bloodInstance.Play();
            bloodMist.Play();*/
        }
        else
        {
            if (cm.coverInWay)
            {
                for (int i = 0; i < cm.cover.ricochetEffects.Count; i++)
                {
                    ParticleSystem psInstance = Instantiate(cm.cover.ricochetEffects[i]);
                    OrientParticleEffect(psInstance, command, cm.coverPosition);
                }
                //cm.cover.PlayRicochet();
            }
        }
    }
}
