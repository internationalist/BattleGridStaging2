using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Team
{
    public List<PlayerController> players;
    public bool isTurnActive;
    public bool aiAgent;
    public string name;
    public string teamID;
    public Dictionary<int, PlayerController> memberMap = new Dictionary<int, PlayerController>();

    public void init()
    {
        if(players != null && players.Count > 0)
        {
            foreach(PlayerController player in players)
            {
                memberMap.Add(player.ID, player);
            }
        }
    }
}
