using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "HealGunCommand", menuName = "Commands/HealGun", order = 1)]
public class HealGunCommand : CommandTemplate
{
    protected ParticleSystem impactEffect;
    public BulletTrail bulletTrailPrefab;
    BulletTrail bullet;
    public string uiImageName;
    public override UIMetaData getUIMetadata()
    {
        return new UIMetaData("primaryattack", uiImageName);
    }
    public override void Launch(Command command, CommandDataInstance wd)
    {
        if (wd.rlaunchPoint != null)
        {
            wd.rlaunchPoint = wd.r_weapon.gameObject.transform.Find("LaunchPoint");
            wd.MuzzleLight = wd.rlaunchPoint.gameObject.transform.Find("MuzzleLight").gameObject;
        }

        if (impactEffect == null)
        {
            impactEffect = GameObject.Instantiate(impactFlashPrefab, Vector3.zero, Quaternion.identity);
        }
        command.playerController.StartCoroutine(ActivateMuzzleLight(wd.MuzzleLight));
        MuzzleFlash(wd);
        command.playerController.StartCoroutine(RunNoiseEffect(wd));
        command.playerController.StartCoroutine(ApplyDamage(command));
    }
    public override Command getCommand(Animator anim, NavMeshAgent nav, PlayerController controller)
    {
        return new HealGunFSM(anim, nav, controller, this);
    }

    private IEnumerator ActivateMuzzleLight(GameObject muzzleLight)
    {
        muzzleLight.SetActive(true);
        UIManager.StartCamShake();
        yield return new WaitForSeconds(.5f);
        muzzleLight.SetActive(false);
        UIManager.StopCamShake();
    }

    protected IEnumerator ApplyDamage(Command command)
    {
        yield return new WaitUntil(() => { return effectComplete; });
        effectComplete = false;
        HealGunFSM healGunFSM = (HealGunFSM)command;
        if (healGunFSM.attackAmounts.Count > 0)
        {
            ApplicableDamage ad = healGunFSM.attackAmounts.Dequeue();
            TakeDamage(command, ad.damageAmt, ad.critical, ad.cm);
        }
    }

    protected void TakeDamage(Command command, int damageAmt, bool critical, CoverMeta cm)
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
            }
        }
    }

    private Quaternion OrientParticleEffect(ParticleSystem ps, Command command, Vector3 impactPoint)
    {
        Quaternion lookRot = Quaternion.LookRotation(command.playerController.transform.position - impactPoint);
        ps.transform.position = impactPoint + command.enemyController.transform.up;
        ps.transform.rotation = lookRot;
        ps.Play();
        return lookRot;
    }

    private void MuzzleFlash(CommandDataInstance wd)
    {
        for (int i = 0; i < effectFlashPrefab.Length; i++)
        {
            ParticleSystem muzzleFlashInstance = GameObject.Instantiate(effectFlashPrefab[i], Vector3.zero, Quaternion.identity);
            muzzleFlashInstance.transform.parent = wd.rlaunchPoint.transform;
            muzzleFlashInstance.transform.localPosition = Vector3.zero;
            muzzleFlashInstance.transform.localRotation = Quaternion.identity;
            muzzleFlashInstance.Play();
        }
    }
}
