using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    PlayerController controller;
    CommandTemplate commandTmpl;
    Vector3 movementLocation;
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
        if (Time.time - startTime > delay
        && controller != null
        && !controller.IsDead
        && aiBrain.enemy != null
        && !aiBrain.enemy.IsDead)
        {
            ApproachEnemy();
        }
    }

    void ApproachEnemy()
    {
        startTime = Time.time;
        var distanceToEnemy = Vector3.Distance(transform.position, aiBrain.enemy.transform.position);
        if (distanceToEnemy > commandTmpl.damageParameters.optimalRange)
        {
            var movementDistance = distanceToEnemy - commandTmpl.damageParameters.optimalRange;
            Vector3 dirOfMovement = (aiBrain.enemy.transform.position - transform.position).normalized;
            movementLocation = transform.position + dirOfMovement * movementDistance;
            AIManager.TriggerMoveCommand(controller, aiBrain.enemy, movementLocation, ()=> { });
        }
    }
}
