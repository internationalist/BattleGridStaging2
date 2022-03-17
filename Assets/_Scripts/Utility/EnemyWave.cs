using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyWave
{
    public PlayerController[] enemyPrefabs;

    public Vector3 spawnZoneCenter;

    public Vector3 unitLookAt;

}
