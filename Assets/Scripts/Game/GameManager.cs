using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool isGameOver;
    public GameObject gameOverUI;   
    public GameObject completeLevelUI;

    void Start() {
        isGameOver = false;
    }

    void Update()
    {
        if (isGameOver)
        {
           gameOverUI.SetActive(true);
           return;
        }
        if (PlayerStats.Lives <= 0)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        isGameOver = true;
        Debug.Log("Game Over!");
    }

    public void WinLevel()
    {
        isGameOver = false;
        completeLevelUI.SetActive(true);
    }
}