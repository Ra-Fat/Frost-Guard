using UnityEngine;

[CreateAssetMenu(fileName = "EnemySummonData", menuName = "Game/EnemySummonData")]
public class EnemySummonData : ScriptableObject
{
    public int EnemyID;
    public GameObject EnemyPrefab;
}
