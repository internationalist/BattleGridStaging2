using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedMove : MonoBehaviour
{
    public Vector3[] wayPoints;
    public bool activate;
    bool isRunning;
    Vector3 wayPoint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(activate && !isRunning)
        {
            wayPoint = wayPoints[0];
            isRunning = true;
        }


    }
}
