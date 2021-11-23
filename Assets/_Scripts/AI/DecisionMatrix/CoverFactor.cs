using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverFactor : Factor
{
    public CoverFactor(string dmName, float weight) : base(dmName, weight)
    {
    }

    public override int CalculateScore(PlayerController agent, PlayerController target)
    {
        int retValue = 10;
        string targetCoverName = null;
        CoverFramework targetCover = target.cover;
        if (target.inCover)
        {
            targetCoverName = targetCover.name;
        }
        CoverFramework[] cfs = GeneralUtils.CheckCoversBetweenPoints(agent.transform.position, target.transform.position);
        if(cfs != null && cfs.Length > 0)
        {
            for(int i = 0; i < cfs.Length; i++)
            {
                if(CoverFramework.TYPE.full.Equals(cfs[i].coverType)) {//target behind full cover
                    return -10;
                } else if(cfs[i].name.Equals(targetCoverName)) //target behind half cover
                {
                    retValue = 0;
                }
            }
            return retValue; //no cover
        } else
        {
            return retValue;
        }
    }
}
