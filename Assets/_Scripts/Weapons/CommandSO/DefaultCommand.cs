using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "DefaultCommand", menuName = "Commands/Default", order = 1)]
public class DefaultCommand : CommandTemplate
{
    public override Command getCommand(Animator anim, NavMeshAgent nav, PlayerController controller)
    {
        return new IdleFSM(anim, nav, controller, this);
    }

    public override void Launch(Command command, CommandDataInstance wd)
    {

    }

    public override UIMetaData getUIMetadata()
    {
        return new UIMetaData("idle", null);
    }
}
