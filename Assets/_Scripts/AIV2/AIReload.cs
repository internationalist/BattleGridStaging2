using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIReload : MonoBehaviour
{
    public PlayerController enemy;
    PlayerController controller;
    CommandDataInstance commandData;
    AIBrain aiBrain;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        commandData = controller.commands[GeneralUtils.ATTACKSLOT].commandDataInstance;
        aiBrain = GetComponent<AIBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!aiBrain.isRunning
                && controller != null
                && !controller.isDead
                && enemy != null
                && !enemy.isDead)
        {
            if (commandData.ammoCount < commandData.maxBurstFire)
            {
                aiBrain.TriggerReload(controller);
            }
        }
    }
}
