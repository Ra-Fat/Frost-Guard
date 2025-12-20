using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MaxHealth;
    [HideInInspector] public float Health;
    public float Speed;
    public int MoneyReward = 50;
    public int ID;
    public int NodeIndex;

    public void Init()
    {
        Health = MaxHealth;
        transform.position = GameLoopManager.NodePositions[0];
        NodeIndex = 0;
    }
}
