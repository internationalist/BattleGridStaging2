using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageParameters 
{
    [Tooltip("The optimal range of the weapon in metres")]
    public float optimalRange;
    [Tooltip("The minimum range of the weapon in metres. E.g grenades")]
    public float minimumRange;
    [Tooltip("The falloff Range of the weapon in metres")]
    public float fallOff;
    [Tooltip("The weapon damage.")]
    public int baseDamage;
    private float maxAttackRange;
    public int criticalDamageMultiplier;
    [Range(0, 1)]
    public float criticalDamageChance;
}
