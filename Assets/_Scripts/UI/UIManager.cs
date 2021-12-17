using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Cinemachine;

public class UIManager : MonoBehaviour
{
    #region singleton
    private static UIManager _instance;
    public static UIManager I
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (I != null)
        {
            Debug.LogError("Trying to initialize UIManager more than once");
            return;
        }
        _instance = this;
    }
    #endregion

    #region state
    public GameObject mainPanel;

    public TMP_Text points;

    public TMP_Text pointsNeeded;

    public TMP_Text characterId;

    public GameObject messagePanel;

    public TMP_Text beginTurnMsg;
    public TMP_Text beginTurnMsgAI;

    public TMP_Text ammoCounter;

    public TMP_Text turnsLeftForItemUse;

    public UINotifications uiNotifications;


    public GameObject speecPrefab;

    public WorldScreenUI attackData;

    public Camera actionCam;
    public CinemachineVirtualCamera cmActionCam;
    public CinemachineVirtualCamera cmGameCam;

    public Image fadePanel;

    public static readonly int CM_MAX_PRIORITY = 100;
    public static readonly int CM_MIN_PRIORITY = 0;

    #endregion

    #region Static panel show and hide.
    public static void Show()
    {
        GenerateUIForCommands(GameManager._currentPlayer);
            List<CommandContainer> commandList = GameManager._currentPlayer.commandList;
            for (int i = 0; i < commandList.Count; i++)
            {
                CommandContainer commandContainer = commandList[i];
                if(commandContainer.button != null)
                {
                    CommandTemplate commandTemplate = commandContainer.weaponTemplate;
                    CommandDataInstance cdi = GameManager._currentPlayer.commands[commandContainer.slot].commandDataInstance;
                    if (cdi.CanRun())
                    {
                        commandContainer.button.interactable = true;
                    }
                    else
                    {
                        commandContainer.button.interactable = false;
                    }
                }
            }
        I.mainPanel.SetActive(true);
    }

    public static void Hide()
    {
        I.mainPanel.SetActive(false);
    }

    public static void GenerateUIForCommands(PlayerController pc)
    {
        for(int i = 0; i < pc.commandList.Count; i++)
        {
            CommandTemplate ct = pc.GetWeaponTemplateForCommand(pc.commandList[i].slot);
            UIMetaData uid = ct.getUIMetadata();
            if(pc.commandList[i].button != null)
            {
                Image img = pc.commandList[i].button.GetComponent<Image>();
                Sprite cmdImg = Resources.Load<Sprite>("Images/" + uid.commandImage);
                img.sprite = cmdImg;
            }
        }
    }

    public static void ShowMessage(string message)
    {
        LeanTween.scale(I.messagePanel.GetComponent<RectTransform>(), Vector3.one, .25f);
        TMP_Text messageText = I.messagePanel.GetComponentInChildren<TMP_Text>();
        messageText.text = message;
        I.StartCoroutine(I.HideMessage());
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(2);
        LeanTween.scale(I.messagePanel.GetComponent<RectTransform>(), Vector3.zero, .25f);
    }

    public static void ActivateBeginTurnMessage()
    {
        LeanTweenExt.LeanAlphaText(I.beginTurnMsg, 1, 1.5f).setOnComplete(
            ()=>{
                LeanTweenExt.LeanAlphaText(I.beginTurnMsg, 0, 1.5f);
        });
    }

    public static void ActivateBeginTurnMessageAI()
    {
        LeanTweenExt.LeanAlphaText(I.beginTurnMsgAI, 1, 1.5f).setOnComplete(
            () => {
                LeanTweenExt.LeanAlphaText(I.beginTurnMsgAI, 0, 1.5f);
            });
    }

    public static void Talk(string speech, Vector3 position)
    {
        GameObject speechObject = Instantiate(I.speecPrefab, position, Quaternion.identity);
        speechObject.GetComponent<SpeechBubble>().SetText(speech);
    }

    public static void DamageNotification(int dmgAmt, bool critical, Vector3 position)
    {
        string dmgMesg = null;
        GameObject notification = I.uiNotifications.floatTextPrefab;
        if (dmgAmt <= 0)
        {
            dmgMesg = "Missed...";
        }
        else
        {
            if (critical)
            {
                dmgMesg = "Critical!!\n" + dmgAmt;
                notification = I.uiNotifications.criticalDamageTextPrefab;
            }
            else
            {
                if (dmgAmt < 10)
                {
                    notification = I.uiNotifications.mediumDamageTextPrefab;
                }
                else if (dmgAmt > 10)
                {
                    notification = I.uiNotifications.highDamageTextPrefab;
                }
                dmgMesg = dmgAmt.ToString();
            }

        }
        GameObject floatTextObj = Instantiate(notification, position, Quaternion.identity);
        floatTextObj.GetComponent<FloatingText>().SetText(dmgMesg);
    }

    public static void DisplayAmmo(int ammo)
    {
        I.ammoCounter.text = ammo.ToString();
        //TODO: Add ammo depletion effect here.
    }

    public static void DisplayTurnsLeftForItemUse(int turnsLeft)
    {
        if(turnsLeft <= 0)
        {
            I.turnsLeftForItemUse.text = " Now";
        } else
        {
            I.turnsLeftForItemUse.text = String.Format(" in {0} turns", turnsLeft);
        }
    }

    public static void ShowAttackData(string content, GameObject host)
    {
        I.attackData.trackedObject = host;
        I.attackData.content.text = content;
        I.attackData.Show();
    }

    public static void HideAttackData()
    {
        I.attackData.Hide();
    }

    public static void ShowActionCam()
    {
        Time.timeScale = .7f;
        I.cmActionCam.Priority = CM_MAX_PRIORITY;
        I.actionCam.gameObject.SetActive(true);
    }

    public static bool ActionCamObstructed(Transform origin)
    {
        bool somethingInTheWay = GeneralUtils.IsActionCamBlocked(origin.position, I.cmActionCam.transform.position);

        return somethingInTheWay;
    }


    public static void MoveActioncam(Transform actionCamHook)
    {
        I.cmActionCam.transform.position = actionCamHook.position;
        I.cmActionCam.transform.rotation = actionCamHook.rotation;
    }

    public static void HideActionCam()
    {
        Time.timeScale = 1;
        I.cmActionCam.Priority = CM_MIN_PRIORITY;
        I.actionCam.gameObject.SetActive(false);
        I.fadePanel.gameObject.SetActive(false);
    }

    public static void StartCamShake()
    {
        CinemachineBasicMultiChannelPerlin noise = I.cmGameCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CinemachineBasicMultiChannelPerlin actionCamNoise = I.cmActionCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 1;
        actionCamNoise.m_AmplitudeGain = 1;
    }

    public static void StopCamShake()
    {
        CinemachineBasicMultiChannelPerlin noise = I.cmGameCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CinemachineBasicMultiChannelPerlin actionCamNoise = I.cmActionCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0;
        actionCamNoise.m_AmplitudeGain = 0;
    }



    public delegate void OnBlack<T>(T payload);
    public delegate void OnComplete<T>(T payload);
    public static void FadeToBlack(Transform actionCamHook, OnBlack<Transform> onBlack, OnComplete<Transform> onComplete)
    {
        I.fadePanel.gameObject.SetActive(true);
        Color imgColor = I.fadePanel.color;
        imgColor.a = 0;
        I.StartCoroutine(FadeIn(actionCamHook, onBlack, onComplete, I.fadePanel, 1f));
    }

    static IEnumerator FadeIn<T>(T g, OnBlack<T> onBlack, OnComplete<T> onComplete, Image fadePanel, float aTime)
    {
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(0, 0, 0, Mathf.Lerp(0, 1f, t));
            fadePanel.color = newColor;
            yield return null;
        }
        onBlack(g);
        I.StartCoroutine(FadeOut(g, onComplete, fadePanel, aTime));
    }

    static IEnumerator FadeOut<T>(T g, OnComplete<T> onComplete, Image fadePanel, float aTime)
    {
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(0, 0, 0, Mathf.Lerp(1, 0, t));
            fadePanel.color = newColor;
            yield return null;
        }
        onComplete(g);
    }

    #endregion

    private void Update()
    {
        if(GameManager.playerSelected)
        {
            PlayerController character = GameManager._currentPlayer;
            if (GameManager._currentPlayer.isAgent || !(GameManager._currentPlayer.getCurrentCommand() is IdleFSM)) //don't show control panel for AI.
            {
                Hide();
            } else
            {
               Show();
            }
        }   
    }





}
