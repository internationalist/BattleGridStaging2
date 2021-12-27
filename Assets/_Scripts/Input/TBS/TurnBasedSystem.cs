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
    public Team activeTeam;

    public Queue<Team> turnQueue;

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
            turnQueue.Enqueue(activeTeam);
        }
        //Pop the next team in line from the queue.
        activeTeam = turnQueue.Dequeue();
        //Weird bug that results in empty object being dequeued.
        if(activeTeam.players.Count == 0)
        {
            activeTeam = turnQueue.Dequeue();
        }
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

    public void CharacterDied(int ID)
    {
        //Debug.LogFormat("Character {0} just died", ID);
        RemoveFromTeam(ID);
    }

    private void Start()
    {
        turnQueue = new Queue<Team>();

        for(int i = 0; i < teams.Count; i++)
        {
            teams[i].init();
            turnQueue.Enqueue(teams[i]);
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
            if (!activeTeam.isTurnActive) //turn is over.
            {
                StartCoroutine(NextTurn());
            }
        }
    }

    /// <summary>
    /// Turn end event only for human controlled team
    /// </summary>
    /// <param name="id"></param>
    public void OnTurnEnd(int id)
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

    private void RemoveFromTeam(int ID)
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
                //Debug.Log(exc.StackTrace);
            }
        }
        if(pc != null)
        {
            team.players.Remove(pc);
            team.memberMap.Remove(ID);
            if (!pc.isAgent) //dead character is player controlled
            {
                foreach (PlayerController aiChar in AIManager.I.currentTeam.players)
                {
                    aiChar._agent.InitState();
                }
            }
        }
    }



}
