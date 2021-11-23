using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScoreMap
{

    public PlayerController entity;
    public float score;

    public EntityScoreMap(PlayerController entity, float score)
    {
        this.entity = entity;
        this.score = score;
    }
}
