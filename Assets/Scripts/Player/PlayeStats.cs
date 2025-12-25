using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    public static int Lives;
    public int startingMoney = 400;
    public static int rounds;
    public int StartLives = 4;

    private void Start()
    {
        Money = startingMoney;
        Lives = StartLives;
        rounds = 0;
    }
}