using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "GrenadeCommand", menuName = "Commands/Grenade", order = 1)]
public class GrenadeCommand : CommandTemplate
{
    public override Command getCommand(Animator anim, NavMeshAgent nav, PlayerController controller)
    {
        return new ThrowItemFSM(anim, nav, controller, this);
    }

    public override UIMetaData getUIMetadata()
    {
        return new UIMetaData("throwItem", "noun_Grenade_1754176");
    }

    public override void Launch(Command command, CommandDataInstance wd)
    {
        if(wd.payload == null)
        {
            wd.rlaunchPoint = wd.l_weapon.gameObject.transform.Find("LaunchPoint");
            wd.payload = GameObject.Instantiate(payloadPrefab, wd.rlaunchPoint.position, Quaternion.identity);
            wd.payload.impactEffect = GameObject.Instantiate(impactFlashPrefab, Vector3.zero, Quaternion.identity);
            wd.payload.ownerObject = command.playerTransform.name;
            wd.payload.damageParameters = damageParameters;
        } else
        {
            wd.payload.gameObject.SetActive(true);
            wd.payload.transform.position = wd.rlaunchPoint.position;
            wd.payload.transform.rotation = Quaternion.identity;
        }

        GameManager.ThrowProjectile(wd.payload.transform, wd.rlaunchPoint.position, command.Destination.Value,
            ()=> {
                //No implementation. Grenade projectile physics handles this.
            });
    }
}
