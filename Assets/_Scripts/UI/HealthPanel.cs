using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPanel : MonoBehaviour
{
    public Camera mainCamera;
    public Canvas healthPanel;

    // Update is called once per frame
    void Update()
    {
        healthPanel.transform.LookAt(mainCamera.transform);
    }
    
}
