using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIState
{
    public PlayerController agent;
    public List<PlayerController> friends;
    public List<PlayerController> foes;
    public PlayerController target;
    public float distanceToTarget;
    public Vector3 dirToTarget;
    public Vector3 moveLocation;
    public float movementDistance;
    public Command.type? cmdType;
    public CommandTemplate weaponTemplate;
    public CommandDataInstance weaponInstance;
    public bool achievedAttack;
    public bool achievedCover;
    //public bool noCoverToDockTo;
    public PlayerController closestEnemy;
    public DockPoint occupiedDockPoint;


    public AIState(PlayerController agent)
    {
        this.friends = new List<PlayerController>();
        this.foes = new List<PlayerController>();
        this.agent = agent;
        List<PlayerController> foes = new List<PlayerController>();
        List<PlayerController> friends = new List<PlayerController>();
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
        }
    }
    /// <summary>
    /// Commands are contained within Turns.
    /// </summary>
    public void InitForCommand()
    {
        target = null;
        weaponTemplate = null;
        distanceToTarget = float.MaxValue;
        movementDistance = 0;
        cmdType = null;
        achievedCover = false;
    }

    public void RemoveDockPoint()
    {
        if (occupiedDockPoint != null)
        {
            occupiedDockPoint.isOccupied = false;
            occupiedDockPoint.controllerID = -1;
        }
    }

    public void InitForTurn()
    {
        achievedAttack = false;
        
        //noCoverToDockTo = false;
        RemoveDockPoint();
    }

    public override string ToString()
    {
        string output = string.Format("Target {0}\n distanceToTarget {1}\n", target, distanceToTarget);
        return output;
    }

}
