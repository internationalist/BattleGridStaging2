using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollider : MonoBehaviour
{
    public PlayerController pc;
    AudioSource ausrc;

    private void Start()
    {
        ausrc = pc.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Command command = pc.getCurrentCommand();
        if(command is MovementFSM && command.isActivated)
        {
            AudioManager.PlayFootSteps(ausrc);
        }
    }
}
