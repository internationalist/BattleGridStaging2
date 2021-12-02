using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AIUtils
{
    public static bool trace = true;


    public static void ChooseEnemyV2(AIState state, PlayerController agent, List<Factor> decisionMatrix)
    {
        float distance = float.MaxValue;
        PlayerController closest = null;
        List<EntityScoreMap> targetList = new List<EntityScoreMap>();
        foreach (PlayerController foe in state.foes)
        {
            float thisDistance = Vector3.Distance(agent.rangeMarker.transform.position, foe.transform.position);
            //Calculate closest
            if (thisDistance < distance)
            {
                distance = thisDistance;
                closest = foe;
            }
            float score = 0;
            //Use decision materix to select target
            for(int j = 0; j < decisionMatrix.Count; j++)
            {
                score += decisionMatrix[j].CalculateScore(agent, foe) * decisionMatrix[j].weight;
            }

            targetList.Add(new EntityScoreMap(foe, score));
        }

        //Sort descending according to score
        targetList.Sort((x, y) => {
            if(x.score == y.score)
            {
                return 0;
            } else if(x.score > y.score)
            {
                return -1;
            } else
            {
                return 1;
            }
        });

        state.target = targetList[0].entity;
        state.closestEnemy = closest;

        DirectionAndDistanceToTarget(state, agent);

    }
 
    public static void DirectionAndDistanceToTarget(AIState state, PlayerController agent)
    {
        if(state.target != null)
        {
            Vector3 dir = (state.target.transform.position - agent.transform.position);
            state.dirToTarget = dir.normalized;
            state.distanceToTarget = Vector3.Distance(agent.transform.position, state.target.transform.position);
        }
    }

    public static void DirectionAndDistanceToLocation(AIState state, PlayerController agent)
    {
        Vector3 dir = (state.moveLocation - agent.transform.position);
        state.dirToTarget = dir.normalized;
        state.distanceToTarget = Vector3.Distance(agent.transform.position, state.target.transform.position);
    }

    public static void SelectAttack(AIState state, PlayerController controller)
    {
        RifleAttackFSM primaryAttack = (RifleAttackFSM)controller.commands[GeneralUtils.ATTACKSLOT];

        state.cmdType = Command.type.primaryattack;

        state.weaponTemplate = controller.GetWeaponTemplateForCommand(GeneralUtils.ATTACKSLOT);
        
        state.weaponInstance = primaryAttack.commandDataInstance;
    }


    public static void ValidateMoveLocationForAPAndNavMesh(AIState state, bool snapToGrid)
    {
        PlayerController agent = state.agent;
        Transform transform = agent.transform;
        //Debug.LogFormat("{0}:: move location is {1}", state.agent.name, state.moveLocation);
        state.movementDistance = Mathf.Round(Vector3.Distance(transform.position, state.moveLocation));
        float decrement = 0;
        Vector3 dir = (state.moveLocation - transform.position).normalized;

        int infiniteLoopCounter = 0;
        int maxLoops = 1000;

        //Debug.LogFormat("{0}:: Ap needed is {1}. Available is {2}", state.agent.name, state.apNeeded, agent);

        while (state.movementDistance > agent.playerMetaData.movementRange)
        {
            //Log("ValidateMoveLocationForAPAndNavMesh", "Ap needed is more than available. Calibrating.");
            Vector3 newLocation = transform.position + dir * (agent.playerMetaData.movementRange - decrement);
            if (snapToGrid)
            {
                state.moveLocation = GeneralUtils.SnapToGrid(newLocation);
            }
            else
            {
                state.moveLocation = newLocation;
            }

            DirectionAndDistanceToLocation(state, agent);

            state.movementDistance = Mathf.Round(Vector3.Distance(transform.position, state.moveLocation));
            ++decrement;
            ++infiniteLoopCounter;
            if (infiniteLoopCounter > maxLoops)
            {
                break;
            }
        }
        Debug.DrawLine(transform.position + Vector3.up * 1.5f, state.moveLocation + Vector3.up * 1.5f, Color.red, 15, false);

        float distToTarget = Mathf.Round((state.target.transform.position - state.moveLocation).magnitude);

        float newDistToTarget = 0;

        infiniteLoopCounter = 0;

        Vector3 originalLocation = state.moveLocation;
        while (!GeneralUtils.InsideNavMesh(state.moveLocation, agent.nav) || newDistToTarget > distToTarget)
        {

            Vector2 randPos = Random.insideUnitCircle * 2f;
            /*state.moveLocation = GeneralUtils.SnapToGrid(new Vector3(state.moveLocation.x
                                                          + Mathf.Round(randPos.x), state.moveLocation.y,
                                                          state.moveLocation.z + Mathf.Round(randPos.y)));*/
            state.moveLocation = new Vector3(Mathf.Round(originalLocation.x + randPos.x), state.moveLocation.y, Mathf.Round(originalLocation.z + randPos.y));

            newDistToTarget = Mathf.Round((state.target.transform.position - state.moveLocation).magnitude);

            Log("ValidateMoveLocationForAPAndNavMesh", "Move location not inside navmesh. Calibrating.");
            ++infiniteLoopCounter;
            if (infiniteLoopCounter > maxLoops)
            {
                break;
            }
        }

        //Log("ValidateMoveLocationForAPAndNavMesh", "New move location after validation is {0} and AP used {1}", state.moveLocation, state.apNeeded);
        Debug.DrawLine(transform.position + Vector3.up * 1.5f, state.moveLocation + Vector3.up * 1.5f, Color.yellow, 5, false);
        DirectionAndDistanceToLocation(state, agent);
    }

    public static void RushEnemy(AIState state, PlayerController agent, CoverFramework enemyCover, bool aggressive = false)
    {
        List<DockPoint> dockPointClone = new List<DockPoint>(enemyCover.flankPoints);
        DockPointCompare lc = null;
        //Sort dock points of the cover from closet to enemy to farthest
        if (aggressive)
        {
            lc = new DockPointCompareAscending(state.target.transform.position);
        } else
        {
            lc = new DockPointCompareDescending(state.target.transform.position);
        }
        
        //sort by descending so that farthest location comes first.
        dockPointClone.Sort(lc.Compare);
        foreach (DockPoint dockPoint in dockPointClone)
        {
            if (!GeneralUtils.IsSpotOccupied(dockPoint.position))
            {
                //run a raycast from this dock point to the enemy. And make sure there is NO high cover in between.
                Vector3 dockPos = dockPoint.position;
                if (!GeneralUtils.CheckCoverBetweenTwoPoints(dockPos, state.target.transform.position, true))
                {
                    //Log("DockWithEnemyCover", "Moving to dockpoint at {0} belonging to cover {1}", dockPos, enemyCover.name);
                    state.moveLocation = dockPos;
                    ValidateMoveLocationForAPAndNavMesh(state, false);
                    state.cmdType = Command.type.move;
                    RecordCover(state, dockPoint);
                    break;
                }
            }
        }
    }


    public static bool FlankEnemyFromCover(AIState state, PlayerController agent, RangedAttackState.Agression agression)
    {
        bool aggressive = (agression == RangedAttackState.Agression.LOW ? false : true);
        CoverFramework enemyCover = null;
        bool foundTacticalCover = false; ;
        List<CoverFramework> coverCloneList = new List<CoverFramework>(AIManager.I.covers);
        CoverCompare cca = null;
        if (aggressive) //More aggresive get cover closer to target
        {
            cca = new CoverCompare(state.target.transform.position);
        } else //less aggressive
        {
            cca = new CoverCompare(state.agent.transform.position);
        }
        
        coverCloneList.Sort(cca.Compare);
        foreach (CoverFramework cover in coverCloneList)
        {
            if (state.target.inCover)
            {
                if (state.target.cover.name.Equals(cover.name) && !state.target.cover.coverType.Equals(CoverFramework.TYPE.full)) //Cover found is the same one that enemy has taken shelter in.
                {
                    enemyCover = cover;
                    continue;
                }
            }
            if (cover.coverType.Equals(CoverFramework.TYPE.full)) //ignore cover which is full. This is not tactical.
            {
                continue;
            }
            if (EvaluateIfFlankCover(state, cover, agression == RangedAttackState.Agression.HIGH? true:false))
            {
                foundTacticalCover = true;
                break;
            }
        }
        if (!foundTacticalCover) //If we have not found tactical cover than use the enemy cover itself.
        {
            if (enemyCover != null)
            {
                foundTacticalCover = EvaluateIfFlankCover(state, enemyCover, aggressive);
            }
        }
        return foundTacticalCover;
    }

    public static void ApproachEnemy(AIState state, PlayerController agent)
    {
        //Debug.LogFormat("{0} AIAttackState:Update->Attack command out of range", agent.name);
        float deltaTravel = Mathf.Max(1, (state.distanceToTarget - state.weaponTemplate.damageParameters.optimalRange));

        state.moveLocation = GeneralUtils.SnapToGrid(agent.transform.position + state.dirToTarget * deltaTravel);
        //Calibrate for target location inside nav mesh and available AP.
        ValidateMoveLocationForAPAndNavMesh(state, true);
        state.cmdType = Command.type.move;
    }

    public static void Retreat(AIState state, PlayerController agent)
    {
        List<CoverFramework> coverCloneList = new List<CoverFramework>(AIManager.I.covers);
        CoverCompare cca = new CoverCompare(agent.transform.position.normalized, true);
        coverCloneList.Sort(cca.Compare);
        foreach (CoverFramework cover in coverCloneList)
        {
            if(RetreatToCover(state, cover))
            {
                break;
            }
        }
    }

    private static bool RetreatToCover(AIState state, CoverFramework closestCover)
    {
        bool ableToDock = false;
        List<DockPoint> dockPointClone = new List<DockPoint>(closestCover.coverDockPoints);
        //sort by ascending so that closest location comes first.
        DockPointCompareAscending lc = new DockPointCompareAscending(state.agent.transform.position);
        dockPointClone.Sort(lc.Compare);
        foreach (DockPoint dockPoint in dockPointClone)
        {
            bool selfOccupied;
            if (!GeneralUtils.IsSpotOccupied(dockPoint.position, state.agent.ID, out selfOccupied) && !selfOccupied)
            {
                //run a raycast from this dock point to the closest enemy. And make sure there is the cover in between.
                //Debug.LogFormat("checking dockpoint {0} with enemy {1}", dockPoint.position, state.closestEnemy.name);
                Vector3 dockPos = dockPoint.position;
                if (GeneralUtils.CheckCoverBetweenPointsByName(dockPos, state.closestEnemy.transform.position, closestCover.name))
                {
                    //Log("DockWithCover", "{0} Found cover to dock at {1} for cover name {2}", agent.name, dockPoint.position, closestCover.name);
                    //Debug.LogFormat("dock point chosen is {0} cover name is {1}", dockPoint.position, closestCover.name);
                    state.moveLocation = dockPos;
                    DirectionAndDistanceToLocation(state, state.agent);
                    ValidateMoveLocationForAPAndNavMesh(state, false);
                    state.cmdType = Command.type.move;
                    state.achievedCover = true;
                    RecordCover(state, dockPoint);
                    ableToDock = true;
                    break;
                }
            }
        }
        //At this point there is no cover left to dock with
        //state.noCoverToDockTo = true;
        return ableToDock;
    }

    private static bool EvaluateIfFlankCover(AIState state, CoverFramework thisCover, bool aggressive)
    {
        bool ableToDock = false;
        DockPoint chosenDockPoint = null;
        List<DockPoint> dockPointClone = new List<DockPoint>(thisCover.coverDockPoints);
        //sort by ascending so that closest location comes first.
        DockPointCompareAscending lc = new DockPointCompareAscending(state.agent.transform.position);
        dockPointClone.Sort(lc.Compare);
        //PrintDockPointList(dockPointClone);
        for (int i = 0; i <  dockPointClone.Count && !ableToDock; i++)
        {
            bool selfOccupied;
            //Debug.LogFormat("checking dockpoint {0} with enemy {1}", dockPoint.position, state.target.name);
            if (!GeneralUtils.IsSpotOccupied(dockPointClone[i].position, state.agent.ID, out selfOccupied) && !selfOccupied)
            {
                //run a raycast from this dock point to the closest enemy. And make sure there is the cover in between.
                Vector3 dockPos = dockPointClone[i].position;
                CoverFramework[] covers = GeneralUtils.CheckCoversBetweenPoints(dockPos, state.target.transform.position);
                //if(covernames.Length == 1) 
                for(int j = 0; j < covers.Length; j++) //Next we need to make sure there is a clear shot from this position to the enemy
                {
                    if(CoverFramework.TYPE.full.Equals(covers[j].coverType)) // No clear shot from this dockpoint
                    {
                        ableToDock = false;
                        break;
                    }
                    if(chosenDockPoint == null && thisCover.name.Equals(covers[0]))
                    {
                        if(!aggressive && IsExposedToEnemy(state, state.moveLocation))
                        {
                            continue;
                        } else
                        {
                            chosenDockPoint = dockPointClone[i];
                            ableToDock = true;
                        }
                    }
                }
            }
            if(ableToDock)
            {
                MoveToDockPoint(state, chosenDockPoint);
            }
        }
        //At this point we cannot dock at this cover.
        return ableToDock;
    }

    private static void MoveToDockPoint(AIState state, DockPoint chosen)
    {
        state.moveLocation = chosen.position;
        DirectionAndDistanceToLocation(state, state.agent);
        ValidateMoveLocationForAPAndNavMesh(state, false);
        state.cmdType = Command.type.move;
        state.achievedCover = true;
        RecordCover(state, chosen);
    }

    private static bool IsExposedToEnemy(AIState state, Vector3 dockPos)
    {
        for (int i = 0; i < state.foes.Count; i++)
        {
            string[] covernames2 = GeneralUtils.CheckCoversBetweenTwoPoints(dockPos, state.foes[i].transform.position);
            if (covernames2 == null || covernames2.Length == 0)
            {
                return true;
            }
        }
        return false;
    }

    private static void PrintDockPointList(List<DockPoint> dl)
    {
        foreach (DockPoint dp in dl)
        {
            Debug.LogFormat("{0}", dp.position);
        }
    }


    private static void RecordCover(AIState state, DockPoint dockPoint)
    {
        state.RemoveDockPoint();
        dockPoint.isOccupied = true;
        dockPoint.controllerID = state.agent.ID;
        state.occupiedDockPoint = dockPoint;
    }



    private static void Log(string function, string message, params object[] args)
    {
        string logHeader = "AIUtils::" + function + "->";
        if (trace)
        {
            //Debug.LogFormat(logHeader + message, args);
        }
    }
}
