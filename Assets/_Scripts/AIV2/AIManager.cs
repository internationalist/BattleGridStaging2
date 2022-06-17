using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AIManager { 

    public static void TriggerMoveCommand(PlayerController controller, PlayerController enemy, Vector3 movemenLocation,
        Command.OnCompleteCallback onComplete)
    {
        if (!GameManager.I.RecordOccupancyIfEmpty(controller.ID, movemenLocation))
        {
            Vector2 displacement = 2 * Random.insideUnitCircle;
            Vector3 displacement3d = new Vector3(displacement.x, 0, displacement.y);
            movemenLocation += displacement3d;
        }
        TriggerCommand(Command.type.move, controller, enemy, movemenLocation, onComplete);
    }

    public static void TriggerPrimaryAction(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.primaryaction, controller, enemy, Vector3.zero,
            () => { });
    }

    public static void TriggerSpecialAction(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.specialaction, controller, enemy, Vector3.zero,
            () => { });
    }

    public static void TriggerReload(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.reload, controller, enemy, Vector3.zero,
            () => { });
    }

    private static void TriggerCommand(Command.type cmdType,
                                  PlayerController controller,
                                  PlayerController enemy,
                                  Vector3 movementLocation,
                                  Command.OnCompleteCallback onComplete)
    {

        //commandStartTimeInSec = Time.realtimeSinceStartup; // We will enforce a timeout on the command due to a vague problem of commands not finishing.
        switch (cmdType)
        {
            case Command.type.move:
                controller.AddToCommandQueue(GeneralUtils.MOVESLOT, enemy.transform, movementLocation, onComplete);
                break;
            case Command.type.primaryaction:
                //Debug.Log("TriggerCommand::Running attack command and  setting attack achieved flag to true");
                controller.AddToCommandQueue(GeneralUtils.ATTACKSLOT, enemy.transform, enemy.transform.position, onComplete);
                break;
            case Command.type.specialaction:
                controller.AddToCommandQueue(GeneralUtils.ITEMSLOT, enemy.transform, enemy.transform.position, onComplete);
                break;
            case Command.type.reload:
                controller.AddToCommandQueue(GeneralUtils.RELOADSLOT, enemy.transform, null, onComplete);
                break;
        }
    }
}
