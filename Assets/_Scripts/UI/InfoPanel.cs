using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class InfoPanel
{
    // Start is called before the first frame update
    public GameObject panel;
    public TMP_Text level;
    public TMP_Text username;
    public TMP_Text levelBg;
    public TMP_Text usernameBg;

    public void SetInfo(string level, string userName)
    {
        this.level.text = level;
        this.levelBg.text = level;
        this.username.text = userName;
        this.usernameBg.text = userName;
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
