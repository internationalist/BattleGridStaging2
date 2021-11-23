using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DamageMetaUI
{

    public float dmgChance;
    public float criticalDmgChance;
    public float dmgMultiplier;

    public DamageMetaUI(float dmgChance, float criticalDmgChance, float dmgMultiplier)
    {
        this.dmgChance = dmgChance;
        this.criticalDmgChance = criticalDmgChance;
        this.dmgMultiplier = dmgMultiplier;
    }
}
