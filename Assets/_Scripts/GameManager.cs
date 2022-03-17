using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    #region singleton code
    private static GameManager _instance;
    public static GameManager I
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (I != null)
        {
            Debug.LogError("Trying to initialize GameManager more than once");
            return;
        }
        _instance = this;
        universalAgent = GetComponent<NavMeshAgent>();
        Cursor.SetCursor(cursorGroup.select, Vector3.zero, CursorMode.Auto);
        regularCoverMaterial = new Dictionary<MeshRenderer, Material>();
    }
    #endregion

    #region state
    public static PlayerController _currentPlayer;
    public static bool playerSelected;

    public delegate void PlayerSelectAction(PlayerController playerController);
    public static event PlayerSelectAction OnSelected;

    public static NavMeshAgent universalAgent;

    public GameObject grenadeMarker;

    public float grenadeThrowAngle;

    public float gravity;

    public static Dictionary<string, Vector2> occupancyMap = new Dictionary<string, Vector2>();

    private static GameObject transparentCover;
    public Material transparentCoverMaterial;
    private Dictionary<MeshRenderer, Material> regularCoverMaterial;
    public CursorGroup cursorGroup;
    [Range(0,1)]
    public float actionCamChance;
    //public bool readOnly;

    public bool levelComplete;

    #endregion

    #region select and activate command
    public static void SelectPlayer(PlayerController playerController)
    {
        
        if(playerController != null)
        {
//            Debug.LogFormat("current player is {0}", _currentPlayer);
            if(_currentPlayer == null || _currentPlayer.ID != playerController.ID) //only play selection sound if a new character is selected.
            {
                AudioManager.ShowSelctionUI();
                _currentPlayer = playerController;
                playerSelected = true;
                if (!playerController.isAgent)
                {
                    UIManager.Show();
                }
                UIManager.I.characterId.text = "Soldier:" + _currentPlayer.ID;
                OnSelected(_currentPlayer);
            }
        } else
        {
            _currentPlayer = null;
            playerSelected = false;
            UIManager.Hide();
        }
    }

    public static Command ActivateCommand(Transform enemyTransform, Vector3? destination)
    {
        //Debug.LogFormat("GameManager::{0}->Command is {1} and target point is {2}", _currentPlayer.name, _currentPlayer.CurrentCommand, destination);
        //Evaluate which command is selected and invoke command trigger.
        Command cmd = GameManager._currentPlayer.getCurrentCommand();
        return InvokeCmd(enemyTransform, destination, cmd);
    }

    public static Command ActivateCommand(Transform enemyTransform, Vector3? destination, Command.OnCompleteCallback onComplete)
    {
        //Evaluate which command is selected and invoke command trigger.
        Command cmd = GameManager._currentPlayer.getCurrentCommand();
        cmd.onCompleteCallback = onComplete;
        return InvokeCmd(enemyTransform, destination, cmd);
    }

    private static Command InvokeCmd(Transform enemyTransform, Vector3? destination, Command cmd)
    {
        if (cmd != null)
        {
            cmd.StartCommand(enemyTransform, destination);
        }
        return cmd;
    }
    #endregion

    private void Update()
    {
        FadeCover();
    }

    #region cover fading
    private void FadeCover()
    {
        RaycastHit hit;
        if (GeneralUtils.MousePointerOverCover(out hit))
        {
            GameObject coverObject = hit.collider.gameObject;
            if (transparentCover != null)
            {
                if (!transparentCover.name.Equals(coverObject.name))
                {
                    MakePreviousCoverVisible(); //Make previous faded cover visible
                    ActivateFade(coverObject);
                }
            }
            else
            {
                ActivateFade(coverObject);
            }
        }
        else
        {
            if (transparentCover != null)
            {
                MakePreviousCoverVisible(); //Make previous faded cover visible
                ResetAlphaFadeMaterial();//Reset alpha of fade material for next fade.
            }
        }
    }

    private void MakePreviousCoverVisible()
    {
        MeshRenderer[] mr = GetRenderer(transparentCover);
        for(int i = 0; i < mr.Length; i++)
        {
            mr[i].material = regularCoverMaterial[mr[i]];
        }
        regularCoverMaterial.Clear();
    }

    private void ResetAlphaFadeMaterial()
    {
        Color c = transparentCoverMaterial.color;
        c.a = 1;
        transparentCoverMaterial.color = c;
        transparentCover = null;
    }

    private void ActivateFade(GameObject coverObject)
    {
        MeshRenderer[] mr = GetRenderer(coverObject);
        for(int i = 0; i < mr.Length; i++)
        {
            regularCoverMaterial.Add(mr[i], mr[i].material); //cache the material of this renderer.
            mr[i].material = transparentCoverMaterial;
        }
        //regularCoverMaterial = mr.material; 
        
        StartCoroutine(GeneralUtils.Fade(transparentCoverMaterial));
        transparentCover = coverObject;
    }


    private static MeshRenderer[] GetRenderer(GameObject coverObject)
    {
        //MeshRenderer mr = coverObject.GetComponentInChildren<MeshRenderer>();
        //if (mr == null)
        //{
            MeshRenderer[] mr = coverObject.GetComponent<CoverFramework>().coverRenderer;
        //}

        return mr;
    }
    #endregion

    #region UI Handlers
    public static void AssignCommandFromUI(int slot)
    {
        if(_currentPlayer.turnActive)
        {
            AudioManager.ClickButton();
            Command command = AssignCommand(slot);
            if(command.invokeImmediate)
            {
                ActivateCommand(null, Vector3.zero);
            }
        }
    }

    public static void EndPlayerTurn()
    {
        AudioManager.ClickButton();
        _currentPlayer.EndTurn();
    }

    public static Command AssignCommand(int slot)
    {
        if (_currentPlayer != null)
        {
            if (_currentPlayer.getCurrentCommand() != null)
            {
                return SwapCommand(slot);
            }
            else
            {
                _currentPlayer.setCurrentCommand(_currentPlayer.commands[slot]);
                return _currentPlayer.commands[slot];
            }
        }
        return null;
    }

    private static Command SwapCommand(int slot)
    {
        if(_currentPlayer != null)
        {
            Command oldCommand = _currentPlayer.getCurrentCommand();
            oldCommand.Cancel();
            _currentPlayer.setCurrentCommand(_currentPlayer.commands[slot]);
            return _currentPlayer.commands[slot];
        }
        return null;
    }
    #endregion

    #region weapon and movement visualizations
    public static void VisualizeRoute(NavMeshPath path, LineRenderer pathVisualizer, Vector3 origin)
    {
        pathVisualizer.positionCount = path.corners.Length; //set the array of positions to the amount of corners

        if (pathVisualizer.positionCount > 0)
        {
            pathVisualizer.SetPosition(0, origin);
        }

        var length = path.corners.Length;
        if(length > 1)
        {
            for (var i = 1; i < length - 1; i++)
            {
                pathVisualizer.SetPosition(i, path.corners[i]); //go through each corner and set that to the line renderer's position
            }
            Vector3 lastSegmentFrag = (path.corners[length - 1] - path.corners[length - 2]).normalized;
            Vector3 lastSegment = path.corners[length - 1] - lastSegmentFrag * .5f;
            pathVisualizer.SetPosition(length - 1, lastSegment);
        }
    }

    public static void CreateGrenadePath(Vector3 endPoint)
    {
        if (_currentPlayer != null)
        {
            _currentPlayer.pathVisualizer.gameObject.SetActive(true);
            GeneralUtils.DrawArcPath(
                marker: I.grenadeMarker.transform,
                source: GameManager._currentPlayer.gameObject.transform.position,
                target: endPoint,
                firingAngle: I.grenadeThrowAngle,
                throwPath: _currentPlayer.pathVisualizer,
                gravity: _instance.gravity);
        }
    }

    public static void ThrowProjectile (Transform projectile, Vector3 startPoint, Vector3 endPoint, GeneralUtils.IsThrowComplete throwComplete)
    {
        I.StartCoroutine(GeneralUtils.SimulateProjectile(
            projectile: projectile,
            source: startPoint,
            target: endPoint,
            firingAngle: I.grenadeThrowAngle,
            gravity: _instance.gravity,
            throwComplete));
    }

    #endregion

    #region Spawn Management

    public List<EnemyWave> waves;
    int waveCounter = 0;

    public LineRenderer pathVisualizer;
    public LineRenderer rangeVisualizer;
    public void SpawnNextWave(Team t)
    {
        if(waves != null && waves.Count > 0)
        {
            ++waveCounter;
            t.aiAgent = true;
            t.name = "AIWave";
            t.teamID = String.Format("{0}", waveCounter);
            t.players = new List<PlayerController>();
            EnemyWave wave = waves[0];
            Vector2 pt;
            for (int i = 0; i < wave.enemyPrefabs.Length; i++) 
            {
                pt = UnityEngine.Random.insideUnitCircle * 5f;
                Vector3 spawnLocation = wave.spawnZoneCenter + new Vector3(pt.x, wave.spawnZoneCenter.y, pt.y);
                while(!GeneralUtils.InsideNavMesh(spawnLocation, universalAgent)
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
                enemy.ID = String.Format("EW{0}{1}", waveCounter, i);
                GameManager.occupancyMap[enemy.ID] = new Vector2(Mathf.Floor(spawnLocation.x), Mathf.Floor(spawnLocation.z));
                t.AddPlayer(enemy);
            }
        }
    }
    





    #endregion
}
