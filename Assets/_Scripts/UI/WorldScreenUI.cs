using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldScreenUI : MonoBehaviour
{
    public GameObject trackedObject;
    public float offsetX, offsetY;
    Camera cam;
    RectTransform rectTransform;
    public TMP_Text content;
    private bool visible;
    // Start is called before the first frame update
    void Awake()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        content = GetComponentInChildren<TMP_Text>();
    }

    // Update is called once per frame
/*    void Update()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(trackedObject.transform.position);
        rectTransform.position = new Vector2(screenPos.x + offsetX, screenPos.y + offsetY);
    }*/

    public void Show()
    {
        if(!visible)
        {
            visible = true;
            this.gameObject.SetActive(visible);
        }
        
    }

    public void Hide()
    {
        if(visible)
        {
            visible = false;
            this.gameObject.SetActive(visible);
        }
        
    }

    public void SetContent(string s)
    {
        content.text = s;
    }
}
