using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceModule", menuName = "Sounds/Voice", order = 1)]
public class VoiceModule : ScriptableObject
{
    public List<AudioClip> responseOnSelect;
    public List<AudioClip> responseOnMove;
    public List<AudioClip> responseOnAction;
    public List<AudioClip> responseOnUseItem;
    public List<AudioClip> responseOnReload;
}
