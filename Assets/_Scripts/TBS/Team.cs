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
    public Dictionary<string, PlayerController> memberMap = new Dictionary<string, PlayerController>();

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

    public void Remove(PlayerController player)
    {
        players.Remove(player);
        memberMap.Remove(player.ID);
    }

    public bool isWiped()
    {
        return players.Count == 0;
    }

    public void AddPlayer(PlayerController pc)
    {
        players.Add(pc);
        memberMap.Add(pc.ID, pc);
    }
}
