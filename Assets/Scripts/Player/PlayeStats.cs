using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    public int startingMoney =400;

    private void Start()
    {
        Money = startingMoney;
    }
}