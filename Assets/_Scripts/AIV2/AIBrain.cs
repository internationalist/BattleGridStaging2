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
    List<PlayerController> foes;
    List<PlayerController> friends;
    bool choosingEnemy;
    
    #endregion

    #region Unity events
    private void Start()
    {
        player = GetComponent<PlayerController>();
        allPlayers = FindObjectsOfType<PlayerController>();
        ChooseEnemy();
    }

    private void Update()
    {
        if(enemy.IsDead && !choosingEnemy)
        {
            ChooseEnemy();
        }
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

    public void TriggerMoveCommand(PlayerController controller, PlayerController enemy, Vector3 movemenLocation,
        Command.OnCompleteCallback onComplete)
    {
        TriggerCommand(Command.type.move, controller, enemy, movemenLocation, onComplete);
    }

    public void TriggerPrimaryAction(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.primaryaction, controller, enemy, Vector3.zero,
            ()=> { });
    }

    public void TriggerSpecialAction(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.specialaction, controller, enemy, Vector3.zero,
            () => { });
    }

    public void TriggerReload(PlayerController controller, PlayerController enemy)
    {
        TriggerCommand(Command.type.reload, controller, enemy, Vector3.zero,
            () => { });
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void TriggerCommand(Command.type cmdType,
                                  PlayerController controller,
                                  PlayerController enemy,
                                  Vector3 movementLocation,
                                  Command.OnCompleteCallback onComplete)
    {

    //commandStartTimeInSec = Time.realtimeSinceStartup; // We will enforce a timeout on the command due to a vague problem of commands not finishing.
        switch (cmdType)
        {
            case Command.type.move:
                controller.AddToCommandQueue(GeneralUtils.MOVESLOT, enemy.transform, movementLocation, onComplete);
                break;
            case Command.type.primaryaction:
                //Debug.Log("TriggerCommand::Running attack command and  setting attack achieved flag to true");
                controller.AddToCommandQueue(GeneralUtils.ATTACKSLOT, enemy.transform, enemy.transform.position, onComplete);
                break;
            case Command.type.specialaction:
                controller.AddToCommandQueue(GeneralUtils.ITEMSLOT, enemy.transform, enemy.transform.position, onComplete);
                break;
            case Command.type.reload:
                controller.AddToCommandQueue(GeneralUtils.RELOADSLOT, enemy.transform, null, onComplete);
                break;
        }
    }

    public DockPoint EvaluateCover(PlayerController pc,
        PlayerController enemy, CoverFramework thisCover)
    {
        bool ableToDock = false;
        DockPoint chosenDockPoint = null;
        List<DockPoint> dockPointClone = new List<DockPoint>(thisCover.coverDockPoints);
        //sort by ascending so that closest location comes first.
        DockPointCompareAscending lc = new DockPointCompareAscending(pc.transform.position);
        dockPointClone.Sort(lc.Compare);
        //PrintDockPointList(dockPointClone);
        for (int i = 0; i < dockPointClone.Count && !ableToDock; i++)
        {
            bool selfOccupied;
            //Debug.LogFormat("checking dockpoint {0} with enemy {1}", dockPoint.position, state.target.name);
            if (!GeneralUtils.IsSpotOccupied(dockPointClone[i].position, pc.ID, out selfOccupied) && !selfOccupied)
            {
                //run a raycast from this dock point to the closest enemy. And make sure there is the cover in between.
                Vector3 dockPos = dockPointClone[i].position;
                CoverFramework[] covers = GeneralUtils.CheckCoversBetweenPoints(dockPos, enemy.transform.position);
                //if(covernames.Length == 1) 
                for (int j = 0; j < covers.Length; j++) //Next we need to make sure there is a clear shot from this position to the enemy
                {
                    if (CoverFramework.TYPE.full.Equals(covers[j].coverType)) // No clear shot from this dockpoint
                    {
                        ableToDock = false;
                        break;
                    }
                    if (chosenDockPoint == null && thisCover.ID.Equals(covers[0].ID))
                    {
                        if (IsExposedToEnemy(dockPos))
                        {
                            continue;
                        }
                        else
                        {
                            chosenDockPoint = dockPointClone[i];
                            ableToDock = true;
                        }
                    }
                }
            }
            if (ableToDock)
            {
                return chosenDockPoint;
            }
        }
        //At this point we cannot dock at this cover.
        return null;
    }

    private bool IsExposedToEnemy(Vector3 dockPos)
    {
        for (int i = 0; i < foes.Count; i++)
        {
            string[] covernames2 = GeneralUtils.CheckCoversBetweenTwoPoints(dockPos, foes[i].transform.position);
            if (covernames2 == null || covernames2.Length == 0)
            {
                return true;
            }
        }
        return false;
    }
}
