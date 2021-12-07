using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStats : MonoBehaviour
{
    public RectTransform statsPanel;
    public Transform hostUnit;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        statsPanel = GetComponent<RectTransform>();
        //cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 screenPos = cam.WorldToScreenPoint(hostUnit.position);
        statsPanel.position = screenPos;
    }
}
