using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "ReloadCommand", menuName = "Commands/Reload", order = 1)]
public class ReloadCommand : CommandTemplate
{
    public override Command getCommand(Animator anim, NavMeshAgent nav, PlayerController controller)
    {
        return new ReloadFSM(anim, nav, controller, this);
    }

    public override void Launch(Command command, CommandDataInstance wd)
    {
        command.playerController.StartCoroutine(RunNoiseEffect(wd));
    }

    public override UIMetaData getUIMetadata()
    {
        return new UIMetaData("reload", "Reload");
    }

}
