using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public abstract class CommandTemplate : ScriptableObject
{
    #region scriptable object assets
    [Tooltip("The effect on firing the weapon.")]
    public ParticleSystem[] effectFlashPrefab;
    [Tooltip("The audio effect on firing the weapon.")]
    public AudioClip[] soundEffects;
    [Tooltip("The bullet or projectile object.")]
    public Payload payloadPrefab;
    [Tooltip("The effect on the weapon impact.")]
    public ParticleSystem impactFlashPrefab;
    [Tooltip("The speed at which the payload travels.")]
    public float payLoadSpeed;
    [Tooltip("The prefab of the right hand weapon")]
    public GameObject RweaponPrefab;
    [Tooltip("The prefab of the left hand weapon")]
    public GameObject LweaponPrefab;
    public AudioSource weaponAudioSource;
    public Vector3 R_WeaponHolder_Position;
    public Vector3 R_WeaponHolder_Rotation;
    [Tooltip("The action points needed")]
    public float apNeeded;
    [Tooltip("The layer belonging to enemy")]
    public int enemyLayer;
    [Tooltip("How many shots will be fired each time")]
    public int maxBurstFire;
    public int maxAmmo;
    [Tooltip("Is this the idle or default command")]
    public bool isDefault;
    [Tooltip("Max number of commands in one turn")]
    public int maxRuns;

    [Tooltip("A timeout value on the command. Usually set it to the time of the animation clip.")]
    public float timeOutInSecs;

    protected bool effectComplete =false;

    public DamageParameters damageParameters;

    #endregion


    public abstract void Launch(Command command, CommandDataInstance wd);

    public abstract UIMetaData getUIMetadata();

    public abstract Command getCommand(Animator anim, NavMeshAgent nav, PlayerController controller);

    public void Setup(Transform rWeaponHolder, Transform lWeaponHolder, CommandDataInstance weaponData)
    {
        OrientRWeaponHolder(rWeaponHolder);

        if (RweaponPrefab != null && rWeaponHolder != null && weaponData.r_weapon == null)
        {
            //Orient the weaponHolder
            weaponData.r_weapon = GameObject.Instantiate(RweaponPrefab, Vector3.zero, Quaternion.identity);
            weaponData.rlaunchPoint = weaponData.r_weapon.gameObject.transform.Find("LaunchPoint");
            weaponData.r_weapon.transform.parent = rWeaponHolder.transform;
            weaponData.r_weapon.transform.localPosition = Vector3.zero;
            weaponData.r_weapon.transform.localRotation = Quaternion.identity;
        }
        else
        {
            if(weaponData.r_weapon != null)
            {
                weaponData.r_weapon.SetActive(true);
            }
        }

        if (LweaponPrefab != null && lWeaponHolder != null && weaponData.l_weapon == null)
        {
            weaponData.l_weapon = GameObject.Instantiate(LweaponPrefab, lWeaponHolder.position, Quaternion.identity);
            weaponData.l_weapon.transform.parent = lWeaponHolder.transform;
            weaponData.l_weapon.transform.localPosition = Vector3.zero;
            weaponData.l_weapon.transform.localRotation = Quaternion.identity;
        } else
        {
            if(weaponData.l_weapon != null)
            {
                weaponData.l_weapon.SetActive(true);
            }
        }

        void OrientRWeaponHolder(Transform rWeaponHolder)
        {
            rWeaponHolder.transform.localPosition = R_WeaponHolder_Position;
            rWeaponHolder.transform.localRotation = Quaternion.Euler(R_WeaponHolder_Rotation);
        }
    }

    public void TearDown(CommandDataInstance commandData)
    {
        if (commandData.r_weapon != null)
        {
            commandData.r_weapon.SetActive(false);
        }

        if (commandData.l_weapon != null)
        {
            commandData.l_weapon.SetActive(false);
        }
    }

    protected IEnumerator RunNoiseEffect(CommandDataInstance wd)
    {
        float totalDuration = 0;
        for (int k = 0; k < soundEffects.Length; k++)
        {
            totalDuration += soundEffects[k].length;
            GameManager.I.StartCoroutine(AudioManager.TransitionToActionAndBack(totalDuration));
        }
        for (int k = 0; k < soundEffects.Length; k++)
        {
            float delay = soundEffects[k].length;
            wd.audioSource.PlayOneShot(soundEffects[k]);
            if(k == soundEffects.Length - 1) //this is the last element
            {
                effectComplete = true;
            }
            yield return new WaitForSeconds(delay);
        }
    }
}
