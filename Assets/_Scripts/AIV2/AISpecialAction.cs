using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpecialAction : MonoBehaviour
{
    PlayerController controller;
    CommandTemplate commandTmpl;
    CommandDataInstance commandData;
    AIBrain aiBrain;
    [Range(0, 1)]
    public float fireChance;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        commandTmpl = controller.GetWeaponTemplateForCommand(GeneralUtils.ITEMSLOT);
        commandData = controller.commands[GeneralUtils.ATTACKSLOT].commandDataInstance;
        aiBrain = GetComponent<AIBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!aiBrain.isRunning
                && controller != null
                && !controller.IsDead
                && aiBrain.enemy != null
                && !aiBrain.enemy.IsDead)
        {
            var distanceToEnemy = Vector3.Distance(transform.position, aiBrain.enemy.transform.position);
            if (distanceToEnemy <= commandTmpl.damageParameters.optimalRange
                && CalculateChance())
            {
                aiBrain.TriggerSpecialAction(controller, aiBrain.enemy);
            }
        }
    }

    private bool CalculateChance()
    {
        float outcome = Random.Range(0f, 1f);
        if (fireChance > outcome)
        {
            return true;
        } else
        {
            return false;
        }
    }
}
