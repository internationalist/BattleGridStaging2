using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "HealGunCommand", menuName = "Commands/HealGun", order = 1)]
public class HealGunCommand : CommandTemplate
{
    protected ParticleSystem impactEffect;
    public LaserBeam beamTrailPrefab;
    LaserBeam beamTrail;
    public string uiImageName;
    List<ParticleSystem> muzzleFlashes;
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
            impactEffect = GameObject.Instantiate(impactFlashPrefab,
                                                  command.enemyController.transform.position, Quaternion.identity);
        }
        StartEffects(command, wd);
    }

    private void StartEffects(Command command, CommandDataInstance wd)
    {
        ActivateMuzzleLight(wd.MuzzleLight);
        MuzzleFlash(wd);
        command.playerController.StartCoroutine(RunNoiseEffect(wd));
        command.playerController.StartCoroutine(ApplyHeal(command));
        GenerateBulletTrail(command, wd);
    }

    public void DeActivateEffects(Command command, CommandDataInstance wd)
    {
        DeActivateMuzzleLight(wd.MuzzleLight);
        DestroyBulletTrail(command, wd);
        for(int i = 0; i < muzzleFlashes.Count; i++)
        {
            MonoBehaviour.Destroy(muzzleFlashes[i].gameObject);
        }
        MonoBehaviour.Destroy(impactEffect.gameObject);
    }

    public override Command getCommand(Animator anim, NavMeshAgent nav, PlayerController controller)
    {
        return new HealGunFSM(anim, nav, controller, this);
    }

    private void ActivateMuzzleLight(GameObject muzzleLight)
    {
        muzzleLight.SetActive(true);
        UIManager.StartCamShake();
    }

    private void DeActivateMuzzleLight(GameObject muzzleLight)
    {
        muzzleLight.SetActive(false);
        UIManager.StopCamShake();
    }

    protected IEnumerator ApplyHeal(Command command)
    {
        yield return new WaitUntil(() => { return effectComplete; });
        effectComplete = false;
        HealGunFSM healGunFSM = (HealGunFSM)command;
        if (healGunFSM.attackAmounts.Count > 0)
        {
            ApplicableDamage ad = healGunFSM.attackAmounts.Dequeue();
            Heal(command, ad.damageAmt, ad.critical, ad.cm);
        }
    }

    protected void Heal(Command command, int damageAmt, bool critical, CoverMeta cm)
    {
        command.enemyController.StartCoroutine(command.enemyController.Heal(damageAmt, 2, critical));
        //Create damage notification floating text.
        //UIManager.DamageNotification(damageAmt, critical, command.enemyController.transform.position + Vector3.up * 1.8f);
        if (damageAmt > 0)
        {
            Quaternion lookRot = OrientParticleEffect(impactEffect, command, command.enemyController.transform.position);
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
        Quaternion lookRot = Quaternion.LookRotation(command.enemyController.transform.up);
        ps.transform.position = impactPoint + command.enemyController.transform.up;
        ps.transform.rotation = lookRot;
        ps.Play();
        return lookRot;
    }

    private void MuzzleFlash(CommandDataInstance wd)
    {
        muzzleFlashes = new List<ParticleSystem>();
        for (int i = 0; i < effectFlashPrefab.Length; i++)
        {
            ParticleSystem muzzleFlashInstance = GameObject.Instantiate(effectFlashPrefab[i], Vector3.zero, Quaternion.identity);
            muzzleFlashInstance.transform.parent = wd.rlaunchPoint.transform;
            muzzleFlashInstance.transform.localPosition = Vector3.zero;
            muzzleFlashInstance.transform.localRotation = Quaternion.identity;
            muzzleFlashInstance.Play();
            muzzleFlashes.Add(muzzleFlashInstance);
        }
    }

    private void GenerateBulletTrail(Command command, CommandDataInstance wd)
    {
        beamTrail = GameObject.Instantiate(beamTrailPrefab, Vector3.zero, Quaternion.identity);
        beamTrail.endPoint = command.EnemyTransform.position + Vector3.up * 1.5f;
        beamTrail.startPoint = wd.rlaunchPoint.transform;
    }

    private void DestroyBulletTrail(Command command, CommandDataInstance wd)
    {
        if(beamTrail && beamTrail.gameObject)
        {
            MonoBehaviour.Destroy(beamTrail.gameObject);
        }
    }
}
