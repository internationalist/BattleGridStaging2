using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    #region state
    public PlayerController enemy;
    PlayerController player;
    PlayerController[] allPlayers;
    public List<PlayerController> foes;
    List<PlayerController> friends;
    bool choosingEnemy;
    public CommandTemplate commandTmpl;

    AISpecialAction specialAction;
    public bool isSpecialActionComplete;
    [Tooltip("The ai Brain runs a pass after this time in seconds")]
    public float aiCycleDelay;
    float lastRunTime;
    public Vector3 movementLocation;

    #endregion

    #region Unity events
    private void Start()
    {
        player = GetComponent<PlayerController>();
        allPlayers = FindObjectsOfType<PlayerController>();
        specialAction = GetComponent<AISpecialAction>();
        ChooseEnemy();
        commandTmpl = player.GetWeaponTemplateForCommand(GeneralUtils.ATTACKSLOT);
    }

    private void Update()
    {
        if(Time.time - lastRunTime > aiCycleDelay)
        {
            lastRunTime = Time.time;
            if (enemy != null
                && enemy.IsDead
                && !choosingEnemy)
            {
                ChooseEnemy();
            }
            if(specialAction != null)
            {
                LaunchSpecialAction();
                if (isSpecialActionComplete)
                {
                    isSpecialActionComplete = false;
                    commandTmpl = player.GetWeaponTemplateForCommand(GeneralUtils.ATTACKSLOT);
                }
            }
        }
    }

    public void SetMovementLocation(Vector3 movementLocation)
    {
        this.movementLocation = movementLocation;
    }



    private void ChooseEnemy()
    {
        choosingEnemy = true;
        foes = new List<PlayerController>();
        friends = new List<PlayerController>();
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i] != null
                && !allPlayers[i].IsDead
                && allPlayers[i].ID != player.ID)
            {
                if(allPlayers[i].teamID != player.teamID)
                {
                    enemy = allPlayers[i];
                    foes.Add(allPlayers[i]);
                } else
                {
                    friends.Add(allPlayers[i]);
                }
            }
        }
        choosingEnemy = false;
    }
    #endregion

    #region AI Brain Stuff

    void LaunchSpecialAction()
    {
        if(specialAction.IsActive())
        {
            specialAction.canRun = true;
            commandTmpl = player.GetWeaponTemplateForCommand(GeneralUtils.ITEMSLOT);
        } else
        {
            specialAction.canRun = false;
        }
    }



    #endregion

}
