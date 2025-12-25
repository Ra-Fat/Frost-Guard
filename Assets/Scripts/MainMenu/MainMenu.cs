using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string LevelToLoad = "Sample Scene";

    [Header("Menu Music")]
    public AudioSource menuAudioSource;
    public AudioClip menuMusic;

    private void Start()
    {
        if (menuAudioSource != null && menuMusic != null)
        {
            menuAudioSource.clip = menuMusic;
            menuAudioSource.loop = true;
            menuAudioSource.Play();
        }
    }

    public void Play ()
    {
        Debug.Log("Play");
        if (menuAudioSource != null && menuAudioSource.isPlaying)
        {
            menuAudioSource.Stop();
        }
        SceneManager.LoadScene(LevelToLoad);
    }

    public void Quit ()
    {
        Debug.Log("Exiting......");
        Application.Quit();
    }
}
