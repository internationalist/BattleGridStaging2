using System.Collections;
using System.Collections.Generic;
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

    protected void TriggerCommand(Command.type cmdType, PlayerController controller, PlayerController enemy, Vector3 movementLocation)
    {
        //commandStartTimeInSec = Time.realtimeSinceStartup; // We will enforce a timeout on the command due to a vague problem of commands not finishing.
        lock(this)
        {
            isRunning = true;
            switch (cmdType)
            {
                case Command.type.move:
                    //Debug.LogFormat("{0} AIAttackState:TriggerCommand->Running move command", aim._controller.name);
                    //Debug.Log("TriggerCommand::Running move command");
                    Command cmd = GameManager.ActivateCommand(controller, GeneralUtils.MOVESLOT, null, movementLocation, () =>
                    {
                        isRunning = false;
                    });
                    break;
                case Command.type.primaryaction:
                    //Debug.Log("TriggerCommand::Running attack command and  setting attack achieved flag to true");
                    GameManager.ActivateCommand(controller, GeneralUtils.ATTACKSLOT, enemy.transform, enemy.transform.position, () =>
                    {
                        GameManager.I.StartCoroutine(DelayedCommandComplete());
                    });
                    break;
                case Command.type.specialaction:
                    //Debug.Log("TriggerCommand::Running special attack command and  setting attack achieved flag to true");
                    GameManager.ActivateCommand(controller, GeneralUtils.ITEMSLOT, enemy.transform, enemy.transform.position, () =>
                    {
                        GameManager.I.StartCoroutine(DelayedCommandComplete());
                    });
                    break;
                case Command.type.reload:
                    GameManager.ActivateCommand(controller, GeneralUtils.RELOADSLOT, null, null, () =>
                    {
                        isRunning = false;
                    });
                    break;
            }
        }
    }

    private IEnumerator DelayedCommandComplete()
    {
        yield return new WaitForSeconds(postCommandPauseInSecs);
        isRunning = false;
    }
}
