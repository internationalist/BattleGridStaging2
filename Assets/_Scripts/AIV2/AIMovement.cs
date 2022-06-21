using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    PlayerController controller;
    //CommandTemplate commandTmpl;
    private float startTime;
    public float delay;
    AIBrain aiBrain;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        //commandTmpl = controller.GetWeaponTemplateForCommand(GeneralUtils.ATTACKSLOT);
        aiBrain = GetComponent<AIBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActive())
        {
            startTime = AIUtils.ApproachEnemy(controller,
                                              aiBrain.enemy,
                                              aiBrain.commandTmpl,
                                              ()=> { });
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
}
