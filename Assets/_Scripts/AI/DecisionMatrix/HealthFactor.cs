using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthFactor : Factor
{
    public HealthFactor(string dmName, float weight) : base(dmName, weight)
    {
    }

    public override int CalculateScore(PlayerController agent, PlayerController target)
    {
        float health = target.playerMetaData.Hp;

        float maxHealth = target.playerMetaData.maxHP;
        float calculated = maxHealth - health;
        if (calculated > 0)
        {
            calculated = (calculated / maxHealth) * 10;
        }
        return Mathf.RoundToInt(calculated);
    }
}
