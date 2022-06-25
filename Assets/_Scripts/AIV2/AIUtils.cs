using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AIUtils
{
    public static float ApproachEnemy(PlayerController controller,
                                      PlayerController enemy,
                                      CommandTemplate commandTmpl,
                                      Command.OnCompleteCallback callBack)
    {
        float startTime = Time.time;
        var distanceToEnemy = Vector3.Distance(controller.transform.position, enemy.transform.position);
        if (Mathf.Round(distanceToEnemy) > commandTmpl.damageParameters.optimalRange)
        {
            var movementDistance = distanceToEnemy - commandTmpl.damageParameters.optimalRange;
            Vector3 dirOfMovement = (enemy.transform.position - controller.transform.position).normalized;
            Vector3 movementLocation = controller.transform.position + dirOfMovement * movementDistance;
            AIManager.TriggerMoveCommand(controller, enemy, movementLocation, callBack);
        }
        return startTime;
    }

    public static void ApproachLocation(PlayerController controller,
                                      Vector3 movementLocation,
                                      Command.OnCompleteCallback callBack)
    {
        AIManager.TriggerMoveCommand(controller, movementLocation, callBack);
    }

    public static DockPoint EvaluateCover(PlayerController thisPlayer,
                                   PlayerController enemy,
                                   CoverFramework thisCover,
                                   List<PlayerController> foes,
                                   CommandTemplate commandMetaData
                                   )
    {
        bool ableToDock = false;
        DockPoint chosenDockPoint = null;
        List<DockPoint> dockPointClone = new List<DockPoint>(thisCover.coverDockPoints);
        //sort by ascending so that closest location comes first.
        DockPointCompareAscending lc = new DockPointCompareAscending(thisPlayer.transform.position);
        dockPointClone.Sort(lc.Compare);
        //PrintDockPointList(dockPointClone);
        for (int i = 0; i < dockPointClone.Count && !ableToDock; i++)
        {
            bool selfOccupied;
            //Debug.LogFormat("checking dockpoint {0} with enemy {1}", dockPoint.position, state.target.name);
            if (!GeneralUtils.IsSpotOccupied(dockPointClone[i].position, thisPlayer.ID, out selfOccupied) && !selfOccupied)
            {
                //run a raycast from this dock point to the closest enemy. And make sure there is the cover in between.
                Vector3 dockPos = dockPointClone[i].position;
                CoverFramework[] covers = GeneralUtils.CheckCoversBetweenPoints(dockPos, enemy.transform.position);
                for (int j = 0; j < covers.Length; j++) //Next we need to make sure there is a clear shot from this position to the enemy
                {
                    if (CoverFramework.TYPE.full.Equals(covers[j].coverType)) // No clear shot from this dockpoint
                    {
                        ableToDock = false;
                        break;
                    }
                    if (chosenDockPoint == null && thisCover.ID.Equals(covers[0].ID))
                    {
                        if (IsExposedToEnemy(foes, dockPos))
                        {
                            continue;
                        }
                        else
                        {
                            chosenDockPoint = dockPointClone[i];
                            //Check if within firing range or not.
                            var distanceToEnemy = Vector3.Distance(chosenDockPoint.position, enemy.transform.position);
                            /* weapon range check */
                            if (distanceToEnemy > commandMetaData.damageParameters.optimalRange)
                            {
                                continue;
                            }
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

    private static bool IsExposedToEnemy(List<PlayerController> foes, Vector3 dockPos)
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
