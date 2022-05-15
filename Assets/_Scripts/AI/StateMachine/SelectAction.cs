using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectAction : AIActionState
{
    public override void EnterState(AIStateMachine aiMachine)
    {
        aim = aiMachine;
    }

    public override void Update()
    {
        //if (aim._controller.turnActive)
        //{
            SelectAttack(aim);
            aim.TransitionToState(next);
        //}
    }

    public static void SelectAttack(AIStateMachine aim)
    {
        PlayerController controller = aim._controller;
        float chance = Random.Range(0f, 1f);
        //Debug.LogFormat("Grenade throw chance {0}", chance);
        AIState state = aim._aiState;

        if (controller.playerMetaData.CanUseItem() &&
            chance < aim.specialAttackChance)
        {
            Command specialCmd = controller.commands[GeneralUtils.ITEMSLOT];
            if (specialCmd is ThrowItemFSM)
            {
                ThrowItemFSM throwItem = specialCmd as ThrowItemFSM;
            }
            state.cmdType = Command.type.specialaction;
            state.weaponTemplate = controller.GetWeaponTemplateForCommand(GeneralUtils.ITEMSLOT);
            state.weaponInstance = specialCmd.commandDataInstance;
            state.attackType = Command.type.specialaction;
        }
        else
        {

            RifleAttackFSM primaryAttack = (RifleAttackFSM)controller.commands[GeneralUtils.ATTACKSLOT];

            state.cmdType = Command.type.primaryaction;

            state.weaponTemplate = controller.GetWeaponTemplateForCommand(GeneralUtils.ATTACKSLOT);

            state.weaponInstance = primaryAttack.commandDataInstance;
            state.attackType = Command.type.primaryaction;
        }
    }
}
