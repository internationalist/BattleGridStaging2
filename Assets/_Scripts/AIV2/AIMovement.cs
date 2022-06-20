using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    PlayerController controller;
    CommandTemplate commandTmpl;
    private float startTime;
    public float delay;
    AIBrain aiBrain;
    bool acquiringCover;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        commandTmpl = controller.GetWeaponTemplateForCommand(GeneralUtils.ATTACKSLOT);
        aiBrain = GetComponent<AIBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActive())
        {
            startTime = AIUtils.ApproachEnemy(controller, aiBrain.enemy, commandTmpl);
        }
    }

    private bool IsActive()
    {
        return Time.time - startTime > delay //within the repeat delay
                && controller != null //This player exists.
                && !controller.IsDead //This player is not dead
                && aiBrain.enemy != null //Enemy exists
                && !aiBrain.enemy.IsDead; //Enemy is not dead
    }

    void ApproachEnemy()
    {
        startTime = Time.time;
        var distanceToEnemy = Vector3.Distance(transform.position, aiBrain.enemy.transform.position);
        if (distanceToEnemy > commandTmpl.damageParameters.optimalRange)
        {
            var movementDistance = distanceToEnemy - commandTmpl.damageParameters.optimalRange;
            Vector3 dirOfMovement = (aiBrain.enemy.transform.position - transform.position).normalized;
            Vector3 movementLocation = transform.position + dirOfMovement * movementDistance;
            AIManager.TriggerMoveCommand(controller, aiBrain.enemy, movementLocation, ()=> { });
        }
    }
}
