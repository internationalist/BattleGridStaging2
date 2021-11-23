using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "MovementCommand", menuName = "Commands/Movement", order = 1)]
public class MovementCommand : CommandTemplate
{
    public override Command getCommand(Animator anim, NavMeshAgent nav, PlayerController controller)
    {
        return new MovementFSM(anim, nav, controller, this);
    }

    public override void Launch(Command command, CommandDataInstance wd)
    {

    }

    public override UIMetaData getUIMetadata()
    {
        return new UIMetaData("move", "Movement");
    }
}
