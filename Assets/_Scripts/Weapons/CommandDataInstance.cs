using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CommandDataInstance
{
    public ParticleSystem muzzleFlashInstance;
    public AudioSource audioSource;
    public GameObject r_weapon;
    public GameObject l_weapon;
    public Transform r_weaponHolder;
    public Transform l_weaponHolder;
    public Payload payload;
    public Transform rlaunchPoint;
    public GameObject MuzzleLight;
    public Command.type commandType;
    public int maxAmmo;
    public int ammoCount;
    public int maxBurstFire;
    public int maxCommands;
    public int commandsRun;

    public CommandDataInstance(AudioSource audioSource)
    {
        this.audioSource = audioSource;
    }

    public void UseAmmo()
    {
        --ammoCount;
    }

    public bool isAmmoLeft()
    {
        bool ammoLeft = ammoCount >= maxBurstFire;
        return ammoLeft;
    }

    public bool isAmmoFull()
    {
        return ammoCount == maxAmmo;
    }
    public void IncrementRunCount()
    {
        ++commandsRun;
    }
    public void InitializeRunCount()
    {
        commandsRun = 0;
    }
    public bool CanRun()
    {
        return maxCommands - commandsRun > 0;
    }

}
