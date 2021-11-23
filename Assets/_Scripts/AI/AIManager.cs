using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    #region singleton code
    private static AIManager _instance;
    public static AIManager I
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (I != null)
        {
            Debug.LogError("Trying to initialize AIManager more than once");
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("cover");
        foreach(GameObject cover in coverObjects)
        {
            covers.Add(cover.GetComponent<CoverFramework>());
        }
    }


    #endregion

    #region state
    //public List<PlayerController> foes;
    Stack<PlayerController> friends;
    PlayerController activeAgent;
    public Team currentTeam;

    public List<CoverFramework> covers;


    #endregion

    private void Update()
    {
        if (IsTeamTurnActive())
        {
            if (activeAgent == null || !activeAgent.turnActive)
            {
                if(friends.Count > 0)
                {
                    activeAgent = friends.Pop();
                    GameManager.SelectPlayer(activeAgent);
                    activeAgent.StartTurn();
                } else //All agents exhausted, end turn
                {
                    currentTeam.isTurnActive = false;
                }
            }
        } else
        {
            //Turn over, reset agent.
            activeAgent = null;
        }
    }

    private bool IsTeamTurnActive()
    {
        return currentTeam != null && currentTeam.isTurnActive;
    }

    public void StartTurn()
    {
        friends = new Stack<PlayerController>(currentTeam.players);
        currentTeam.isTurnActive = true;
    }
    #region commented code
    /*    public static void PrintValues(IEnumerable myCollection, char mySeparator)
        {
            foreach (Object obj in myCollection)
                Debug.LogFormat("{0}{1}", mySeparator, obj);
        }*/
    #endregion
    public static void Init(Team team)
    {
        I.currentTeam = team;
    }
}
