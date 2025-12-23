using System.Collections.Generic;
using UnityEngine;

public class EntitySummoner : MonoBehaviour
{
    public static List<Enemy> EnemiesInGame;
    public static List<Transform> EnemiesInGameTransforms;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> EnemyObjectPools;
    private static bool isInitialized;

    public static Dictionary<int, EnemySummonData> EnemyData; // Store full data

    public static void Init()
    {
        if (!isInitialized)
        {
            EnemiesInGame = new List<Enemy>();
            EnemyPrefabs = new Dictionary<int, GameObject>();
            EnemyData = new Dictionary<int, EnemySummonData>();
            EnemiesInGameTransforms = new List<Transform>();
            EnemyObjectPools = new Dictionary<int, Queue<Enemy>>();

            EnemySummonData[] Enemies = Resources.LoadAll<EnemySummonData>("Enemies");
            foreach (EnemySummonData enemy in Enemies)
            {
                EnemyPrefabs.Add(enemy.EnemyID, enemy.EnemyPrefab);
                EnemyData.Add(enemy.EnemyID, enemy); // Store full data
                EnemyObjectPools.Add(enemy.EnemyID, new Queue<Enemy>());
            }
            isInitialized = true;
        }
        else
        {
            Debug.Log("EntitySummoner is already initialized.");
        }
    }

    public static Enemy SummonEnemy(int EnemyID)
    {
        Enemy SummonedEnemy = null;

        if (EnemyPrefabs.ContainsKey(EnemyID))
        {
            Queue<Enemy> ReferenceQueue = EnemyObjectPools[EnemyID];

            if (ReferenceQueue.Count > 0)
            {
                //Dequeue Enemy and initialize
                SummonedEnemy = ReferenceQueue.Dequeue();

                // Check if the enemy object is still valid (not destroyed)
                if (SummonedEnemy == null)
                {
                    // Object was destroyed, create a new one instead
                    GameObject NewEnemy = Instantiate(EnemyPrefabs[EnemyID], GameLoopManager.NodePositions[0], Quaternion.identity);

                    // FIXED: Use GetComponentInChildren to find Enemy script in child
                    SummonedEnemy = NewEnemy.GetComponentInChildren<Enemy>();

                    if (SummonedEnemy == null)
                    {
                        Debug.LogError($"Enemy prefab {NewEnemy.name} has no Enemy component in hierarchy!");
                        Destroy(NewEnemy);
                        return null;
                    }

                    // Apply stats from ScriptableObject
                    ApplyEnemyStats(SummonedEnemy, EnemyID);
                    SummonedEnemy.Init();
                }
                else
                {
                    // Apply stats from ScriptableObject before Init
                    ApplyEnemyStats(SummonedEnemy, EnemyID);
                    SummonedEnemy.Init();
                    SummonedEnemy.gameObject.SetActive(true);
                }
            }
            else
            {
                //Instantiate new Enemy
                GameObject NewEnemy = Instantiate(EnemyPrefabs[EnemyID], GameLoopManager.NodePositions[0], Quaternion.identity);

                // FIXED: Use GetComponentInChildren to find Enemy script in child
                SummonedEnemy = NewEnemy.GetComponentInChildren<Enemy>();

                if (SummonedEnemy == null)
                {
                    Debug.LogError($"Enemy prefab {NewEnemy.name} has no Enemy component in hierarchy!");
                    Destroy(NewEnemy);
                    return null;
                }

                // Apply stats from ScriptableObject
                ApplyEnemyStats(SummonedEnemy, EnemyID);
                SummonedEnemy.Init();
            }
        }
        else
        {
            Debug.LogError("No prefab found for EnemyID: " + EnemyID);
            return null;
        }

        EnemiesInGameTransforms.Add(SummonedEnemy.transform);
        EnemiesInGame.Add(SummonedEnemy);
        SummonedEnemy.ID = EnemyID;

        return SummonedEnemy;
    }

    static void ApplyEnemyStats(Enemy enemy, int enemyID)
    {
        if (EnemyData.ContainsKey(enemyID))
        {
            EnemySummonData data = EnemyData[enemyID];
            enemy.Speed = data.Speed;
            enemy.MaxHealth = data.MaxHealth;
            Debug.Log($"Applied stats to {enemy.name}: Speed={enemy.Speed}, MaxHealth={enemy.MaxHealth}");
        }
    }

    public static void RemoveEnemy(Enemy EnemyToRemove)
    {
        EnemyObjectPools[EnemyToRemove.ID].Enqueue(EnemyToRemove);
        EnemyToRemove.gameObject.SetActive(false);
        EnemiesInGameTransforms.Remove(EnemyToRemove.transform);
        EnemiesInGame.Remove(EnemyToRemove);
    }

    public static void Cleanup()
    {
        if (EnemiesInGame != null)
        {
            EnemiesInGame.Clear();
        }
        if (EnemiesInGameTransforms != null)
        {
            EnemiesInGameTransforms.Clear();
        }
        if (EnemyObjectPools != null)
        {
            foreach (var pool in EnemyObjectPools.Values)
            {
                pool.Clear();
            }
        }
        isInitialized = false;
    }
}