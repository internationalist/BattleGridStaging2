using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[System.Serializable]
public class MovementFSM : Command
{
    //CollisionProbe cp = null;
    public MovementFSM(Animator anim, NavMeshAgent nav, PlayerController controller, CommandTemplate weaponData) : base(anim, nav, controller, weaponData)
    {
        currentState = new IdleState();
        StateMap.Add(InternalState.idle.ToString(), this.currentState);
        StateMap.Add(InternalState.turnLeft.ToString(), new TurnForMoveState(TurnForMoveState.Dir.left));
        StateMap.Add(InternalState.turnRight.ToString(), new TurnForMoveState(TurnForMoveState.Dir.right));
        StateMap.Add(InternalState.approach.ToString(), new ApproachState());
        actionData = Resources.Load<ScriptableObject>("ScriptableObjects/MovementData");
        commandType = type.move;
    }
    private bool isColliding = false;
    private string collidingObjectName;


    public override void Setup(Transform rWeaponHolder, Transform lWeaponHolder)
    {
        base.Setup(rWeaponHolder, lWeaponHolder);
    }

    protected override void BeforeActivate()
    {
        RaycastHit hit = new RaycastHit();
        MovementData movementData = (MovementData)actionData;

        if (!playerController.isAgent && !EventSystem.current.IsPointerOverGameObject() && GeneralUtils.MousePointerOnGroundAndCharacters(out hit))
        {
            Vector3 destination = hit.point;
            if ("Ground".Equals(hit.transform.gameObject.tag))
            {
                playerController.CreateRangeIndicatorForMove();
                playerController.CreatePath(destination);
                if (destinationMarker == null)
                {
                    destinationMarker = GameObject.Instantiate(movementData.markerPrefab, destination, Quaternion.identity);
                }
                else
                {
                    ReuseDestinationMarker(destination);
                }
                playerController.playerMetaData.moveLocation = destination;
                Vector3 moveTargePoint = destination;
                moveTargePoint.y = playerTransform.position.y;
                float distanceToMove = Mathf.Round(Vector3.Distance(playerTransform.position, moveTargePoint));
                playerController.playerMetaData.distanceToMove = distanceToMove;
                Cursor.SetCursor(GameManager.I.cursorGroup.move, Vector3.zero, CursorMode.Auto);
            }
        }
    }

    public override void Complete()
    {
        base.Complete();
        playerController.playerMetaData.IncrementMoveCount();
        playerController.ResetPath();
        isColliding = false;
        if (destinationMarker != null)
        {
            destinationMarker.SetActive(false);
        }
    }

    public override void Cancel()
    {
        base.Cancel();
        playerController.ResetPath();
        playerController.ResetRangeIndicator();
        isColliding = false;
        if (destinationMarker != null)
        {
            destinationMarker.SetActive(false);
        }
        Cursor.SetCursor(GameManager.I.cursorGroup.select, Vector3.zero, CursorMode.Auto);
    }

    protected override void Activated()
    {
        playerController.CreatePath();
        playerController.ResetRangeIndicator();
    }


    protected override void Activate(Transform enemyTransform, Vector3? destination)
    {
        base.Activate(enemyTransform, destination);
        if (destination.HasValue)
        {
            if (playerController.playerMetaData.voice.responseOnMove != null && playerController.playerMetaData.voice.responseOnMove.Count > 0)
            {
                AudioManager.PlayVoice(playerController.playerMetaData.voice.responseOnMove, playerController.audioSource);

            }
            Destination = destination.Value;
            Destination = GeneralUtils.GetUniqueLocation(playerController, destination.Value);
            Debug.LogFormat("{0} Final destination value is {1}", playerController.name, Destination.Value);
            GameManager.occupancyMap[playerController.ID] = new Vector2(Mathf.Floor(Destination.Value.x), Mathf.Floor(Destination.Value.z));
            isActivated = true;
        }
        else
        {
            isActivated = false;
        }
        Cursor.SetCursor(GameManager.I.cursorGroup.select, Vector3.zero, CursorMode.Auto);
    }

    void OnCollide(bool didCollide, string collidingObjectName)
    {
        if(!playerController.name.Equals(collidingObjectName))
        {
            isColliding = didCollide;
            this.collidingObjectName = collidingObjectName;
        } else
        {
            isColliding = false;
        }
        
    }


    private void MarkDestination()
    {
        if (destinationMarker == null)
        {
            destinationMarker = GameObject.Instantiate(((MovementData)actionData).markerPrefab, Destination.Value, Quaternion.identity);
        }
        else
        {
            ReuseDestinationMarker(Destination.Value);
        }
    }

    private void ReuseDestinationMarker(Vector3 target)
    {
        destinationMarker.transform.position = target;
        destinationMarker.transform.position += destinationMarker.transform.up * .2f;
        destinationMarker.SetActive(true);
    }

    public override void ActivateWeapon()
    {
        
    }

    protected override bool Validate(Transform enemyTransform, Vector3? destination)
    {
        //Debug.LogFormat("{0}::In Validate points remaining {1}", playerController.name, pointsRemaining);
        if (playerController.playerMetaData.WithinRange())
        {
            return this.commandDataInstance.CanRun();
        }
        else
        {
            if (!playerController.isAgent)
            { //No need to show UI notification if AI
                UIManager.ShowMessage("Not within range");
                AudioManager.NoMoreAP();
            }
            Debug.Log("Cannot run command. Not within range");
            //Validation failed. Cancel this command/move.
            cancel = true;
        }
        return false;
    }

}
