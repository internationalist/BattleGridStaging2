using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeFactor : Factor
{
    public RangeFactor(string dmName, float weight) : base(dmName, weight)
    {
    }

    public override int CalculateScore(PlayerController agent, PlayerController target)
    {
        float distance = Vector3.Distance(agent.transform.position, target.transform.position);
        DamageParameters dmgParams = agent.GetWeaponTemplateForCommand(GeneralUtils.ATTACKSLOT).damageParameters;
        float optimalRange = dmgParams.optimalRange + dmgParams.fallOff;
        float calculated = optimalRange - distance;
        if(calculated > 0)
        {
            calculated = (calculated / optimalRange) * 10;
        } else
        {
            calculated = 0;   
        }
        return Mathf.RoundToInt(calculated);
    }
}
