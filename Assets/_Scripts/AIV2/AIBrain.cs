using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    public bool isRunning;
    float postCommandPauseInSecs = 1;
    public PlayerController enemy;


    public void TriggerMoveCommand(PlayerController controller, Vector3 movemenLocation)
    {
        TriggerCommand(Command.type.move, controller, null, movemenLocation);
    }

    public void TriggerPrimaryAction(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.primaryaction, controller, enemy, Vector3.zero);
    }

    public void TriggerSpecialAction(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.specialaction, controller, enemy, Vector3.zero);
    }

    public void TriggerReload(PlayerController controller)
    {
        TriggerCommand(Command.type.reload, controller, null, Vector3.zero);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void TriggerCommand(Command.type cmdType, PlayerController controller, PlayerController enemy, Vector3 movementLocation)
    {

    //commandStartTimeInSec = Time.realtimeSinceStartup; // We will enforce a timeout on the command due to a vague problem of commands not finishing.
        isRunning = true;
        switch (cmdType)
        {
            case Command.type.move:
                //Debug.LogFormat("{0} AIAttackState:TriggerCommand->Running move command", aim._controller.name);
                //Debug.Log("TriggerCommand::Running move command");
                controller.AddToCommandQueue(GeneralUtils.MOVESLOT);
                Command cmd = controller.ActivateCommand(GeneralUtils.MOVESLOT, null, movementLocation, () =>
                {
                    isRunning = false;
                });
                break;
            case Command.type.primaryaction:
                //Debug.Log("TriggerCommand::Running attack command and  setting attack achieved flag to true");
                controller.AddToCommandQueue(GeneralUtils.ATTACKSLOT);
                controller.ActivateCommand(GeneralUtils.ATTACKSLOT, enemy.transform, enemy.transform.position, () =>
                {
                    GameManager.I.StartCoroutine(DelayedCommandComplete());
                });
                break;
            case Command.type.specialaction:
                controller.AddToCommandQueue(GeneralUtils.ITEMSLOT);
                //Debug.Log("TriggerCommand::Running special attack command and  setting attack achieved flag to true");
                controller.ActivateCommand(GeneralUtils.ITEMSLOT, enemy.transform, enemy.transform.position, () =>
                {
                    GameManager.I.StartCoroutine(DelayedCommandComplete());
                });
                break;
            case Command.type.reload:
                controller.AddToCommandQueue(GeneralUtils.RELOADSLOT);
                controller.ActivateCommand(GeneralUtils.RELOADSLOT, null, null, () =>
                {
                    isRunning = false;
                });
                break;
        }
    }

    private IEnumerator DelayedCommandComplete()
    {
        yield return new WaitForSeconds(postCommandPauseInSecs);
        isRunning = false;
    }
}
