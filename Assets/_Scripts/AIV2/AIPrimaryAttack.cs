using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPrimaryAttack : MonoBehaviour
{
    public PlayerController enemy;
    PlayerController controller;
    CommandTemplate commandTmpl;
    CommandDataInstance commandData;
    Vector3 movementLocation;
    float postCommandPauseInSecs = 1;
    AIBrain aiBrain;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        commandTmpl = controller.GetWeaponTemplateForCommand(GeneralUtils.ATTACKSLOT);
        commandData = controller.commands[GeneralUtils.ATTACKSLOT].commandDataInstance;
        aiBrain = GetComponent<AIBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!aiBrain.isRunning && enemy != null && !enemy.isDead)
        {
            var distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if(distanceToEnemy <= commandTmpl.damageParameters.optimalRange
                && commandData.ammoCount >= commandData.maxBurstFire)
            {
                TriggerCommand(Command.type.primaryaction);
            }
        }
    }

    protected void TriggerCommand(Command.type cmdType)
    {
        //commandStartTimeInSec = Time.realtimeSinceStartup; // We will enforce a timeout on the command due to a vague problem of commands not finishing.
        aiBrain.isRunning = true;
        switch (cmdType)
        {
            case Command.type.move:
                GameManager.AssignCommand(controller, GeneralUtils.MOVESLOT);
                //Debug.LogFormat("{0} AIAttackState:TriggerCommand->Running move command", aim._controller.name);
                //Debug.Log("TriggerCommand::Running move command");
                Command cmd = GameManager.ActivateCommand(controller, null, movementLocation, () =>
                {
                    //Debug.LogFormat("{0} AIAttackState:TriggerCommand->Command done", aim._controller.name);
                    aiBrain.isRunning = false;
                });
                break;
            case Command.type.primaryaction:
                GameManager.AssignCommand(controller, GeneralUtils.ATTACKSLOT);
                //Debug.Log("TriggerCommand::Running attack command and  setting attack achieved flag to true");
                GameManager.ActivateCommand(controller, enemy.transform, enemy.transform.position, () =>
                {
                    GameManager.I.StartCoroutine(DelayedCommandComplete());
                });
                break;
            case Command.type.specialaction:
                GameManager.AssignCommand(controller, GeneralUtils.ITEMSLOT);
                //Debug.Log("TriggerCommand::Running special attack command and  setting attack achieved flag to true");
                GameManager.ActivateCommand(controller, enemy.transform, enemy.transform.position, () =>
                {
                    GameManager.I.StartCoroutine(DelayedCommandComplete());
                });
                break;
            case Command.type.reload:
                GameManager.AssignCommand(controller, GeneralUtils.RELOADSLOT);
                GameManager.ActivateCommand(controller, null, null, () =>
                {
                    //Debug.LogFormat("{0} AIAttackState:TriggerCommand->Command done", aim._controller.name);
                    aiBrain.isRunning = false;
                });
                break;
        }
    }

    private IEnumerator DelayedCommandComplete()
    {
        yield return new WaitForSeconds(postCommandPauseInSecs);
        aiBrain.isRunning = false;
    }
}
