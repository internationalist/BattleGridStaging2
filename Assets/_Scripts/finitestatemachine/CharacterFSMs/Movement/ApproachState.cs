using UnityEngine;

public class ApproachState : BaseState
{
    public float normalizedSpeed;
    private MovementFSM command;
    float expectedTimeToComplete;
    float startTime;
    float timeOverFlowPercent = 300;
    public override void EnterState(BaseFSMController controller)
    {
        command = (MovementFSM)controller;
        DefaultIdleState.ResetIdleAnims(command);
        //Debug.LogFormat("{0}: Approaching destination {1}", command.playerController.name, command.Destination.Value);
        command.anim.CrossFade("Idle", 0.2f);
        command.nav.SetDestination(command.Destination.Value);
        float distance = Vector3.Distance(command.playerController.transform.position,
                         command.Destination.Value);
        expectedTimeToComplete = distance / command.nav.speed;
        //Debug.LogFormat("Time to complete move is {0}", expectedTimeToComplete);
        startTime = Time.time;
    }

    public override void Update(BaseFSMController controller)
    {
        normalizedSpeed = Mathf.Clamp(command.nav.velocity.sqrMagnitude, 0, 1);
        command.anim.SetFloat("Blend", normalizedSpeed);
        if (!command.nav.pathPending)
        {
            if (command.nav.remainingDistance <= command.nav.stoppingDistance)
            {
                if (!command.nav.hasPath || command.nav.velocity.sqrMagnitude == 0f)
                {
                    command.TransitionToState(command.StateMap[Command.InternalState.idle.ToString()]);
                    if (command.playerController.InCover)
                    {
                        //Debug.LogFormat("{0} Reached cover {1} of type {2}", command.playerController.name, command.playerController.cover.name, command.playerController.cover.coverType);
                    }
                    command.complete = true;
                }
            } else
            {
                //Verify if navigation hung
                float runningFor = Time.time - startTime;
                if(runningFor > (expectedTimeToComplete + expectedTimeToComplete*(timeOverFlowPercent)/100))
                {
                    Debug.LogFormat("Navigation possibly hung. Ending move. Running For {0}, Expected time to complete {1}"
                        , runningFor, expectedTimeToComplete);
                    command.nav.isStopped = true;
                    command.complete = true;
                }
            }
        }
    }
}
