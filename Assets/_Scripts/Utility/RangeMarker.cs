using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeMarker : MonoBehaviour
{
    public float range;
    public bool active;
    public Quaternion lookRotation;
    public GameObject markerPrefab;
    public Color inRangecolor;
    public Color outOfRangecolor;
    public bool inRange = true;
    private List<GameObject> markerList = new List<GameObject>();

    private void Update()
    {
        if(active)
        {
            int markerNum = Mathf.FloorToInt(range);
            for(int i = 0; i < markerNum; i++)
            {
                GameObject marker = Instantiate(markerPrefab, (transform.position + transform.forward * i), Quaternion.Euler(90,0,0));
                var rend = marker.GetComponent<Renderer>();
                if (inRange)
                {
                    rend.material.SetColor("_BaseColor", inRangecolor);
                    rend.material.SetColor("_EmissionColor", inRangecolor);
                }
                else 
                {
                    rend.material.SetColor("_BaseColor", outOfRangecolor);
                    rend.material.SetColor("_EmissionColor", outOfRangecolor);
                }
                marker.transform.rotation = lookRotation;
                marker.transform.parent = transform;
                markerList.Add(marker) ;
            }
        } else
        {
            foreach(GameObject marker in markerList)
            {
                Destroy(marker);
            }
            markerList.Clear();
        }
    }
}
