using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        UNIVERSAL_AGENT = GetComponent<NavMeshAgent>();
        //Cursor.SetCursor(cursorGroup.select, Vector3.zero, CursorMode.Auto);
        regularCoverMaterial = new Dictionary<MeshRenderer, Material>();
        GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("cover");
        for (int i = 0; i < coverObjects.Length; i++)
        {
            CoverFramework cf = coverObjects[i].GetComponent<CoverFramework>();
            cf.ID = i;
            covers.Add(cf);
        }
    }
    #endregion

    #region state
    public static PlayerController _currentPlayer;
    public static bool playerSelected;

    public delegate void PlayerSelectAction(PlayerController playerController);
    public static event PlayerSelectAction OnSelected;

    public static NavMeshAgent UNIVERSAL_AGENT;

    public GameObject grenadeMarker;

    public float grenadeThrowAngle;

    public float gravity;

    public static Dictionary<string, Vector2> occupancyMap = new Dictionary<string, Vector2>();

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool RecordOccupancyIfEmpty(string ID, Vector3 position)
    {
        if(GeneralUtils.IsSpotOccupied(position))
        {
            Debug.LogFormat("ID: {0} Position occupied {1}", ID, position);
            return false;
        } else
        {
            Debug.LogFormat("ID: {0} Adding position to occupancy map {1}", ID, position);
            occupancyMap[ID] = new Vector2(position.x, position.z);
            return true;
        }
    }

    private static GameObject transparentCover;
    public Material transparentCoverMaterial;
    private Dictionary<MeshRenderer, Material> regularCoverMaterial;
    public CursorGroup cursorGroup;

    public bool levelComplete;

    [Header("This is a very important control and is used to turn on or off realtime combat!")]
    public bool realTimeCombat;

    public List<CoverFramework> covers = new List<CoverFramework>();

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
        //Debug.LogFormat("Cover object getting renderer {0}", coverObject.transform.parent.name);
        MeshRenderer[] mr = coverObject.GetComponent<CoverFramework>().coverRenderer;
        //}
        return mr;
    }
    #endregion

    #region UI Handlers
   /* public static void AssignCommandFromUI(int slot)
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
    }*/

    /// <summary>
    /// Called by UI
    /// </summary>
    public static void EndPlayerTurn()
    {
        AudioManager.ClickButton();
        _currentPlayer.EndTurn();
    }

/*    public static Command AssignCommand(int slot)
    {
        return AssignCommand(_currentPlayer, slot);
    }

    public static Command AssignCommand(PlayerController player, int slot)
    {
        if (player != null)
        {
            if (player.getCurrentCommand() != null)
            {
                return SwapCommand(player, slot);
            }
            else
            {
                player.setCurrentCommand(player.commands[slot]);
                return player.commands[slot];
            }
        }
        return null;
    }

    private static Command SwapCommand(PlayerController player, int slot)
    {
        if(player != null)
        {
            Command oldCommand = player.getCurrentCommand();
            oldCommand.Cancel();
            player.setCurrentCommand(player.commands[slot]);
            return player.getCurrentCommand();
        }
        return null;
    }*/

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
}
