using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ricochet", menuName = "Sounds/Ricochet", order = 1)]
public class Ricochet : ScriptableObject
{
    public List<AudioClip> leading;
    public List<AudioClip> trailing;
}
