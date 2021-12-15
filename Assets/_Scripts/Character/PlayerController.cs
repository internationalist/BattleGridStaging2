using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Cinemachine;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    public Dictionary<int, Command> commands;

    [Tooltip("Transform within the rig that acts as the parent of the right hand weapon.")]
    public Transform R_WeaponHolder;

    [Tooltip("Transform within the rig that acts as the parent of the left hand weapon.")]
    public Transform L_WeaponHolder;

    [Header("A list of all the weapon templates that map onto all the states that this character supports.")]
    public List<CommandContainer> commandList;

    public bool turnActive;

    [Tooltip("The default state. It is always idle")]
    [SerializeField]
    private Command defaultCommand;


    public int ID;
    public string teamID;


    public RangeMarker rangeMarker;

    public LineRenderer pathVisualizer;

    public LineRenderer rangeVisualizer;

    public GameObject destinationMarker;

    [SerializeField]
    public bool isDead = false;
    [SerializeField]
    private bool isSelected = false;

    public PlayerMetaData playerMetaData;

    public AIStateMachine _agent;

    public bool isAgent;

    [Tooltip("A boolean indicating if this character is in a cover position or not")]
    private bool inCover;

    public CoverFramework cover;

    public List<ParticleSystem> bloodEffects;
    public ParticleSystem bloodMist;

    public List<ParticleSystem> deathBloods;

    public ParticleSystem deathHeadBlood;

    public InfoPanel infoPanel;



    #region internal state
    private Command currentCommand;

    public Command getCurrentCommand() { return currentCommand;}

    public void setCurrentCommand(Command value)
    {
        //Debug.LogFormat("{0}::setCurrentCommand Starting", name);
        currentCommand = value;
        currentCommand.Setup(R_WeaponHolder, L_WeaponHolder);
        CommandDataInstance wd = currentCommand.commandDataInstance;
        if (wd != null)
        {
            barrelGrip = wd.r_weapon.gameObject.transform.Find("BarrelGrip");
        }
    }

    public string currentState;
    private Animator anim;
    public NavMeshAgent nav;
    GameObject marker;
    public Image healthBar;
    public Image coverStatus;

    public bool barrelGripIK = true;


    private NavMeshPath path;
    private Transform barrelGrip;

    public Transform actionCamHook;

    #endregion

    #region Events
    public delegate void StateAnimationCompleted(string id);

    public static event StateAnimationCompleted OnAnimationComplete;

    public delegate void DamageSustained(float damageAmt);

    public event DamageSustained OnDamage;

    public delegate void CharacterDead(int id);

    public event CharacterDead OnDeath;

    public delegate void TurnOver(int id);

    public event TurnOver OnTurnEnd;

    public AudioSource audioSource;

    public AudioSource weaponAudio;

    public bool InCover { get => inCover; }

    public void EnableCover(bool inCover, CoverFramework cf = null)
    {
        this.cover = cf;
        this.inCover = inCover;
        if(cf != null)
        {
            this.coverStatus.transform.parent.gameObject.SetActive(true);
            this.coverStatus.fillAmount = cf.damageReductionPct;
        } else
        {
            this.coverStatus.transform.parent.gameObject.SetActive(false);
            this.coverStatus.fillAmount = 0;
        }
        
    }

    /// <summary>
    /// Event that is invoked for weapon effects launch from a certain point in the
    /// weapon activation animation. This event only happens during an attack move.
    /// </summary>
    public void WeaponsLaunch()
    {
        try
        {
            currentCommand.ActivateWeapon();
        }
        catch (System.NotImplementedException nex)
        {
            Debug.LogError(name + "::Activate weapon not implemented for command " + currentCommand);
            Debug.LogError(nex.StackTrace);
        }
    }

    /// <summary>
    /// This public method is invoked by other objects to simulate this player taking damage.
    /// </summary>
    /// <param name="dmgAmt"></param>
    public void TakeDamage(int dmgAmt, bool critical = false)
    {
        
        playerMetaData.TakeDamage(dmgAmt);
        healthBar.fillAmount = playerMetaData.NormalizedHealthRemaining();
        UIManager.DamageNotification(dmgAmt, critical, transform.position + Vector3.up * 1.8f);

        if (playerMetaData.Hp <= 0)
        {
            AudioManager.PlayHurtSound(playerMetaData.grunts.screams, audioSource);
            StartCoroutine(deathBloodGush());
            isDead = true;
            if (OnDeath != null)
            {
                OnDeath(ID);
            }
        }
        else
        {
            float dmgPercent = playerMetaData.DamagePercent(dmgAmt);
            //Debug.Log("Damage percent " + dmgPercent);
            if(dmgPercent > .3)
            {
                AudioManager.PlayHurtSound(playerMetaData.grunts.heavyHurt, audioSource);
            } else
            {
                AudioManager.PlayHurtSound(playerMetaData.grunts.lightHurt, audioSource);
            }
            
            if (OnDamage != null)
            {
                OnDamage(dmgAmt);
            }
        }
    }

    private IEnumerator deathBloodGush()
    {
        yield return new WaitForSeconds(.05f);
        deathHeadBlood.Play();
        ParticleSystem deathBlood = deathBloods[Random.Range(0, 2)];
        deathBlood.Play();
        yield return new WaitForSeconds(2f);
        deathHeadBlood.Stop();
        deathBlood.Stop();
    }

    public void TopUpAmmo()
    {
        CommandDataInstance wi = commands[GeneralUtils.ATTACKSLOT].commandDataInstance;
        wi.ammoCount = wi.maxAmmo;
    }

    /// <summary>
    /// For certain state animations there are no clear condition within code to identify their completion.
    /// For such cases we define an animation completion event within the animation. Individual animation states
    /// that need this event will subscribe to this.
    /// </summary>
    /// <param name="id"></param>
    public void AnimationComplete(string id)
    {
        if (OnAnimationComplete != null)
        {
            OnAnimationComplete(id);
        }
    }

    public void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }
    #endregion

    #region Unity events

    void Awake()
    {
        path = new NavMeshPath();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        marker = transform.Find("Marker").gameObject;
        commands = new Dictionary<int, Command>();
        _agent = GetComponent<AIStateMachine>();

        if (_agent != null)
        {
            isAgent = true;
        }

        for (int k = 0; k < commandList.Count; ++k)
        {
            commands.Add(commandList[k].slot, commandList[k].weaponTemplate.getCommand(anim, nav, this));
            if(commandList[k].weaponTemplate.isDefault)
            {
                setCurrentCommand(commands[commandList[k].slot]);
                defaultCommand = commands[commandList[k].slot];
            }
        }
        
        playerMetaData.Initialize();
    }

    private void OnEnable()
    {
        GameManager.OnSelected += OnSelect;
    }

    private void OnDisable()
    {
        GameManager.OnSelected -= OnSelect;
    }

    private void Update()
    {
        currentState = currentCommand.currentState.GetType().Name;
        if (currentCommand != null && (isSelected || currentCommand.isActivated))
        {
            currentCommand.Update();
        }
        else
        {
            defaultCommand.Update();
        }

        if (isSelected) //TODO for better performance, put this inside a coroutine
        {
            UpdatePlayerHUD();
        }

        if(turnActive && !playerMetaData.CanRunCommand()) {
            EndTurn();
        }
    }
    #endregion

    #region Commands

    public CommandTemplate GetWeaponTemplateForCommand(int slot)
    {
        foreach (CommandContainer cmdContainer in commandList)
        {
            if (slot == cmdContainer.slot)
            {
                return cmdContainer.weaponTemplate;
            }
        }
        return null;
    }

    public void OnSelect(PlayerController player)
    {
        if (player != null && player.ID == this.ID)
        {
            if(playerMetaData.responseOnSelect != null && playerMetaData.responseOnSelect.Count > 0)
            {
                AudioManager.PlayHurtSound(playerMetaData.responseOnSelect, audioSource);
                //int audioIndex = Random.Range(0, playerMetaData.responseOnSelect.Count);
                //audioSource.PlayOneShot(playerMetaData.responseOnSelect[audioIndex]);
            }
            marker.SetActive(true);
            isSelected = true;
            UpdatePlayerHUD();
        }
        else
        {
            marker.SetActive(false);
            isSelected = false;
        }
    }

    public void CancelCommand()
    {
        currentCommand.Cancel();
        setCurrentCommand(defaultCommand);
        currentCommand.TransitionToState(currentCommand.currentState);
    }

    public void EndTurn()
    {
        turnActive = false;
        if (OnTurnEnd != null)
        {
            OnTurnEnd(ID);
        }
    }

    public void StartTurn()
    {
        playerMetaData.GetReadyForTurn();
        for(int i = 0; i < commands.Count; ++i)
        {
            commands[i].commandDataInstance.InitializeRunCount();
        }
        turnActive = true;
    }

    #endregion

    #region UI Related
    public void ResetCommand()
    {
        currentCommand.Complete();
        setCurrentCommand(defaultCommand);
        currentCommand.TransitionToState(currentCommand.currentState);
    }

    public void CreatePath()
    {
        NavMesh.CalculatePath(gameObject.transform.position,
        currentCommand.Destination.Value, NavMesh.AllAreas, path);
        pathVisualizer.gameObject.SetActive(true);
        GameManager.VisualizeRoute(path, pathVisualizer, gameObject.transform.position);
    }

    public void CreatePath(Vector3 endPoint)
    {
        NavMesh.CalculatePath(gameObject.transform.position,
        endPoint, NavMesh.AllAreas, path);
        pathVisualizer.gameObject.SetActive(true);
        GameManager.VisualizeRoute(path, pathVisualizer, gameObject.transform.position);
    }

    public void CreateRangeIndicator(float range)
    {
        rangeVisualizer.gameObject.SetActive(true);
        GeneralUtils.DrawCircle(transform.position, rangeVisualizer, range, .1f);
    }

    public void CreateRangeIndicatorForMove()
    {
        float range = this.playerMetaData.movementRange;
        rangeVisualizer.gameObject.SetActive(true);
        GeneralUtils.DrawCircle(transform.position, rangeVisualizer, range, .1f);
    }

    public void ResetPath()
    {
        pathVisualizer.positionCount = 0;
        pathVisualizer.gameObject.SetActive(false);
    }

    public void ResetRangeIndicator()
    {
        rangeVisualizer.positionCount = 0;
        rangeVisualizer.gameObject.SetActive(false);
    }

    public void UpdatePlayerHUD()
    {
        CommandDataInstance wi = commands[GeneralUtils.ATTACKSLOT].commandDataInstance;
        UIManager.DisplayAmmo(wi.ammoCount);
        UIManager.DisplayTurnsLeftForItemUse(playerMetaData.turnsLeftForItemUse);
    }

    public void ShowInfoPanel()
    {
        infoPanel.Show();
        infoPanel.SetInfo("5", this.name);
    }

    public void HideInfoPanel()
    {
        infoPanel.Hide();
    }



    public bool IsAmmoFull()
    {
        CommandDataInstance wi = currentCommand.commandDataInstance;
        return wi.isAmmoFull();
    }
    #endregion























}
