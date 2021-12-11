using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class RifleAttackFSM : Command
{

    public Queue<ApplicableDamage> attackAmounts;
    public bool actionCam;
    
    public RifleAttackFSM(Animator anim, NavMeshAgent nav, PlayerController controller, CommandTemplate weaponData) : base(anim, nav, controller, weaponData)
    {
        currentState = new IdleState();
        StateMap.Add(state.idle.ToString(), this.currentState);
        StateMap.Add(state.turnLeft.ToString(), new TurnForMoveState(TurnForMoveState.Dir.left, state.attack));
        StateMap.Add(state.turnRight.ToString(), new TurnForMoveState(TurnForMoveState.Dir.right, state.attack));
        StateMap.Add(state.attack.ToString(), new RifleAttackState());
        actionData = Resources.Load<ScriptableObject>("ScriptableObjects/AttackData");
        commandType = type.primaryattack;
        attackAmounts = new Queue<ApplicableDamage>();
    }

    public RifleAttackFSM() { }



    protected override void BeforeActivate()
    {
        RaycastHit hit = new RaycastHit();
        MovementData movementData = (MovementData)actionData;

        if (!playerController.isAgent && !EventSystem.current.IsPointerOverGameObject() && GeneralUtils.MousePointerOnGroundAndCharacters(out hit))
        {
            if ("Enemy".Equals(hit.transform.gameObject.tag))
            {      
                Vector3 targetDir = new Vector3(hit.transform.position.x, playerController.rangeMarker.transform.position.y, hit.transform.position.z);
                Vector3 direction = (targetDir - playerController.rangeMarker.transform.position).normalized;
                float distance = Mathf.Floor((targetDir - playerController.rangeMarker.transform.position).magnitude);
                if(distance <= commandTemplate.damageParameters.optimalRange)
                {
                    playerController.rangeMarker.inRange = true;
                } else
                {
                    playerController.rangeMarker.inRange = false;
                }
                playerController.rangeMarker.range = Mathf.Min(commandTemplate.damageParameters.optimalRange,distance);
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                playerController.rangeMarker.transform.rotation = lookRotation;
                playerController.rangeMarker.lookRotation = lookRotation;
                playerController.rangeMarker.active = true;
                Vector3 effectLocation = hit.point;
                effectLocation.y = playerTransform.position.y;
                if (destinationMarker == null)
                {
                    destinationMarker = GameObject.Instantiate(movementData.markerPrefab, effectLocation, Quaternion.identity);
                }
                else
                {
                    destinationMarker.transform.position = effectLocation;
                    destinationMarker.transform.position += destinationMarker.transform.up * .2f;
                    destinationMarker.SetActive(true);
                }
                Cursor.SetCursor(GameManager.I.cursorGroup.target, Vector3.zero, CursorMode.Auto);
                this.EnemyTransform = hit.transform;

                Vector3 dir = hit.transform.position - playerController.transform.position;
                CoverMeta coverMeta = GeneralUtils.IsCoverInTheMiddle(this, dir);
                DamageMetaUI dm;
                if (coverMeta.coverInWay)
                {
                    dm = GeneralUtils.CalculateDamageChance(commandTemplate.damageParameters, distance, coverMeta.cover);
                } else
                {
                    dm = GeneralUtils.CalculateDamageChance(commandTemplate.damageParameters, distance, null);
                }

                string attackMeta = string.Format("CRITICAL DAMAGE:{0}%\nHIT CHANCE: {1}%\n DMG MULTIPLIER:{2}", dm.criticalDmgChance.ToString("0.##"), dm.dmgChance.ToString("0.##"), dm.dmgMultiplier.ToString("0.##"));
                
                UIManager.ShowAttackData(attackMeta, hit.transform.gameObject);
            } else
            {
                TurnOffMarker();
                playerController.rangeMarker.active = false;
                UIManager.HideAttackData();
            }
        } else
        {
            playerController.rangeMarker.active = false;
            TurnOffMarker();
            UIManager.HideAttackData();
        }
    }

    protected override void Activated()
    {
        TurnOffMarker();
        UIManager.HideAttackData();
        playerController.rangeMarker.active = false;
    }

    public override void Complete()
    {
        base.Complete();
        playerController.playerMetaData.IncrementAttackCount();
        TurnOffMarker();
        UIManager.HideAttackData();
        playerController.rangeMarker.active = false;
    }

    public override void Cancel()
    {
        base.Cancel();
        TurnOffMarker();
        UIManager.HideAttackData();
        playerController.rangeMarker.active = false;
    }

    protected override void Activate(Transform enemyTransform, Vector3? destination)
    {
        if (playerController.playerMetaData.responseOnAction != null && playerController.playerMetaData.responseOnAction.Count > 0)
        {
            AudioManager.PlayHurtSound(playerController.playerMetaData.responseOnAction, playerController.audioSource);
        }
        base.Activate(enemyTransform, destination);
        if (enemyTransform != null && destination.HasValue)
        {
            this.Destination = destination.Value;
            this.EnemyTransform = enemyTransform;
            isActivated = true;
            CommandTemplate wt = this.commandTemplate;
            int enemyHp = this.enemyController.playerMetaData.Hp;
            int totalDamage = 0;
            for (int i = 0; i <  wt.maxBurstFire; i++)
            {
                float distanceCovered, damageReductionPct;
                CoverMeta cm = GeneralUtils.CalculateDmgPercent(this, out distanceCovered, out damageReductionPct);
                int damageAmt = 0;
                bool critical;
                
                GeneralUtils.CalculateDamage(wt.damageParameters, distanceCovered,
                    damageReductionPct, out damageAmt, out critical);
                attackAmounts.Enqueue(new ApplicableDamage(damageAmt, critical, cm));
                totalDamage += damageAmt;
            }
            float actionCamRandom = Random.Range(0f, 1f);
            
            if (totalDamage >= enemyHp && GameManager.I.actionCamChance > actionCamRandom)
            {
                actionCam = true;
            } else
            {
                actionCam = false;
            }
        }
        else
        {
            Destination = destination.Value;
            this.EnemyTransform = null;
            isActivated = false;
        }
    }

    public override void ActivateWeapon()
    {
        commandDataInstance.UseAmmo();
        UIManager.DisplayAmmo(commandDataInstance.ammoCount);
        commandTemplate.Launch(this, this.commandDataInstance);
    }


    private void TurnOffMarker()
    {
        if (destinationMarker != null)
        {
            destinationMarker.SetActive(false);
        }
        Cursor.SetCursor(GameManager.I.cursorGroup.select, Vector3.zero, CursorMode.Auto);
    }

    protected override bool Validate(Transform enemyTransform, Vector3? destination)
    {
        //Debug.LogFormat("{0} Can attack: {1}", playerController.name, playerController.playerMetaData.CanAttack());
        if (playerController.playerMetaData.CanAttack() && commandDataInstance.isAmmoLeft())
        {
            return true;
        }
        else
        {
            if (!playerController.isAgent)
            { //No need to show UI notification if AI
                if(playerController.playerMetaData.CanAttack())
                {
                    UIManager.ShowMessage("Ammo depleted.");
                } else
                {
                    UIManager.ShowMessage("Attack completed.");
                }
                
                AudioManager.NoMoreAP();
            }
            Debug.Log("Cannot run command. Not enough AP");
            //Validation failed. Cancel this command/move.
            cancel = true;
        }
        return false;
    }
}
