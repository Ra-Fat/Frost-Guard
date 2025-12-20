using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool isGameOver = false;

    void Update()
    {
        if (isGameOver)
        {
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
}