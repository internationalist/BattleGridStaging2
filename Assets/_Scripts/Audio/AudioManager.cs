using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    #region singleton
    private static AudioManager _instance;
    public static AudioManager I
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
        this._mainAudio = GetComponent<AudioSource>();
        this.lastPlayedFoots = new Queue<AudioClip>();
    }
    #endregion

    #region State
    public AudioSource _mainAudio;
    public AudioClip _click;
    public AudioClip _noMoreAP;
    public AudioClip _showUI;
    public AudioClip _playerTurn;
    public AudioClip _enemyTurn;
    public AudioClip _enemySpawn;

    public List<AudioClip> footsteps;
    public int footStepsNoRepeatRange;
    Queue<AudioClip> lastPlayedFoots;
    public AudioMixerSnapshot defaultSound;
    public AudioMixerSnapshot actionSound;
    public AudioSource musicAudio;

    public List<AudioClip> backingTracks;
    Queue<AudioClip> lastPlayedTracks;
    public int trackNoRepeatRange;

    private bool transitioning;
    #endregion

    public static void ClickButton()
    {
        I._mainAudio.clip = I._click;
        I._mainAudio.Play();
    }

    public void PlayEnemySpawn(AudioSource auSrc)
    {
        auSrc.PlayOneShot(_enemySpawn);
    }

    public static void NoMoreAP()
    {
        I._mainAudio.clip = I._noMoreAP;
        I._mainAudio.Play();
    }

    public static void ShowSelctionUI()
    {
        I._mainAudio.clip = I._showUI;
        I._mainAudio.Play();
    }

    public static void BeginPlayerTurn()
    {
        I._mainAudio.clip = I._playerTurn;
        I._mainAudio.Play();
    }

    public static void BeginEnemyTurn()
    {
        I._mainAudio.clip = I._enemyTurn;
        I._mainAudio.Play();
    }

    public static IEnumerator TransitionToActionAndBack(float delay)
    {
        if(!I.transitioning)
        {
            I.transitioning = true;
            I.actionSound.TransitionTo(.01f);
            yield return new WaitForSeconds(delay);
            I.defaultSound.TransitionTo(.2f);
            I.transitioning = false;
        }
    }

    public static void PlayFootSteps(AudioSource ausrc)
    {
        AudioClip footStep = GeneralUtils.GetRandomAudio(I.footsteps, I.lastPlayedFoots, I.footStepsNoRepeatRange);
        I.StartCoroutine(TransitionToActionAndBack(footStep.length));
        if (footStep != null)
        {
            ausrc.PlayOneShot(footStep);
        }
    }

    public static IEnumerator PlayRicochet(List<AudioClip> leading, List<AudioClip> trailing, AudioSource auSrc)
    {
        
        int idx = Random.Range(0, leading.Count);
        AudioClip ricochetSound = leading[idx];
        idx = Random.Range(0, trailing.Count);
        AudioClip ricochetTrailing = trailing[idx];
        I.StartCoroutine(TransitionToActionAndBack(ricochetTrailing.length + ricochetSound.length));
        auSrc.PlayOneShot(ricochetSound);
        yield return new WaitForSeconds(.1f);
        auSrc.PlayOneShot(ricochetTrailing);
    }

    public static void PlayVoice(List<AudioClip> voices, AudioSource auSrc)
    {
        int idx = Random.Range(0, voices.Count);
        AudioClip groan = voices[idx];
        I.StartCoroutine(TransitionToActionAndBack(groan.length));
        auSrc.PlayOneShot(groan);
    }


}
