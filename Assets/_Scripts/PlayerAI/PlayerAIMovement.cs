using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAIMovement : MonoBehaviour
{
    // Start is called before the first frame update
    PlayerController controller;
    Vector3 movementLocationInProgress;
    AIBrain aiBrain;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        aiBrain = GetComponent<AIBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActive() && aiBrain.movementLocation != movementLocationInProgress)
        {
            movementLocationInProgress = aiBrain.movementLocation;
            AIUtils.ApproachLocation(controller,
                                     movementLocationInProgress,
                                              () => { });
        }
    }

    private bool IsActive()
    {
        return  controller != null //This player exists.
                && !controller.IsDead; //This player is not dead
    }
}
