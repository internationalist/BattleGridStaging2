using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPrimaryAction : MonoBehaviour
{
    PlayerController controller;
    CommandTemplate commandTmpl;
    CommandDataInstance commandData;
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
        if(//!aiBrain.isRunning
                //&&
                controller != null
                && !controller.IsDead
                && aiBrain.enemy != null
                && !aiBrain.enemy.IsDead)
        {
            var distanceToEnemy = Vector3.Distance(transform.position, aiBrain.enemy.transform.position);
            if(distanceToEnemy <= commandTmpl.damageParameters.optimalRange
                && commandData.ammoCount >= commandData.maxBurstFire)
            {
                aiBrain.TriggerPrimaryAction(controller, aiBrain.enemy);
            }
        }
    }

}
