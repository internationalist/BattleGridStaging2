using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public Transform startPoint;
    public Vector3 endPoint;
    LineRenderer beam;
    float textureOffset;

    public void Start()
    {
        beam = GetComponent<LineRenderer>();
        Vector3 wspace = startPoint.TransformPoint(Vector3.zero);
        beam.SetPosition(0, wspace);
        beam.SetPosition(1, endPoint);
    }

    private void Update()
    {
        textureOffset -= Time.deltaTime * 5f;
        if(textureOffset < -10f)
        {
            textureOffset = 0;
        }
        beam.sharedMaterials[1].SetTextureOffset("_MainTex", new Vector2(textureOffset, 0f));
    }




}
