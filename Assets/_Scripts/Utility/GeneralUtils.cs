using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class GeneralUtils {

    public static readonly int DEFAULTSLOT = 0;
    public static readonly int RELOADSLOT = 1;
    public static readonly int MOVESLOT = 2;
    public static readonly int ATTACKSLOT = 3;
    public static readonly int ITEMSLOT = 4;

    #region Cast ray from camera
    public static bool MousePointerOnGroundAndCharacters(out RaycastHit hit) { 
        var ray = Camera.main.ScreenPointToRay(PCInputManager.Instance.hoverPosition());
        int groundLayerMask = 1 << 9;
        int enemyLayerMask = 1 << 11;
        int playerLayerMask = 1 << 8;

        int finalLayerMask = groundLayerMask | enemyLayerMask | playerLayerMask;
        
        return Physics.Raycast(ray, out hit, Mathf.Infinity, finalLayerMask);
    }

    public static PlayerController MousePointerOnCharacter(out RaycastHit hit)
    {
        PlayerController pc = null;
        var ray = Camera.main.ScreenPointToRay(PCInputManager.Instance.hoverPosition());
        int enemyLayerMask = 1 << 11;
        int playerLayerMask = 1 << 8;

        int finalLayerMask = enemyLayerMask | playerLayerMask;

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, finalLayerMask))
        {
            pc = hit.collider.GetComponent<PlayerController>();
        }
        return pc;
    }

    public static bool MousePointerOverCover(out RaycastHit hit)
    {
        var ray = Camera.main.ScreenPointToRay(PCInputManager.Instance.hoverPosition());
        int coverLayerMask = 1 << 13;
        return Physics.Raycast(ray, out hit, Mathf.Infinity, coverLayerMask);
    }

    /// <summary>
    /// Cast ray from action cam
    /// </summary>
    public static bool IsActionCamBlocked(Vector3 origin, Vector3 target)
    {
        int layerMask1 = 1 << LayerMask.NameToLayer("wall");
        int layerMask2 = 1 << LayerMask.NameToLayer("roof");
        int layerMask = layerMask1 | layerMask2;
        Vector3 direction = (target - origin).normalized;
        float distance = (target - origin).magnitude;
        bool wallInBetween = Physics.Raycast(origin, direction, distance, layerMask);
        Debug.DrawRay(origin, direction * distance, Color.green, 5f);
        return wallInBetween;
    }


    #endregion

    #region Simulate throwing object in a parabolic arc.
    public delegate void IsThrowComplete();

    public static IEnumerator SimulateProjectile(Transform projectile,
                               Vector3 source,
                               Vector3 target,
                               float firingAngle,
                               float gravity,
                               IsThrowComplete ThrowCompleteCallBack
                               )
    {
        // Move projectile to the position of throwing object + add some offset if needed.
        projectile.position = source;

        // Calculate distance to target
        float target_Distance = Vector3.Distance(source, target);

        // Calculate the velocity needed to throw the object to the target at specified angle.
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

        // Extract the X  Y componenent of the velocity
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

        // Calculate flight time.
        float flightDuration = target_Distance / Vx;

        // Rotate projectile to face the target.
        projectile.rotation = Quaternion.LookRotation(target - projectile.position);

        float elapse_time = 0;

        while (elapse_time < flightDuration)
        {
            projectile.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);

            elapse_time += Time.deltaTime;

            yield return null;
        }
        if (elapse_time >= flightDuration)
        {
            //projectile.transform.position = source;
            ThrowCompleteCallBack();
        }
    }
    #endregion

    #region Draw the path of the arc that the parabolic projectile will take
    public static void DrawArcPath(Transform marker,
                          Vector3 source,
                          Vector3 target,
                          float firingAngle,
                          LineRenderer throwPath, float gravity)
    {
        //lineMarker.position = myTransform.position + new Vector3(0, 0.0f, 0);
        marker.position = source;

        // Calculate distance to target
        float target_Distance = Vector3.Distance(source, target);
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

        //Calculate velocity on the x-y axis
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

        // Calculate flight time.
        float flightDuration = target_Distance / Vx;

        marker.rotation = Quaternion.LookRotation(target - source);

        //We need a total of 60 points for the line renderer.
        float timeStep = flightDuration / 60f;

        float elapsedTime = 0;
        throwPath.positionCount = 61;
        throwPath.SetPosition(0, marker.position);
        int i = 1;
        elapsedTime += timeStep;
        //Please note that this while loop will be invoked every frame.
        while (elapsedTime < flightDuration)
        {
            marker.Translate(0, (Vy - (gravity * elapsedTime)) * timeStep, Vx * timeStep);
            throwPath.SetPosition(i, marker.position);
            ++i;
            float timeRemaining = flightDuration - elapsedTime;
            if (timeRemaining < timeStep)
            {
                elapsedTime += timeRemaining;
            }
            else
            {
                elapsedTime += timeStep;
            }
        }
        throwPath.SetPosition(60, target);
        marker.position = source;
    }
    #endregion

    #region navigation related utilities
    public static Vector3 SnapToGrid(Vector3 point)
    {
        return new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z));
    }

    public static bool InsideNavMesh(Vector3 targetPosition, NavMeshAgent agent)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    #endregion
    #region cover detection utilities
    public static bool CheckCoverBetweenPointsByName(Vector3 origin, Vector3 termination, string coverName)
    {
        string cover = CheckCoverBetweenTwoPoints(origin, termination);
        if(cover != null)
        {
            return cover.Equals(coverName);
        } else
        {
            return false;
        }
    }

    public static CoverFramework GetHighCoverBetweenPoints(Vector3 origin, Vector3 target)
    {
        CoverFramework retValue = null;
        int coverLayerMask = 1 << 13;
        Vector3 direction = (target - origin).normalized;
        float distance = (target - origin).magnitude;

        List<CoverFramework> covers = new List<CoverFramework>();

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, coverLayerMask);

        if(hits != null && hits.Length > 0)
        {
            foreach(RaycastHit hit in hits)
            {
                covers.Add(hit.transform.GetComponent<CoverFramework>());
            }
            CoverCompare cca = new CoverCompare(target);
            covers.Sort(cca.Compare);
            foreach(CoverFramework cover in covers)
            {
                if(cover.coverType.Equals(CoverFramework.TYPE.full))
                {
                    return cover;
                }
            }
        }
        return retValue;
    }

    private static string CheckCoverBetweenTwoPoints(Vector3 origin, Vector3 termination)
    {
        string retValue = null;
        int coverLayerMask = 1 << 13;
        Vector3 direction = (termination - origin).normalized;
        float distance = (termination - origin).magnitude;
        RaycastHit hit;
        // Does the ray intersect any cover
        if (Physics.Raycast(origin, direction, out hit, distance, coverLayerMask))
        {
            retValue = hit.transform.name;
        }
        return retValue;
    }

    public static string[] CheckCoversBetweenTwoPoints(Vector3 origin, Vector3 termination)
    {
        string[] retValues = null;
        int coverLayerMask = 1 << 13;
        Vector3 direction = (termination - origin).normalized;
        float distance = (termination - origin).magnitude;
        RaycastHit[] hits;
        // Does the ray intersect any cover
        hits = Physics.RaycastAll(origin, direction, distance, coverLayerMask);
        Debug.DrawRay(origin, direction, Color.green);
        if (hits != null)
        {
            retValues = new string[hits.Length];

            for (int i = 0; i < hits.Length; i++)
            {
                retValues[i] = hits[i].transform.name;
            }
        }
        return retValues;
    }

    public static CoverFramework[] CheckCoversBetweenPoints(Vector3 origin, Vector3 termination)
    {
        CoverFramework[] retValues = null;
        int coverLayerMask = 1 << 13;
        Vector3 direction = (termination - origin).normalized;
        float distance = (termination - origin).magnitude;
        RaycastHit[] hits;
        // Does the ray intersect any cover
        hits = Physics.RaycastAll(origin, direction, distance, coverLayerMask);
        if (hits != null)
        {
            retValues = new CoverFramework[hits.Length];

            for (int i = 0; i < hits.Length; i++)
            {
                retValues[i] = hits[i].transform.GetComponent<CoverFramework>();
            }
        }
        return retValues;
    }

    public static bool CheckCoverBetweenTwoPoints(Vector3 origin, Vector3 termination, bool highOnly)
    {
        bool cover = false; ;
        int coverLayerMask = 1 << 13;
        Vector3 direction = (termination - origin).normalized;
        float distance = (termination - origin).magnitude;
        RaycastHit hit;
        // Does the ray intersect any cover
        if (Physics.Raycast(origin, direction, out hit, distance, coverLayerMask))
        {
            if(highOnly)
            {
                CoverFramework cf = hit.transform.GetComponent<CoverFramework>();
                if(cf.coverType.Equals(CoverFramework.TYPE.full))
                {
                    cover = true;
                }
            } else
            {
                cover = true;
            }
        }
        return cover;
    }

    public static CoverMeta IsCoverInTheMiddle(Command command, Vector3 dirForCover)
    {
        int layerMask = 1 << 13;
        Vector3 unitDir = dirForCover.normalized;
        CoverMeta cm = new CoverMeta();
        cm.coverInWay = false;
        /*
         * We run a raycast all to detect multiple covers that could be in the way 
        */
        RaycastHit[] hits = Physics.RaycastAll(command.playerController.transform.position, unitDir, Mathf.Infinity, layerMask);

        if (hits != null && hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                if (Mathf.Round(dirForCover.magnitude)
                    >= Mathf.Round(Vector3.Distance(command.playerController.transform.position, hit.point)))
                {
                    bool coverInTheWay = EvaluateCoverType(command, unitDir, hit);
                    if (coverInTheWay)
                    {
                        cm.coverInWay = true;
                        //cm.coverPosition = hit.point + (Vector3.up * 1.8f);
                        cm.coverPosition = hit.point;
                        cm.cover = hit.transform.gameObject.GetComponent<CoverFramework>();
                        cm.coverType = cm.cover.coverType;
                        if(cm.coverType == CoverFramework.TYPE.full)
                        {
                            break;
                        }
                    }
                }
            }
        }

        return cm;
    }

    public static bool IsThereCoverBetween(Vector3 origin, Vector3 target, string coverName)
    {
        bool isInCover = false;
        CoverFramework[] covers = GeneralUtils.CheckCoversBetweenPoints(origin, target);
        for (int i = 0; i < covers.Length; i++)
        {
            if (covers[i].name.Equals(coverName) || CoverFramework.TYPE.full.Equals(covers[i].coverType))
            {
                isInCover = true;
                break;
            }
        }
        return isInCover;
    }


    /// <summary>
    /// <para>
    ///     If character is in cover this logic evaluates if the cover detected is not the one which the player is already in.
    /// </para>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="wd"></param>
    /// <param name="unitDir"></param>
    /// <param name="hit"></param>
    private static bool EvaluateCoverType(Command command, Vector3 unitDir, RaycastHit hit)
    {
        bool coverInTheWay = false;
        CoverFramework cf = hit.transform.GetComponent<CoverFramework>();
        if (cf.coverType.Equals(CoverFramework.TYPE.full))
        {
            coverInTheWay = true;
        }
        else if (command.enemyController.InCover)
        {
            if (command.enemyController.cover.name.Equals(cf.name))
            {
                coverInTheWay = true;
            }
        }
        return coverInTheWay;
    }
    #endregion

    #region Resolving unique location and checking if spot occupied
    public static Vector3 GetUniqueLocation(PlayerController controller, Vector3 location)
    {
      Dictionary<int, Vector2> occupancyMap = GameManager.occupancyMap;
      float originalDistance = Vector3.Distance(controller.transform.position, location);
      Vector2 newLocation = new Vector2(Mathf.Floor(location.x), Mathf.Floor(location.z));
      //Debug.LogFormat("GetUniqueLocation::{0} getting unique location with {1}", controller.name, newLocation);
      //PrintDictionary(occupancyMap);
      int infiniteLoopCounter = 0;
      int maxLoops = 1000;
      float newDistance = originalDistance;
        bool logtest = newDistance < originalDistance;
        //Debug.LogFormat("Location is inside occupancy map {0}, it is inside nav mesh {1}, new distance is less than original distance {2}",
        //                 occupancyMap.ContainsValue(newLocation), InsideNavMesh(location, controller.nav), logtest);
      while (occupancyMap.ContainsValue(newLocation) || !InsideNavMesh(location, controller.nav)
            || newDistance < originalDistance)
      {
        
        ++infiniteLoopCounter;
        if (infiniteLoopCounter > maxLoops)
        {
            break;
        }
        Vector2 randPos = Random.insideUnitCircle * 2;
        randPos = new Vector2(randPos.x, randPos.y);
        location = SnapToGrid(new Vector3(location.x + randPos.x, location.y, location.z + randPos.y));
        newDistance = Vector3.Distance(controller.transform.position, location);
        newLocation = new Vector2(Mathf.Floor(location.x), Mathf.Floor(location.z));  
      }
      //Debug.LogFormat("GetUniqueLocation::{0} final move location is {1}", controller.name, newLocation);
      return location;
    }

    public static bool IsSpotOccupied(Vector3 position)
    {
        Vector2 location = new Vector2(Mathf.Floor(position.x), Mathf.Floor(position.z));
        return GameManager.occupancyMap.ContainsValue(location);
    }

    public static bool IsSpotOccupied(Vector3 position, int ID, out bool selfOccupied)
    {
        selfOccupied = false;
        //PrintDictionary(GameManager.occupancyMap);
        Vector2 location = new Vector2(Mathf.Floor(position.x), Mathf.Floor(position.z));

        bool isOccupied = GameManager.occupancyMap.ContainsValue(location);
        //Debug.LogFormat("Character ID is {0}, location is {1} isOccupied is {2}", ID, location, isOccupied);

        if (isOccupied)
        {
            //Debug.LogFormat("Checking for occupancy using {0}", ID);
            Vector2 locationForId = Vector2.zero;
            GameManager.occupancyMap.TryGetValue(ID, out locationForId);

            if(locationForId.Equals(location))
            {
                selfOccupied = true;
                return false;
            } else
            {
                return true;
            }
        } else
        {
            return false;
        }
    }

    private static void PrintDictionary(Dictionary<int, Vector2> occupancyMap)
    {
        foreach (KeyValuePair<int, Vector2> entry in occupancyMap)
        {
            Debug.LogFormat("{0}::{1}", entry.Key, entry.Value);
        }
    }
    #endregion

    #region General Utilities

    public static AudioClip GetRandomAudio(List<AudioClip> audios, Queue<AudioClip> lastPlayed, int noRepeatRange)
    {

        int randomSound = Random.Range(0, audios.Count);
        AudioClip sound = audios[randomSound];
        bool usedBefore = false;
        lock (lastPlayed)
        {
            usedBefore = lastPlayed.Contains(sound);
            while (usedBefore)
            {
                randomSound = Random.Range(0, audios.Count);
                sound = audios[randomSound];
                usedBefore = lastPlayed.Contains(sound);
            }
            lastPlayed.Enqueue(sound);
            if (!(lastPlayed.Count < noRepeatRange))
            {
                lastPlayed.Dequeue();
            }
        }
        return sound;
    }
    public static Command.InternalState GetTurnDirection(Transform transform, Vector3 target, out Vector3 targetDirection)
    {
        Vector3 fwd = transform.forward;
        targetDirection = (target - transform.position).normalized;
        Vector3 cross = Vector3.Cross(fwd, targetDirection);
        //Debug.LogFormat("Idle::cross.y {0}", cross.y);
        if (cross.y >= 0)
        {
            return Command.InternalState.turnRight;
        }
        else
        {
            return Command.InternalState.turnLeft;
        }
    }

    public static void SetAnimationTrigger(Animator anim, string trigger)
    {
        AnimatorStateInfo currentAnimState = anim.GetCurrentAnimatorStateInfo(0);
        if (!currentAnimState.IsName(trigger))
        {
            anim.SetTrigger(trigger);
        }
    }

    public static IEnumerator Fade(Material material)
    {
        for (float ft = 1f; ft >= 0; ft -= 0.1f)
        {
            //Debug.LogFormat("Fade::ft {0}", ft);
            Color c = material.color;
            c.a = ft;
            material.color = c;
            yield return null;
        }
    }

    public static void DrawCircle(Vector3 origin, LineRenderer line, float radius, float lineWidth)
    {
        var segments = 360;
        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = segments + 1;

        var pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
        var points = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            points[i] = origin + new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
        }

        line.SetPositions(points);
    }

    public static PlayerController FindClosest(PlayerController player, Team team, out float distance)
    {
        Vector3 origin = player.transform.position;
        float minDistance = float.MaxValue;
        PlayerController pc = null;

        foreach (PlayerController agent in team.players)
        {
            bool isApplicable = false;
            //first check if there is no cover in between
            CoverFramework[] covers = CheckCoversBetweenPoints(origin, agent.transform.position);
            isApplicable = (covers == null || covers.Length == 0);
            if(!isApplicable)
            {
                for (int i =0; i < covers.Length; i++)
                {
                    if(covers[i] != null && (player.cover != null
                        && covers[i].name.Equals(player.cover.name)))
                    {
                        isApplicable = false;
                        break;
                    } else if(covers[i] != null && covers[i].coverType.Equals(CoverFramework.TYPE.full)) {
                        isApplicable = false;
                        break;
                    }
                    isApplicable = true;
                }
            }

            if(isApplicable) //Only consider this enemy if there is no cover in between
            {
                distance = Vector3.Distance(origin, agent.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    pc = agent;
                }
            }
        }
        distance = minDistance;
        return pc;
    }
    #endregion

    #region damage related calculations

    public static CoverMeta CalculateDmgPercent(Command command, out float distanceCovered, out float damageReductionPct)
    {
        Vector3 targetPosition = command.EnemyTransform.position;
        Vector3 dirForCover = command.enemyController.transform.position - command.playerController.transform.position;
        CoverMeta cm = new CoverMeta();

        CoverMeta coverMeta = GeneralUtils.IsCoverInTheMiddle(command, dirForCover);
        if (coverMeta.coverInWay)
        {
            cm.coverInWay = true;
            cm.cover = coverMeta.cover;
            cm.coverPosition = coverMeta.coverPosition;
            cm.coverType = coverMeta.coverType;
        }
        else
        {
            cm.coverInWay = false;
        }

        distanceCovered = Mathf.Floor(Vector3.Distance(targetPosition, command.playerController.transform.position));
        damageReductionPct = 0;
        if (cm.coverInWay && cm.cover != null)
        {
            damageReductionPct = cm.cover.damageReductionPct;
        }
        return cm;
    }
    public static DamageMetaUI CalculateDamageChance(DamageParameters damageParameters,
                                        float distance,
                                        CoverFramework cf)
    {
        float optimal = damageParameters.optimalRange;
        float falloff = damageParameters.fallOff;
        float exponent = Mathf.Pow(Mathf.Max(0, distance - optimal) / falloff, 2);
        float chanceToHit = Mathf.Pow(0.5f, exponent);
        float coverReductionPct = cf != null?cf.damageReductionPct:0;
        chanceToHit -= coverReductionPct;
        chanceToHit = chanceToHit < 0 ? 0 : chanceToHit;
        chanceToHit *= 100;
        float damageMultiplier = CalculateDamageMultiplier(distance, optimal, falloff);

        return new DamageMetaUI(
                                chanceToHit,
                                damageParameters.criticalDamageChance*100,
                                damageMultiplier);
    }

    public static bool CalculateDamage(DamageParameters damageParameters,
                                        float distance,
                                        float coverReductionPct,
                                        out int damageAmt,
                                        out bool critical)
    {
        critical = false;
        damageAmt = 0;
        float optimal = damageParameters.optimalRange;
        float falloff = damageParameters.fallOff;

        float exponent = Mathf.Pow(Mathf.Max(0, distance - optimal) / falloff, 2);
        float chanceToHit = Mathf.Pow(0.5f, exponent);
        chanceToHit -= coverReductionPct;
        float rand = Random.value;
        if(rand < chanceToHit) //Hit the target
        {
            float damageMultiplier = CalculateDamageMultiplier(distance, optimal, falloff);

            damageAmt = Mathf.RoundToInt(damageParameters.baseDamage * damageMultiplier);
            float criticalDmg = Random.value;
            if (damageParameters.criticalDamageChance > criticalDmg)
            {
                damageAmt *= 3;
                critical = true;
            }

            return true;
        }

        return false;
    }

    private static float CalculateDamageMultiplier(float distance, float optimal, float falloff)
    {
        float damageMultiplier = 1;
        if (distance > optimal)
        {
            damageMultiplier = 1 - Mathf.Pow(distance / (optimal + falloff), 2);
        }
        damageMultiplier = damageMultiplier < 0 ? 0 : damageMultiplier;
        return damageMultiplier;
    }



    #endregion
}
