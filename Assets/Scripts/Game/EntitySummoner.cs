using System.Collections.Generic;
using UnityEngine;

public class EntitySummoner : MonoBehaviour
{
    public static List<Enemy> EnemiesInGame;
    public static List<Transform> EnemiesInGameTransforms;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> EnemyObjectPools;

    private static bool isInitialized;
    public static void Init()
    {
        if(!isInitialized)
        {
            EnemiesInGame = new List<Enemy>();
            EnemyPrefabs = new Dictionary<int, GameObject>();
            EnemiesInGameTransforms = new List<Transform>();
            EnemyObjectPools = new Dictionary<int, Queue<Enemy>>();

            // Load enemy prefabs from Resources/Enemies folder
            EnemySummonData[] Enemies = Resources.LoadAll<EnemySummonData>("Enemies");
            foreach(EnemySummonData enemy in Enemies)
            {
                EnemyPrefabs.Add(enemy.EnemyID, enemy.EnemyPrefab);
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
        if(EnemyPrefabs.ContainsKey(EnemyID))
        {
            Queue<Enemy> ReferenceQueue = EnemyObjectPools[EnemyID];
            if(ReferenceQueue.Count > 0)
            {
                //Dequeue Enemy and initialize
                SummonedEnemy = ReferenceQueue.Dequeue();
                SummonedEnemy.Init();
                SummonedEnemy.gameObject.SetActive(true);
            }
            else
            {
                //Instantiate new Enemy
                GameObject NewEnemy = Instantiate(EnemyPrefabs[EnemyID], GameLoopManager.NodePositions[0], Quaternion.identity);
                SummonedEnemy = NewEnemy.GetComponent<Enemy>();
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

    public static void RemoveEnemy(Enemy EnemyToRemove)
    {
        EnemyObjectPools[EnemyToRemove.ID].Enqueue(EnemyToRemove);
        EnemyToRemove.gameObject.SetActive(false);
        EnemiesInGameTransforms.Remove(EnemyToRemove.transform);
        EnemiesInGame.Remove(EnemyToRemove);
    }
}
