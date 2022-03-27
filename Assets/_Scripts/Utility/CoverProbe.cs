using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverProbe : MonoBehaviour
{
    PlayerController hostPlayer;

    private void Awake()
    {
        hostPlayer = GetComponentInParent<PlayerController>();
    }


    /// <summary>
    /// <para>We use object lock/synchronization on the OnTrigger Enter and Exit calls.</para>
    /// <para>This is required as the character can be present in a location where it's cover collider is triggering multiple cover objects at the same time.</para>
    /// <para>This will happen in a situation where two cover objects are placed close to each other on the map (Equal or less than 2 metres)</para>
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //Debug.LogFormat("{0} Collided with other-1: {1}", name, other.name);
        lock (this)
        {
            CoverFramework cf = other.GetComponent<CoverFramework>();
            if (cf != null)
            {
                hostPlayer.EnableCover(true, cf);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.LogFormat("{0} Exited from other-1: {1}", name, other.name);
        lock (this)
        {
            //Debug.LogFormat("{0} Exited from other-2: {1}", name, other.name);
            if (hostPlayer.cover != null)
            {
                CoverFramework cf = other.GetComponent<CoverFramework>();
                if (hostPlayer.cover.name.Equals(cf.name))
                {
                    hostPlayer.EnableCover(false);
                }
            } else
            {
                hostPlayer.EnableCover(false);
            }
        }
    }


}
