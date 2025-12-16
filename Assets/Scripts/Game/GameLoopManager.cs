using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GameLoopManager : MonoBehaviour
{
    public static Vector3[] NodePositions;
    public static Queue<Enemy> EnemiesToRemove;
    public static Queue<int> EnemyIdsToSummon;
    public static bool LoopShouldEnd;
    public Transform NodeParent;

    // Wave spawning settings
    [Header("Wave Spawn Settings")]
    public float timeBetweenWaves = 5f;
    public int initialEnemiesPerWave = 10;
    public int enemiesIncrementPerWave = 10;
    public int totalWaves = 5;
    public float spawnOffset = 0.5f; // Distance offset between spawned enemies
    
    private int currentWave = 1;
    private float waveTimer = 0f;

    private void Start()
    {
       LoopShouldEnd = false;
       EnemyIdsToSummon = new Queue<int>();
       EnemiesToRemove = new Queue<Enemy>();
       EntitySummoner.Init();

        NodePositions = new Vector3[NodeParent.childCount];

        for(int i = 0; i < NodePositions.Length; i++)
        {
            NodePositions[i] = NodeParent.GetChild(i).position;
        }

        StartCoroutine(GameLoop());
        StartCoroutine(WaveSpawner());
    }

    IEnumerator WaveSpawner()
    {
        while(LoopShouldEnd == false && currentWave <= totalWaves)
        {
            int enemiesThisWave = initialEnemiesPerWave + (currentWave - 1) * enemiesIncrementPerWave;
            SpawnWave(enemiesThisWave);
            currentWave++;
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    void SpawnWave(int enemyCount)
    {
        for(int i = 0; i < enemyCount; i++)
        {
            // Queue enemy with a slight delay based on index
            StartCoroutine(SpawnEnemyWithDelay(i * 0.2f, i));
        }
    }

    IEnumerator SpawnEnemyWithDelay(float delay, int offsetIndex)
    {
        yield return new WaitForSeconds(delay);
        
        // Calculate offset position behind the first node
        Vector3 direction = (NodePositions[0] - NodePositions[1]).normalized;
        float offset = offsetIndex * spawnOffset;
        Vector3 spawnPosition = NodePositions[0] + direction * offset;
        
        // Store original first node position
        Vector3 originalFirstNode = NodePositions[0];
        
        // Temporarily change first node to spawn position
        NodePositions[0] = spawnPosition;
        
        EnqueueEnemyToSummon(1);
        
        // Restore original position after a frame
        StartCoroutine(RestoreNodePosition(originalFirstNode));
    }

    IEnumerator RestoreNodePosition(Vector3 originalPosition)
    {
        yield return null;
        NodePositions[0] = originalPosition;
    }

    IEnumerator GameLoop()
    {
        while(LoopShouldEnd == false)
        {
            if(EnemyIdsToSummon.Count > 0)
            {
                for(int i = 0; i < EnemyIdsToSummon.Count; i++)
                {
                    EntitySummoner.SummonEnemy(EnemyIdsToSummon.Dequeue());
                }
            }

            // Only process movement if there are enemies
            if(EntitySummoner.EnemiesInGame.Count > 0)
            {
                //Move Enemies
                NativeArray<Vector3> NodeToUse = new NativeArray<Vector3>(NodePositions, Allocator.TempJob);
                NativeArray<float> EnemySpeeds = new NativeArray<float>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
                NativeArray<int> NodeIndices = new NativeArray<int>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
                TransformAccessArray EnemyAccess = new TransformAccessArray(EntitySummoner.EnemiesInGameTransforms.ToArray(), 2);

                for(int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++)
                {
                    EnemySpeeds[i] = EntitySummoner.EnemiesInGame[i].Speed;
                    NodeIndices[i] = EntitySummoner.EnemiesInGame[i].NodeIndex;
                }

                MoveEnemiesJob MoveJob = new MoveEnemiesJob
                {
                    NodePositions = NodeToUse,
                    EnemySpeeds = EnemySpeeds,
                    NodeIndex = NodeIndices,
                    DeltaTime = Time.deltaTime
                };

                JobHandle MoveJobHandle = MoveJob.Schedule(EnemyAccess);
                MoveJobHandle.Complete();

                for(int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++)
                {
                    EntitySummoner.EnemiesInGame[i].NodeIndex = NodeIndices[i];

                    if(EntitySummoner.EnemiesInGame[i].NodeIndex == NodePositions.Length)
                    {
                        EnqueueEnemyToRemove(EntitySummoner.EnemiesInGame[i]);
                    }
                }

                NodeToUse.Dispose();
                EnemySpeeds.Dispose();
                NodeIndices.Dispose();
                EnemyAccess.Dispose();
            }

            if(EnemiesToRemove.Count > 0)
            {
                for(int i = 0; i < EnemiesToRemove.Count; i++)
                {
                    EntitySummoner.RemoveEnemy(EnemiesToRemove.Dequeue());
                }
            }

            yield return null;
        }
    }

    public static void EnqueueEnemyToSummon(int ID)
    {
        EnemyIdsToSummon.Enqueue(ID);
    }

    public static void EnqueueEnemyToRemove(Enemy EnemyToRemove)
    {
        EnemiesToRemove.Enqueue(EnemyToRemove);
    }

    private void OnDestroy()
    {
        LoopShouldEnd = true;
        EntitySummoner.Cleanup();
    }
}

public struct MoveEnemiesJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> NodePositions;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> EnemySpeeds;
    [NativeDisableParallelForRestriction]
    public NativeArray<int> NodeIndex;
    public float DeltaTime;
    
    public void Execute(int index, TransformAccess transform)
    {
        if(NodeIndex[index] >= NodePositions.Length)
        {
            return;
        }
        
        //Move enemy towards next node
        Vector3 PositionToMoveTo = NodePositions[NodeIndex[index]];
        transform.position = Vector3.MoveTowards(transform.position, PositionToMoveTo, EnemySpeeds[index] * DeltaTime);

        if(transform.position == PositionToMoveTo)
        {
            //Update Node Index
            if(NodeIndex[index] < NodePositions.Length)
            {
                NodeIndex[index]++;
            }
        }
    }
}