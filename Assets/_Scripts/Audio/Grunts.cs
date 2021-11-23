using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Grunts", menuName = "Sounds/Grunts", order = 1)]
public class Grunts : ScriptableObject
{
    public List<AudioClip> lightHurt;
    public List<AudioClip> heavyHurt;
    public List<AudioClip> screams;
}
