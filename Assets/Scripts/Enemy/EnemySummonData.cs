using UnityEngine;

[CreateAssetMenu(fileName = "EnemySummonData", menuName = "Game/EnemySummonData")]
public class EnemySummonData : ScriptableObject
{
    public int EnemyID;
    public GameObject EnemyPrefab;

    [Header("Enemy Stats")]
    public float Speed = 2f;
    public float MaxHealth = 20f;
}