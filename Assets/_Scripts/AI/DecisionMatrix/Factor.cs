using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Factor
{
    public float weight;
    public string dmName;

    public Factor(string dmName, float weight)
    {
        this.dmName = dmName;
        this.weight = weight;
    }

    public virtual int CalculateScore(PlayerController agent, PlayerController target)
    {
        return -1;
    }
}
