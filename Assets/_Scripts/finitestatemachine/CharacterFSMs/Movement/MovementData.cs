using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementData", menuName = "Actions/MovementData", order = 1)]
public class MovementData : ScriptableObject
{
    public GameObject markerPrefab;
    public float range;
    public Color effectColor;
}
