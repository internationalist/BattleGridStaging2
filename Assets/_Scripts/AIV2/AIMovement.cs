using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    public PlayerController enemy;
    PlayerController controller;
    CommandTemplate commandTmpl;
    Vector3 movementLocation;
    private float startTime;
    public float delay;
    AIBrain aiBrain;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        commandTmpl = controller.GetWeaponTemplateForCommand(GeneralUtils.ATTACKSLOT);
        startTime = Time.time;
        aiBrain = GetComponent<AIBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(!isRunning)
        if(Time.time - startTime > delay && enemy != null && !enemy.isDead)
        {
            startTime = Time.time;
            var distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if(distanceToEnemy > commandTmpl.damageParameters.optimalRange)
            {
                var movementDistance = distanceToEnemy - commandTmpl.damageParameters.optimalRange;
                Vector3 dirOfMovement = (enemy.transform.position - transform.position).normalized;
                movementLocation = transform.position + dirOfMovement* movementDistance;
                aiBrain.TriggerMoveCommand(controller, movementLocation);
            }
        }
    }
}
