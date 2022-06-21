using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAcquireCover : MonoBehaviour
{
    PlayerController controller;
    //CommandTemplate commandTmpl;
    private float startTime;
    public float delay;
    AIBrain aiBrain;
    bool acquiringCover;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        startTime = Time.time;
        aiBrain = GetComponent<AIBrain>();
    }

    // Update is called once per frame
    void Update()
    {


        if (IsActive())
        {
            AcquireCover();
        }
    }

    private void AcquireCover()
    {
        var distanceToEnemy = Vector3.Distance(transform.position, aiBrain.enemy.transform.position);
        if (distanceToEnemy > aiBrain.commandTmpl.damageParameters.optimalRange || !controller.InCover)
        {
            DockPoint dock = null;
            startTime = Time.time;
            List<CoverFramework> coversBYClosest = new List<CoverFramework>(GameManager.I.covers);
            coversBYClosest.Sort((CoverFramework thisOne, CoverFramework other) =>
            {
                var distanceFromThisOne = Vector3.Distance(transform.position, thisOne.transform.position);
                var distanceFromOther = Vector3.Distance(transform.position, other.transform.position);
                if (distanceFromOther == distanceFromThisOne)
                {
                    return 0;
                }
                else if (distanceFromThisOne > distanceFromOther)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            });
            for (int i = 0; i < coversBYClosest.Count; i++)
            {
                dock = aiBrain.EvaluateCover(controller, aiBrain.enemy, coversBYClosest[i]);
                if (dock != null)
                {
                    Debug.LogFormat("{0}: Moving to cover position {1}", name, dock.position);
                    acquiringCover = true;
                    AIManager.TriggerMoveCommand(controller,
                                               aiBrain.enemy,
                                               dock.position,
                                               () =>
                                               {
                                                   acquiringCover = false;
                                               });
                    break;
                }
            }
            if(dock == null)
            {
                Debug.Log("No suitable cover found");
                startTime = AIUtils.ApproachEnemy(controller,
                                                  aiBrain.enemy,
                                                  aiBrain.commandTmpl,
                                                () =>
                                                {
                                                     acquiringCover = false;
                                                });
            }
        }
    }

    private bool IsActive()
    {
        return !acquiringCover
                && Time.time - startTime > delay
                && controller != null
                && !controller.IsDead
                && aiBrain.enemy != null
                && !aiBrain.enemy.IsDead;
    }
}
