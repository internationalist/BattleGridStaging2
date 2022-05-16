using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    #region singleton code + Unity events
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
        UIManager.I.OnActionCamChange += AIPause;
    }

    private void OnDestroy()
    {
        UIManager.I.OnActionCamChange -= AIPause;
    }

    private void Update()
    {
        if (IsTeamTurnActive()) //Agent turns ending leads to team turn end.
        {
            if (!aiPause && (activeAgent == null || !activeAgent.turnActive))
            {
                if (friends.Count > 0)
                {
                    activeAgent = friends.Pop();
                    GameManager.SelectPlayer(activeAgent);
                    activeAgent.StartTurn();
                }
                else //All agents exhausted, end turn
                {
                    currentTeam.isTurnActive = false;
                }
            }
        }
        else
        {
            //Turn over, reset agent.
            activeAgent = null;
        }
    }


    #endregion

    #region state
    //public List<PlayerController> foes;
    Stack<PlayerController> friends;
    PlayerController activeAgent;
    public Team currentTeam;
    public List<CoverFramework> covers;
    public bool aiPause;


    #endregion

    #region Team related code
    private bool IsTeamTurnActive()
    {
        return currentTeam != null && currentTeam.isTurnActive;
    }

    public void StartTurn()
    {
        friends = new Stack<PlayerController>(currentTeam.players);
        currentTeam.isTurnActive = true;
    }

    public static void Init(Team team)
    {
        I.currentTeam = team;
    }

    private void AIPause(bool pause)
    {
        //AI turn pauses when action cam is on and is active when action cam is off.
        this.aiPause = !pause;
    }
    #endregion
}
