using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    [Serializable]
    public class UIAudio
    {
        [SerializeField] AudioSource uiAudio;
        [SerializeField] AudioClip mapSelectSound;
        [SerializeField] AudioClip playButtonSound;

    }

    [SerializeField] Button playButton;
    [SerializeField] GameObject disabledPlayButton;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] UIAudio _uiAudio;

    [SerializeField] string lvlName;


    

    public void SelectedMap(string mapName)
    {
        lvlName = mapName;
        playButton.interactable = true;
        disabledPlayButton.SetActive(false);
    }

    public void PlayLevel()
    {
        loadingScreen.SetActive(true);
        loadingScreen.GetComponent<Animator>().SetTrigger("In");
        StartCoroutine(SceneLoading());
    }
    public void BackToMenu(string mapName)
    {
        lvlName = mapName;
        loadingScreen.GetComponent<Animator>().SetTrigger("In");
        StartCoroutine(SceneLoading());
    }

    IEnumerator SceneLoading()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(lvlName);
    }

}
