using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public Transform startPoint;
    public Vector3 endPoint;
    LineRenderer beam;

    public void Start()
    {
        beam = GetComponent<LineRenderer>();
        Vector3 wspace = startPoint.TransformPoint(Vector3.zero);
        beam.SetPosition(0, wspace);
        beam.SetPosition(1, endPoint);
    }


}
