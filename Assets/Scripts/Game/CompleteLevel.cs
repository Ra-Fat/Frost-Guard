using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteLevel : MonoBehaviour
{
    public string menuSceneName = "Main Menu";
    public void Contine ()
    {
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
        Debug.Log("Next Level Coming Soon!");
    }
    public void Menu ()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
    }
}
