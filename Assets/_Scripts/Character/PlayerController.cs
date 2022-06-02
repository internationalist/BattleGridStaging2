using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Cinemachine;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    #region state
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


    public string ID;
    public string teamID;


    public RangeMarker rangeMarker;

    public LineRenderer pathVisualizer;

    public LineRenderer rangeVisualizer;

    public GameObject destinationMarker;

    [SerializeField]
    private bool isDead = false;
    [SerializeField]
    private bool isSelected = false;

    public PlayerMetaData playerMetaData;

    public AIStateMachine _agent;

    public bool isAgent;

    [Tooltip("A boolean indicating if this character is in a cover position or not")]
    [SerializeField]
    private bool inCover;

    public CoverFramework cover;

    public List<ParticleSystem> bloodEffects;
    public ParticleSystem bloodMist;

    public List<ParticleSystem> deathBloods;

    public ParticleSystem deathHeadBlood;

    public InfoPanel infoPanel;

    public AudioClip spawnSound;

    private CommandQueue commandQueue = new CommandQueue();

    private Command currentCommand;
    
    private bool commandInQueue;

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

    public event StateAnimationCompleted OnAnimationComplete;

    public delegate void DamageSustained(float damageAmt);

    public event DamageSustained OnDamage;

    public delegate void CharacterDead(string id);

    public event CharacterDead OnDeath;

    public delegate void TurnOver(string id);

    public event TurnOver OnTurnEnd;

    public AudioSource audioSource;

    public AudioSource weaponAudio;

    public bool InCover { get => inCover; }

    /// <summary>
    /// Synchronizing on this property so that commands are not run on the object after it is set to be destroyed.
    /// </summary>
    public bool IsDead { get { lock (this) { return isDead; } } set { lock (this) { isDead = value; } } }

    [MethodImpl(MethodImplOptions.Synchronized)]
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
    [MethodImpl(MethodImplOptions.Synchronized)]
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
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void TakeDamage(int dmgAmt, bool critical = false)
    {
        if (!isDead)
        {
            playerMetaData.TakeDamage(dmgAmt);
            healthBar.fillAmount = playerMetaData.NormalizedHealthRemaining();

            if (playerMetaData.Hp <= 0)
            {
                setCurrentCommand(defaultCommand);
                currentCommand.TransitionToState(currentCommand.defaultState);
                AudioManager.PlayVoice(playerMetaData.grunts.screams, audioSource);
                StartCoroutine(deathBloodGush());
                IsDead = true;
            }
            else
            {
                float dmgPercent = playerMetaData.DamagePercent(dmgAmt);
                //Debug.Log("Damage percent " + dmgPercent);
                if (dmgPercent > .3)
                {
                    AudioManager.PlayVoice(playerMetaData.grunts.heavyHurt, audioSource);
                }
                else
                {
                    AudioManager.PlayVoice(playerMetaData.grunts.lightHurt, audioSource);
                }

                if (OnDamage != null)
                {
                    OnDamage(dmgAmt);
                }
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerator Heal(int healAmt, int duration, bool critical = false)
    {
        int deltaHealAmount = healAmt / duration;
        int totalHealed = 0;
        while(totalHealed < healAmt)
        {
            if (playerMetaData.Hp + deltaHealAmount > playerMetaData.maxHP)
            {
                playerMetaData.Hp = playerMetaData.maxHP;
            }
            else
            {
                playerMetaData.Hp += deltaHealAmount;
            }
            totalHealed += deltaHealAmount;
            UIManager.DamageNotification(deltaHealAmount, critical, transform.position + Vector3.up * 1.8f);
            healthBar.fillAmount = playerMetaData.NormalizedHealthRemaining();
            yield return new WaitForSeconds(1);
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

    [MethodImpl(MethodImplOptions.Synchronized)]
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
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void AnimationComplete(string id)
    {
        if (OnAnimationComplete != null)
        {
            OnAnimationComplete(id);
        }
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Die()
    {
        //isDead = true;
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

    private void Start()
    {
        OnDeath += TurnBasedSystem.I.CharacterDied;
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
        if(currentCommand.Equals(defaultCommand) && !commandInQueue)
        {//check command queue for content.
            CommandQueue.CommandQueueElement? queueElementOptional = commandQueue.Peek();
            if(queueElementOptional.HasValue)
            {
                CommandQueue.CommandQueueElement element = queueElementOptional.Value;
                StartCoroutine(DelayedCommandActivate(element));
            } else
            {
                defaultCommand.Update();
            }
        } else if (currentCommand != null)
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

        if(turnActive
            && !playerMetaData.CanRunCommand()
            && !isAgent) {//For AI the AI code will end turn
            EndTurn();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private IEnumerator DelayedCommandActivate(CommandQueue.CommandQueueElement element)
    {
        commandInQueue = true;
        yield return new WaitForSeconds(.5f);
        if(element.enemyTransform != null)
        {
            PlayerController enemy = element.enemyTransform.GetComponent<PlayerController>();
            if(enemy != null && !enemy.isDead && !isDead)
            {
                ActivateCommand(element.slot, element.enemyTransform, element.destination, element.onComplete);
                commandQueue.Dequeue();
            }
        }

        commandInQueue = false;
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
            if(playerMetaData.voice.responseOnSelect != null && playerMetaData.voice.responseOnSelect.Count > 0)
            {
                AudioManager.PlayVoice(playerMetaData.voice.responseOnSelect, audioSource);
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
        currentCommand.TransitionToState(currentCommand.defaultState);
    }

    public void ResetCommand()
    {
        currentCommand.Complete();
        setCurrentCommand(defaultCommand);
        currentCommand.TransitionToState(defaultCommand.defaultState);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void AddToCommandQueue(int slot,
                                    Transform enemyTransform,
                                    Vector3? destination,
                                    Command.OnCompleteCallback onComplete)
    {
        if((currentCommand.commandType == Command.type.move
            || currentCommand.commandType == Command.type.idle)
            && slot == GeneralUtils.MOVESLOT)//if current and requested commanmd is of move type. Immediately execute new move command.
        {
            ActivateCommand(slot, enemyTransform, destination, onComplete);
        } else if(slot == GeneralUtils.MOVESLOT)
        {
            CommandQueue.CommandQueueElement commandQueueElement  = new CommandQueue.CommandQueueElement(slot,
                                        enemyTransform,
                                        destination,
                                        onComplete);

            commandQueue.Replace(commandQueueElement);
        } else
        {
            CommandQueue.CommandQueueElement commandQueueElement = new CommandQueue.CommandQueueElement(slot,
                                        enemyTransform,
                                        destination,
                                        onComplete);

            if (!currentCommand.Equals(commands[slot]) && !commandQueue.Contains(commandQueueElement))
            {
                commandQueue.Enqueue(commandQueueElement);
            }
        }
    }

    private bool IsEnemyAlive(Transform enemyTransform)
    {
        if (enemyTransform != null)
        {
            PlayerController enemy = enemyTransform.GetComponent<PlayerController>();
            if (enemy != null && !enemy.isDead)
            {
                return true;
            }
        }
        return false;
    }


    public Command ActivateCommand(
                                    int slot,
                                    Transform enemyTransform,
                                    Vector3? destination,
                                    Command.OnCompleteCallback onComplete)
    {
        //Evaluate which command is selected and invoke command trigger.
        Command cmd = AssignCommand(slot);
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

    private Command AssignCommand(int slot)
    {

        if (getCurrentCommand() != null)
        {
            return SwapCommand(slot);
        }
        else
        {
            setCurrentCommand(commands[slot]);
            return getCurrentCommand();
        }
    }

    private Command SwapCommand(int slot)
    {
        Command oldCommand = getCurrentCommand();
        oldCommand.Cancel();
        setCurrentCommand(commands[slot]);
        return getCurrentCommand();
    }
    #endregion

    #region Turn related
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
        if(!turnActive || !playerMetaData.CanRunCommand())
        {
            UIManager.DisplayTurnOver(true);
        } else
        {
            UIManager.DisplayTurnOver(false);
        }
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
    #endregion
}
