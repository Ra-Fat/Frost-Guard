using System.Collections.Generic;
using UnityEngine;

// Struct for manual prefab assignment
[System.Serializable]
public struct EnemyPrefabEntry
{
    public int EnemyID;
    public GameObject EnemyPrefab;
}

public class EntitySummoner : MonoBehaviour
{
    [Header("Assign enemy prefabs and IDs here")]
    public EnemyPrefabEntry[] enemyPrefabEntries;
    public static List<Enemy> EnemiesInGame;
    public static List<Transform> EnemiesInGameTransforms;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> EnemyObjectPools;
    private static bool isInitialized;

    // Removed EnemyData and ScriptableObject logic

    public static void Init()
    {
        if (!isInitialized)
        {
            EnemiesInGame = new List<Enemy>();
            EnemyPrefabs = new Dictionary<int, GameObject>();
            EnemiesInGameTransforms = new List<Transform>();
            EnemyObjectPools = new Dictionary<int, Queue<Enemy>>();

            // Populate EnemyPrefabs from Inspector array
            EntitySummoner instance = FindObjectOfType<EntitySummoner>();
            if (instance != null && instance.enemyPrefabEntries != null)
            {
                foreach (var entry in instance.enemyPrefabEntries)
                {
                    if (!EnemyPrefabs.ContainsKey(entry.EnemyID) && entry.EnemyPrefab != null)
                    {
                        EnemyPrefabs.Add(entry.EnemyID, entry.EnemyPrefab);
                        EnemyObjectPools.Add(entry.EnemyID, new Queue<Enemy>());
                    }
                }
            }
            else
            {
                Debug.LogError("No EntitySummoner instance or enemyPrefabEntries not set!");
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

                    SummonedEnemy.Init();
                }
                else
                {
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

    // Removed ApplyEnemyStats and all ScriptableObject stat application

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