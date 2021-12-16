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

    public GameObject hitChancePanel;
    public TMP_Text criticalDmg;
    public TMP_Text criticalDmgBg;
    public TMP_Text hitChance;
    public TMP_Text hitChanceBg;
    public TMP_Text dmgMultiplier;
    public TMP_Text dmgMultiplierBg;

    public GameObject dialogPanel;
    public TMP_Text dialog;

    public void SetInfo(string level, string userName)
    {
        this.level.text = string.Format("Level:{0}",level);
        this.levelBg.text = this.level.text;
        this.username.text = userName;
        this.usernameBg.text = userName;
    }

    public void SetAttackInfo(string criticalDmg,
                        string hitChance,
                        string dmgMultiplier)
    {
        this.criticalDmg.text = string.Format("Critical Damage:{0}%", criticalDmg);
        this.criticalDmgBg.text = this.criticalDmg.text;
        this.hitChance.text = string.Format("Hit Chance:{0}%", hitChance);
        this.hitChance.text = this.hitChance.text;
        this.dmgMultiplier.text = string.Format("Dmg Multiplier:{0}", dmgMultiplier);
        this.dmgMultiplierBg.text = this.dmgMultiplier.text;
    }

    public void SetDialog(string dialog)
    {
        this.dialog.text = dialog;
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void ShowHitChance()
    {
        hitChancePanel.SetActive(true);
    }

    public void HideHitChance()
    {
        hitChancePanel.SetActive(false);
    }

    public IEnumerator ShowDialog()
    {
        this.dialogPanel.SetActive(true);
        yield return new WaitForSeconds(1f);
        this.dialogPanel.SetActive(false);
    }
}
