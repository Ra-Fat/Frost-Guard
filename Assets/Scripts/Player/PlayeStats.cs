using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    public static int Lives;
    public int startingMoney = 400;

    public int StartLives = 20;

    private void Start()
    {
        Money = startingMoney;
        Lives = StartLives;
    }
}