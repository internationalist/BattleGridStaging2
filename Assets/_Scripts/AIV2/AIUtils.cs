using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AIUtils
{
    public static float ApproachEnemy(PlayerController controller,
                                      PlayerController enemy,
                                      CommandTemplate commandTmpl,
                                      Command.OnCompleteCallback callBack)
    {
        float startTime = Time.time;
        var distanceToEnemy = Vector3.Distance(controller.transform.position, enemy.transform.position);
        if (distanceToEnemy > commandTmpl.damageParameters.optimalRange)
        {
            var movementDistance = distanceToEnemy - commandTmpl.damageParameters.optimalRange;
            Vector3 dirOfMovement = (enemy.transform.position - controller.transform.position).normalized;
            Vector3 movementLocation = controller.transform.position + dirOfMovement * movementDistance;
            AIManager.TriggerMoveCommand(controller, enemy, movementLocation, callBack);
        }
        return startTime;
    }


}
