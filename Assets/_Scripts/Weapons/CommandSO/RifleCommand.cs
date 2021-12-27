using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "RifleCommand", menuName = "Commands/Rifle", order = 1)]
public class RifleCommand : CommandTemplate
{
    //CoverMeta cm;
    ParticleSystem impactEffect;
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

    private void GenerateBulletTrail(Command command, CommandDataInstance wd)
    {
        bullet = GameObject.Instantiate(bulletTrailPrefab, Vector3.zero, Quaternion.identity);
        bullet.transform.position = wd.rlaunchPoint.transform.position;
        bullet.transform.rotation = Quaternion.identity;
    }

    #region private methods


    private IEnumerator ActivateMuzzleLight(GameObject muzzleLight)
    {
        muzzleLight.SetActive(true);
        UIManager.StartCamShake();
        yield return new WaitForSeconds(.5f);
        muzzleLight.SetActive(false);
        UIManager.StopCamShake();
    }

    private IEnumerator ApplyDamage(Command command)
    {
        yield return new WaitUntil(()=> { return effectComplete; });
        effectComplete = false;
        RifleAttackFSM attackFSM = (RifleAttackFSM)command;
        if(attackFSM.attackAmounts.Count > 0)
        {
            ApplicableDamage ad = attackFSM.attackAmounts.Dequeue();
            TakeDamage(command, ad.damageAmt, ad.critical, ad.cm);
        }
    }



    public void TakeDamage(Command command, int damageAmt, bool critical, CoverMeta cm)
    {

        command.enemyController.TakeDamage(damageAmt, critical);
        //Create damage notification floating text.
        UIManager.DamageNotification(damageAmt, critical, command.enemyController.transform.position + Vector3.up * 1.8f);
        if (damageAmt > 0)
        {
            Quaternion lookRot = OrientParticleEffect(impactEffect, command, command.enemyController.transform.position);
            int cnt = command.enemyController.bloodEffects.Count;
            ParticleSystem bloodEffectPrfab = command.enemyController.bloodEffects[Random.Range(0, cnt)];
            ParticleSystem bloodInstance = Instantiate(bloodEffectPrfab, command.enemyController.transform.position, lookRot);
            ParticleSystem bloodMist = Instantiate(command.enemyController.bloodMist, command.enemyController.transform.position, lookRot);
            bloodInstance.Play();
            bloodMist.Play();
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
                cm.cover.PlayRicochet();
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
        for(int i = 0; i < effectFlashPrefab.Length; i++)
        {
            ParticleSystem muzzleFlashInstance = GameObject.Instantiate(effectFlashPrefab[i], Vector3.zero, Quaternion.identity);
            muzzleFlashInstance.transform.parent = wd.rlaunchPoint.transform;
            muzzleFlashInstance.transform.localPosition = Vector3.zero;
            muzzleFlashInstance.transform.localRotation = Quaternion.identity;
            muzzleFlashInstance.Play();
        }
    }

    public override Command getCommand(Animator anim, NavMeshAgent nav, PlayerController controller)
    {
        return new RifleAttackFSM(anim, nav, controller, this);
    }
    #endregion
}
