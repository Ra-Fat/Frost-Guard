using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string LevelToLoad = "Sample Scene";
    public void Play ()
    {
        Debug.Log("Play");
        SceneManager.LoadScene(LevelToLoad);
    }

    public void Quit ()
    {
        Debug.Log("Exiting......");
        Application.Quit();
    }

}
