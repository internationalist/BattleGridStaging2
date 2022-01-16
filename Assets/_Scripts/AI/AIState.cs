using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIState
{
    public PlayerController agent;
    public List<PlayerController> friends;
    public List<PlayerController> foes;
    public List<PlayerController> targets;
    public PlayerController target;
    public float distanceToTarget;
    public Vector3 dirToTarget;
    public Vector3 moveLocation;
    public float movementDistance;
    public Command.type? cmdType;
    public Command.type attackType;
    public CommandTemplate weaponTemplate;
    public CommandDataInstance weaponInstance;
    public bool achievedAttack;
    public bool achievedCover;
    public PlayerController closestEnemy;
    public DockPoint occupiedDockPoint;


    public AIState(PlayerController agent)
    {
        this.friends = new List<PlayerController>();
        this.foes = new List<PlayerController>();
        this.agent = agent;
        if(TurnBasedSystem.I != null
            && TurnBasedSystem.I.teams != null
            &  TurnBasedSystem.I.teams.Count > 0)
        {
            foreach (Team t in TurnBasedSystem.I.teams)
            {
                foreach (PlayerController pc in t.players)
                {
                    if (pc.isAgent)
                    {
                        this.friends.Add(pc);
                    }
                    else
                    {
                        this.foes.Add(pc);
                    }
                }
            }
            targets = new List<PlayerController>(foes);
        }
    }
    /// <summary>
    /// Commands are contained within Turns.
    /// </summary>
    public void InitForNewTurn()
    {
        target = null;
        weaponTemplate = null;
        distanceToTarget = float.MaxValue;
        movementDistance = 0;
        cmdType = null;
        achievedCover = false;
        targets = new List<PlayerController>(foes); //refresh target list at start of turn
    }

    public void RemoveDockPoint()
    {
        if (occupiedDockPoint != null)
        {
            occupiedDockPoint.isOccupied = false;
            occupiedDockPoint.controllerID = -1;
        }
    }


    public override string ToString()
    {
        string output = string.Format("Target {0}\n distanceToTarget {1}\n", target, distanceToTarget);
        return output;
    }

}
