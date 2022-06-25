using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAIMovement : MonoBehaviour
{
    // Start is called before the first frame update
    PlayerController controller;
    PlayerAIBrain playerBrain;
    public Vector3 movementLocation;
    Vector3 movementLocationInProgress;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActive() && movementLocation != movementLocationInProgress)
        {
            movementLocationInProgress = movementLocation;
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
