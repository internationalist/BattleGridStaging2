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

    #region state
    public List<Team> teams;
    private List<Team> teamTurnQueue;
    
    private Team activeTeam;

    [SerializeField]
    private bool turnChanging = false;

    bool readyForNextTurn=true;
    #endregion

    #region events and delegates
    public delegate bool ReadyForNextTurn();

    bool ReadyForNextTurnImpl()
    {
        return readyForNextTurn;
    }
    #endregion


    /// <summary>
    /// Coroutine that starts a new turn.
    /// </summary>
    /// <returns></returns>
    public IEnumerator NextTurn()
    {
        turnChanging = true;
        yield return new WaitUntil(()=> { return readyForNextTurn; });

        if (activeTeam != null)
        {
            //Add the team whose turn ended back into the turn queue.
            teamTurnQueue.Add(activeTeam); //Enqueue
        }
        //Pop the next team in line from the queue.
        //activeTeam = turnQueue.Dequeue();
        activeTeam = teamTurnQueue[0];
        teamTurnQueue.Remove(activeTeam); //POP

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
        StartCoroutine(SpawnNextWave(t));
        teams.Insert(0, t); //Add team to 0th position of team list.

        for (int i = 0; i < teams.Count; i++)
        {
            if(!teams[i].aiAgent) { //Only for human player
                teams[i].init();
                teamTurnQueue.Add(teams[i]);
                //Subscribe to the death event of each player.
                foreach (PlayerController pc in teams[i].players)
                {
                    if (!teams[i].aiAgent) //Listen for turn end event only for human player
                    {
                        pc.OnDeath += CharacterDied;
                        pc.OnTurnEnd += OnTurnEnd;
                    }
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
                    if(waves != null && waves.Count > 0)
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
        StartCoroutine(SpawnNextWave(t));
        teams.Add(t); //Add team to team list.
        /*        foreach (PlayerController pc in t.players)
                {
                    pc.OnDeath += CharacterDied;
                }*/
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

    #region Enemy wave spawn
    public List<EnemyWave> waves;
    int waveCounter = 0;

    public LineRenderer pathVisualizer;
    public LineRenderer rangeVisualizer;
    public IEnumerator SpawnNextWave(Team t)
    {
        if (waves != null && waves.Count > 0)
        {
            readyForNextTurn = false; //Stop the turns from running till wave is initialized
            ++waveCounter;
            t.aiAgent = true;
            t.name = "AIWave";
            t.teamID = string.Format("{0}", waveCounter);
            t.players = new List<PlayerController>();
            EnemyWave wave = waves[0];
            waves.Remove(wave);
            Vector2 pt;
            for (int i = 0; i < wave.enemyPrefabs.Length; i++)
            {
                pt = UnityEngine.Random.insideUnitCircle * 5f;
                Vector3 spawnLocation = wave.spawnZoneCenter + new Vector3(pt.x, wave.spawnZoneCenter.y, pt.y);
                while (!GeneralUtils.InsideNavMesh(spawnLocation, GameManager.UNIVERSAL_AGENT)
                        || GeneralUtils.AreInSameSpot(GameManager.occupancyMap, spawnLocation, 3f))
                {
                    pt = UnityEngine.Random.insideUnitCircle * 5f;
                    spawnLocation = wave.spawnZoneCenter + new Vector3(pt.x, wave.spawnZoneCenter.y, pt.y);
                }

                PlayerController enemy = Instantiate(wave.enemyPrefabs[i],
                                         spawnLocation,
                                         Quaternion.LookRotation(wave.unitLookAt - spawnLocation));

                enemy.pathVisualizer = pathVisualizer;
                enemy.rangeVisualizer = rangeVisualizer;
                enemy.ID = string.Format("EW{0}{1}", waveCounter, i);
                enemy.OnDeath += CharacterDied;
                AudioManager.I.PlayEnemySpawn(enemy.audioSource);
                GameManager.occupancyMap[enemy.ID] = new Vector2(Mathf.Floor(spawnLocation.x), Mathf.Floor(spawnLocation.z));
                t.AddPlayer(enemy);
                yield return new WaitForSeconds(.5f);
            }
            /* Register them with AI Manager.
             * At this point system only works for one non-human enemy team.
             * First implementation is with AI. Network agent will be added later.
            */
            AIManager.Init(t);
            t.init();
            teamTurnQueue.Add(t); //Add team to command queue.
            readyForNextTurn = true; //Turns can proceed.
        }
    }
    #endregion
}
