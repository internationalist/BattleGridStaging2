using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPrimaryAttack : MonoBehaviour
{
    public PlayerController enemy;
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
        if(!aiBrain.isRunning
                && controller != null
                && !controller.IsDead
                && enemy != null
                && !enemy.IsDead)
        {
            var distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if(distanceToEnemy <= commandTmpl.damageParameters.optimalRange
                && commandData.ammoCount >= commandData.maxBurstFire)
            {
                aiBrain.TriggerPrimaryAttack(controller, enemy);
            }
        }
    }

}
