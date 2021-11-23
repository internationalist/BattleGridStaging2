using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.OnSelected += OnSelect;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    Vector3 moveToSelected;
    public void OnSelect(PlayerController player)
    {
        moveToSelected = player.transform.position;

        moveToSelected.y = transform.position.y;

        float amt = (moveToSelected - transform.position).magnitude;
        //moveToSelected = transform.InverseTransformDirection(moveToSelected - transform.position);
        moveToSelected = (moveToSelected - transform.position).normalized;
        Debug.Log("player position " + player.transform.position);
        //transform.LookAt(player.transform);
        //m_TargetCameraState.Translate(moveToSelected*amt*.75f);
        transform.position = player.transform.position;
        Debug.Log("camera position " + transform.position);
    }
}
