using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ThrowItemFSM : Command
{
    public ThrowItemFSM(Animator anim, NavMeshAgent nav, PlayerController controller, CommandTemplate weaponData) : base(anim, nav, controller, weaponData)
    {
        currentState = new IdleState();
        StateMap.Add(state.idle.ToString(), this.currentState);
        StateMap.Add(state.turnLeft.ToString(), new TurnForMoveState(TurnForMoveState.Dir.left, state.throwItem));
        StateMap.Add(state.turnRight.ToString(), new TurnForMoveState(TurnForMoveState.Dir.right, state.throwItem));
        StateMap.Add(state.throwItem.ToString(), new ThrowItemState());
        actionData = Resources.Load<ScriptableObject>("ScriptableObjects/AttackData");
        commandType = type.throwItem;
    }

    protected override void BeforeActivate()
    {
        RaycastHit hit = new RaycastHit();
        MovementData movementData = (MovementData)actionData;

        if (!EventSystem.current.IsPointerOverGameObject() && GeneralUtils.MousePointerOnGroundAndCharacters(out hit))
        {
            if ("Ground".Equals(hit.transform.gameObject.tag))
            {
                playerController.CreateRangeIndicator(this.commandTemplate.damageParameters.optimalRange);
                GameManager.CreateGrenadePath(hit.point);
                if (destinationMarker == null)
                {
                    destinationMarker = GameObject.Instantiate(movementData.markerPrefab, hit.point, Quaternion.identity);
                }
                else
                {
                    destinationMarker.transform.position = hit.point;
                    destinationMarker.transform.position += destinationMarker.transform.up * .2f;
                    destinationMarker.SetActive(true);
                }
                playerController.playerMetaData.moveLocation = hit.point;
                Vector3 moveTargePoint = hit.point;
                moveTargePoint.y = playerTransform.position.y;
                float distanceToThrow = Mathf.Round(Vector3.Distance(playerTransform.position, moveTargePoint));
                playerController.playerMetaData.grenadeTargetDistance = distanceToThrow;
            }
        }
    }

    protected void TurnOffMarker()
    {
        if (destinationMarker != null)
        {
            destinationMarker.SetActive(false);
        }
    }

    protected override void Activated()
    {
        playerController.ResetPath();
        playerController.ResetRangeIndicator();
        TurnOffMarker();
    }

    public override void Complete()
    {
        base.Complete();
        playerController.playerMetaData.IncrementItemUseCount();
        playerController.ResetPath();
        playerController.ResetRangeIndicator();
        TurnOffMarker();
    }

    public override void Cancel()
    {
        base.Cancel();
        playerController.ResetPath();
        playerController.ResetRangeIndicator();
        TurnOffMarker();
    }

    protected override void Activate(Transform enemyTransform, Vector3? destination)
    {
        if (playerController.playerMetaData.voice.responseOnAction != null && playerController.playerMetaData.voice.responseOnAction.Count > 0)
        {
            int audioIndex = Random.Range(0, playerController.playerMetaData.voice.responseOnAction.Count);
            playerController.audioSource.PlayOneShot(playerController.playerMetaData.voice.responseOnAction[audioIndex]);
        }
        base.Activate(enemyTransform, destination);
        if (enemyTransform != null)
        {
            if (destination.HasValue)
            {
                Destination = destination.Value;
            }
            this.EnemyTransform = enemyTransform;
            isActivated = true;
        }
        else if (destination.HasValue)
        {
            Destination = destination.Value;
            isActivated = true;
        }
        else
        {
            isActivated = false;
        }
    }

    public override void ActivateWeapon()
    {
        commandTemplate.Launch(this, this.commandDataInstance);
    }

    protected override bool Validate(Transform enemyTransform, Vector3? destination)
    {
        float range = this.commandTemplate.damageParameters.optimalRange;
        if (playerController.playerMetaData.grenadeTargetDistance > range)
        {
            if (!playerController.isAgent)
            { //No need to show UI notification if AI
                UIManager.ShowMessage("Not within range");
                AudioManager.NoMoreAP();
            }
            cancel = true;
        }
        else if (playerController.playerMetaData.CanUseItem())
        {
            return true;
        }
        else
        {
            if (!playerController.isAgent)
            { //No need to show UI notification if AI

                UIManager.ShowMessage("Not ready yet");
                AudioManager.NoMoreAP();
            }
        }
        return false;
    }
}
