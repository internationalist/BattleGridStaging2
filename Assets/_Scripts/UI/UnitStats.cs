using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStats : MonoBehaviour
{
    public RectTransform statsPanel;
    public Transform hostUnit;
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        statsPanel = GetComponent<RectTransform>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(hostUnit.position + Vector3.up*1.2f);
        statsPanel.position = new Vector3(screenPos.x, screenPos.y, 0);
    }
}
