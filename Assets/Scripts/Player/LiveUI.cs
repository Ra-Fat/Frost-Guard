using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LiveUI : MonoBehaviour
{
    public GameObject[] heartObjects; // Assign your heart GameObjects in Inspector

    void Update()
    {
        for (int i = 0; i < heartObjects.Length; i++)
        {
            if (i < PlayerStats.Lives)
                heartObjects[i].SetActive(true);
            else
                heartObjects[i].SetActive(false);
        }
    }
}
