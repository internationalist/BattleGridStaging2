using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    private TMP_Text speech;
    public float lifeTime = 5;
    private void Awake()
    {
        speech = GetComponentInChildren<TMP_Text>();
        GameObject.Destroy(this.gameObject, lifeTime);
    }
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }

    public void SetText(string txt)
    {
        speech.text = txt;
    }

}
