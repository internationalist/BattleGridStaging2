using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload : MonoBehaviour
{
    public bool fire = false;

    [Tooltip("All damage related parameters")]
    public DamageParameters damageParameters;

    public Vector3 targetPosition;
    public PlayerController target;

    public ParticleSystem impactEffect;

    public Vector3 origin;


    [Tooltip("Game objects in this layer will be damaged by the bullet")]
    public int enemyLayer;

    public int coverLayer;


    /// <summary>
    /// Identity of the object that is using this payload so we don't generate any trigger events with it.
    /// </summary>
    public string ownerObject;

    public bool coverInWay;
    public Vector3 coverPosition;
    public CoverFramework.TYPE coverType;
    public CoverFramework cover;
    public bool tracerActive = false;




}
