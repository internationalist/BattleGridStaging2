using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TurnBasedSystem:MonoBehaviour
{
    #region singleton
    private static TurnBasedSystem _instance;
    public static TurnBasedSystem I
    {
        get { return _instance; }
    }

    public Team ActiveTeam { get => activeTeam; set => activeTeam = value; }

    void Awake()
    {
        //Debug.Log("TurnBased system awaking");
        if (I != null)
        {
            Debug.LogError("Trying to initialize UIManager more than once");
            return;
        }
        _instance = this;
    }
    #endregion

    public List<Team> teams;
    private List<Team> teamTurnQueue;
    
    private Team activeTeam;

    //public Queue<Team> turnQueue;

    [SerializeField]
    private bool turnChanging = false;

    /// <summary>
    /// Coroutine that starts a new turn.
    /// </summary>
    /// <returns></returns>
    public IEnumerator NextTurn()
    {
        turnChanging = true;
        if (activeTeam != null)
        {
            //Add the team whose turn ended back into the turn queue.
            teamTurnQueue.Add(activeTeam); //Enqueue
        }
        //Pop the next team in line from the queue.
        //activeTeam = turnQueue.Dequeue();
        activeTeam = teamTurnQueue[0];
        teamTurnQueue.Remove(activeTeam); //POP

        //Weird bug that results in empty object being dequeued.
        //if (activeTeam.players.Count == 0)
        //{
        //    activeTeam = turnQueue.Dequeue();
        //}
        //if team is non-human controlled
        if (activeTeam.aiAgent)
        {
            AudioManager.BeginEnemyTurn();
            UIManager.ActivateBeginTurnMessageAI();
        } else
        {
            AudioManager.BeginPlayerTurn();
            UIManager.ActivateBeginTurnMessage();
        }

        yield return new WaitForSeconds(3);
        //Reset each player in team and set their turn to active.


        //If non-human controlled then start the AI turn.
        if (activeTeam.aiAgent)
        {
            AIManager.I.StartTurn();
        } else //only for human players
        {
            activeTeam.isTurnActive = true;
            foreach (PlayerController player in activeTeam.players)
            {
                player.StartTurn();
            }
        }

        turnChanging = false;
    }

    public void CharacterDied(string ID)
    {
        RemoveFromTeam(ID);
    }

    private void Start()
    {
        teamTurnQueue = new List<Team>();
        Team t = new Team();
        StartCoroutine(GameManager.I.SpawnNextWave(t));
        teams.Insert(0, t);

        for (int i = 0; i < teams.Count; i++)
        {
            teams[i].init();
            teamTurnQueue.Add(teams[i]);
            /* if team controlled by non-human agent then register them
             * with AI Manager. At this point system only works for one non-human enemy team.
             * First implementation is with AI. Network agent will be added later.
            */
            if (teams[i].aiAgent) 
            {
                AIManager.Init(teams[i]);
            }
            //Subscribe to the death event of each player.
            foreach (PlayerController pc in teams[i].players)
            {
                pc.OnDeath += CharacterDied;
                if(!teams[i].aiAgent) //Listen for turn end event only for human player
                {
                    pc.OnTurnEnd += OnTurnEnd;
                }
            }
        }
        //Start first turn
        StartCoroutine(NextTurn());
    }

    public void Update()
    {
        if(!turnChanging)
        {
            if (!activeTeam.isTurnActive && !GameManager.I.levelComplete) //turn is over.
            {
                StartCoroutine(NextTurn());
            }
        }
    }

    /// <summary>
    /// Turn end event only for human controlled team
    /// </summary>
    /// <param name="id"></param>
    public void OnTurnEnd(string id)
    {
        bool teamTurnActive = false;
        foreach (PlayerController pc in activeTeam.players)
        {
            if(pc.turnActive)
            {
                teamTurnActive = true;
                break;
            }
        }
        activeTeam.isTurnActive = teamTurnActive;
    }

    private void RemoveFromTeam(string ID)
    {
        Team team = null;
        PlayerController pc = null;
        foreach(Team t in teams)
        {
            try
            {
                pc = t.memberMap[ID];
                team = t;
                break;
            } catch(System.Exception exc)
            {
            }
        }
        if(pc != null)
        {
            team.Remove(pc);
            if(team.isWiped())
            {
                teams.Remove(team);
                teamTurnQueue.Remove(team);
                if(team.aiAgent)
                {
                    if(GameManager.I.waves != null && GameManager.I.waves.Count > 0)
                    {
                        InitalizeNewWave();
                    }
                    else
                    {
                        //End the game here
                        HandleGameOver(team);
                    }

                } else
                {
                    //End the game here
                    HandleGameOver(team);
                }
            }
            if (!pc.isAgent) //dead character is player controlled
            {
                foreach (PlayerController aiChar in AIManager.I.currentTeam.players)
                {
                    aiChar._agent.InitState();
                    aiChar._agent.TransitionToState(aiChar._agent.defaultState);
                }
            }
        }
    }

    private void InitalizeNewWave()
    {
        Team t = new Team();
        StartCoroutine(GameManager.I.SpawnNextWave(t));
        t.init();
        teams.Add(t);
        teamTurnQueue.Add(t);
        AIManager.Init(t);
        foreach (PlayerController pc in t.players)
        {
            pc.OnDeath += CharacterDied;
        }
    }

    private static void HandleGameOver(Team team)
    {
        GameManager.I.levelComplete = true;
        if (team.aiAgent)
        {
            UIManager.ActivateStageClearMessage();
        }
        else
        {
            UIManager.ActivategameOverMessage();
        }
    }
}
