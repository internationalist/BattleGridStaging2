using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Command : BaseFSMController
{
    public Animator anim;
    public NavMeshAgent nav;
    public Transform playerTransform;


    private Vector3 destination;
    private Transform enemyTransform;
    public PlayerController enemyController;
    public bool isActivated;
    public Vector3 targetDirection;
    public ScriptableObject actionData;
    public type commandType;

    protected GameObject destinationMarker;

    public delegate void OnCompleteCallback();

    public OnCompleteCallback onCompleteCallback;



    public CommandTemplate commandTemplate;

    public CommandDataInstance commandDataInstance;

    //This will become true when command completes.
    public bool complete;
    public bool cancel;

    public bool invokeImmediate;

    protected float timeOutInSecs;
    protected float commandStartTimeInSecs = -1;

    public PlayerController playerController;

    /// <summary>
    /// This represents all the types of FSMs. Each command is represented by an individual FSM. Since each Command can contain more than one distinct action.
    /// E.g. Turn direction, aim, shoot.
    /// </summary>
    public enum type {
        idle,
        move,
        primaryaction,
        specialaction,
        reload,
        buff
    };

    public Vector3? Destination {
        get { return destination; }
        set {
            Vector3 dest = value.Value;
            dest.y = playerTransform.position.y;
            destination = dest;
        }
    }

    public Transform EnemyTransform { 
        get => enemyTransform;
        set { 
            enemyTransform = value;
            enemyController = enemyTransform.GetComponent<PlayerController>();
        }
    }

    public Command(Animator anim, NavMeshAgent nav, PlayerController controller, CommandTemplate weaponAction)
    {
        this.anim = anim;
        this.nav = nav;
        this.playerTransform = controller.gameObject.transform;
        this.commandTemplate = weaponAction;
        this.playerController = controller;
        this.commandDataInstance = new CommandDataInstance(playerController.weaponAudio);
        this.commandDataInstance.maxAmmo = weaponAction.maxAmmo;
        this.commandDataInstance.ammoCount = weaponAction.maxAmmo;
        this.commandDataInstance.maxBurstFire = weaponAction.maxBurstFire;
        this.commandDataInstance.maxCommands = weaponAction.maxRuns;
        this.timeOutInSecs = weaponAction.timeOutInSecs;
    }

    public Command(Animator anim, NavMeshAgent nav, PlayerController controller) : this(anim, nav, controller, null)
    {
    }

    public Command()
    {

    }

    /// <summary>
    /// This list of states are the ones that are internal to the FSM. Each value in this enum represents different state classes that make up the FSMs.
    /// The list of states here is a global list of all states that are used by all different FSM child objects of Command.
    /// </summary>
    public enum InternalState { idle, turnLeft, turnRight, approach, attack, throwItem, lightdamage, heavydamage, death, useItem };




    #region Command Lifecycle.

    public virtual void Setup(Transform rWeaponHolder, Transform lWeaponHolder)
    {
        if(commandTemplate != null)
        {
            commandTemplate.Setup(rWeaponHolder, lWeaponHolder, commandDataInstance);
        }
        //At setup of the command it is not active.
        isActivated = false;
        //At setup the command is not complete.
        complete = false;
        //initialize the cancel state as well
        cancel = false;
    }

    protected abstract void BeforeActivate();

    protected virtual void Activate(Transform enemyTransform, Vector3? destination)
    {
        commandStartTimeInSecs = Time.realtimeSinceStartup;
        //GameManager.I.readOnly = true;
        currentState.EnterState(this);
    }

    public void StartCommand(Transform enemyTransform, Vector3? destination)
    {
        if(Validate(enemyTransform, destination))
        {
            Activate(enemyTransform, destination);
        } else //If validate is false still invoke the complete call back to let other threads know
        {
            if (onCompleteCallback != null)
            {
                onCompleteCallback();
            }
        }
    }

    protected abstract bool Validate(Transform enemyTransform, Vector3? destination);


    /// <summary>
    /// Implement this in child class to set isActivated = true on specific condition.
    /// </summary>
    protected abstract void Activated();

    public virtual void Complete()
    {
        commandStartTimeInSecs = -1f;
        //GameManager.I.readOnly = false;
        commandDataInstance.IncrementRunCount();
        if (commandTemplate != null)
        {
            commandTemplate.TearDown(this.commandDataInstance);
        }
    }

    public virtual void Cancel()
    {
        if (commandTemplate != null)
        {
            commandTemplate.TearDown(this.commandDataInstance);
        }
    }
    #endregion


    public override void Update()
    {
        if(timeOutInSecs > 0 && commandStartTimeInSecs > -1f)
        {
            float timeSinceCommandStart = Time.realtimeSinceStartup - commandStartTimeInSecs;
            if (timeSinceCommandStart > timeOutInSecs)
            {
                Debug.LogFormat("{0} Timeout exceeded with {1} seconds. Terminating command", timeOutInSecs, timeSinceCommandStart);
                complete = true;
            }
        }


        if(!complete && !cancel)
        {
            base.Update();
            if (!isActivated)
            {
                BeforeActivate();
            }
            else
            {
                Activated();
            }
        } else if(complete)
        {
            playerController.ResetCommand();
            if(onCompleteCallback != null)
            {
                onCompleteCallback();
            }
        } else //Cancel when the user right-clicks to cancel command
        {
            playerController.CancelCommand();
            if (onCompleteCallback != null)
            {
                onCompleteCallback();
            }
        }
    }
    public override string ToString()
    {
        return commandType.ToString();
    }
    public abstract void ActivateWeapon();

}
