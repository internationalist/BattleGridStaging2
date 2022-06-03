using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    #region state
    public PlayerController enemy;
    PlayerController player;
    PlayerController[] allPlayers;
    bool choosingEnemy;
    #endregion

    #region Unity events
    private void Start()
    {
        player = GetComponent<PlayerController>();
        allPlayers = FindObjectsOfType<PlayerController>();
        ChooseEnemy();
    }

    private void Update()
    {
        if(enemy.IsDead && !choosingEnemy)
        {
            ChooseEnemy();
        }
    }



    private void ChooseEnemy()
    {
        choosingEnemy = true;
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i] != null
                && !allPlayers[i].IsDead
                && allPlayers[i].ID != player.ID
                && allPlayers[i].teamID != player.teamID)
            {
                enemy = allPlayers[i];
                break;
            }
        }
        choosingEnemy = false;
    }
    #endregion

    public void TriggerMoveCommand(PlayerController controller, PlayerController enemy, Vector3 movemenLocation)
    {
        TriggerCommand(Command.type.move, controller, enemy, movemenLocation);
    }

    public void TriggerPrimaryAction(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.primaryaction, controller, enemy, Vector3.zero);
    }

    public void TriggerSpecialAction(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.specialaction, controller, enemy, Vector3.zero);
    }

    public void TriggerReload(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.reload, controller, enemy, Vector3.zero);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void TriggerCommand(Command.type cmdType, PlayerController controller, PlayerController enemy, Vector3 movementLocation)
    {

    //commandStartTimeInSec = Time.realtimeSinceStartup; // We will enforce a timeout on the command due to a vague problem of commands not finishing.
        switch (cmdType)
        {
            case Command.type.move:
                controller.AddToCommandQueue(GeneralUtils.MOVESLOT, enemy.transform, movementLocation, () =>
                {});
                break;
            case Command.type.primaryaction:
                //Debug.Log("TriggerCommand::Running attack command and  setting attack achieved flag to true");
                controller.AddToCommandQueue(GeneralUtils.ATTACKSLOT, enemy.transform, enemy.transform.position, () =>
                {});
                break;
            case Command.type.specialaction:
                controller.AddToCommandQueue(GeneralUtils.ITEMSLOT, enemy.transform, enemy.transform.position, () =>
                {});
                break;
            case Command.type.reload:
                controller.AddToCommandQueue(GeneralUtils.RELOADSLOT, enemy.transform, null, () =>
                {});
                break;
        }
    }
}
