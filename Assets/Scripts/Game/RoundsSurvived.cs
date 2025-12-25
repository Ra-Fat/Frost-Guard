using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class RoundsSurvived : MonoBehaviour
{
    public TMP_Text roundsText;

    void OnEnable()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText ()
    {
        int rounds = 0;
        roundsText.text = "0";
        yield return new WaitForSeconds(0.7f);
        while (rounds < PlayerStats.rounds)
        {
            rounds++;
            roundsText.text = rounds.ToString();
            yield return new WaitForSeconds(0.05f);
        }
    }
}
