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
    [Header("AI probability of activating special attack")]
    public float fireChance;
    [Header("Cool down delay")]
    public float coolDownDelay;
    private float activationTime;



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
        if (IsActive())
        {
            var distanceToEnemy = Vector3.Distance(transform.position, aiBrain.enemy.transform.position);
            if (distanceToEnemy <= commandTmpl.damageParameters.optimalRange)
            {
                activationTime = Time.time;
                AIManager.TriggerSpecialAction(controller, aiBrain.enemy);
            }
        }
    }

    private bool IsActive()
    {
        return controller != null && !controller.IsDead //Player is alive
                    && aiBrain.enemy != null && !aiBrain.enemy.IsDead //Enemy is alive
                    && CalculateChance() //Probability of activation
                    && (Time.time - activationTime) >= coolDownDelay; //Within the activation delay
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
