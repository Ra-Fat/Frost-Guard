using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using TMPro;

public class GameLoopManager : MonoBehaviour
{
    public static Vector3[] NodePositions;
    public static Queue<Enemy> EnemiesToRemove;
    public static Queue<int> EnemyIdsToSummon;
    public static bool LoopShouldEnd;
    public Transform NodeParent;

    [Header("UI Elements")]
    public TextMeshProUGUI countdownText; // Assign in inspector

    // Enemy type IDs (set these to match your EntitySummoner IDs)
    [Header("Enemy Types")]
    public int enemyType1Id = 1;
    public int enemyType2Id = 2;
    public int enemyType3Id = 3;

    // Wave spawning settings
    [Header("Wave Spawn Settings")]
    public float timeBetweenWaves = 10f; // Set to 10s as requested
    public int initialEnemiesPerWave = 10;
    public int enemiesIncrementPerWave = 10;
    public int totalWaves = 5;
    public float spawnOffset = 0.5f; // Distance offset between spawned enemies

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip backgroundMusic;

    private int currentWave = 1;
    private bool isFirstWave = true;
    private bool allWavesSpawned = false;

    private void Start()
    {
        LoopShouldEnd = false;
        EnemyIdsToSummon = new Queue<int>();
        EnemiesToRemove = new Queue<Enemy>();
        EntitySummoner.Init();

        NodePositions = new Vector3[NodeParent.childCount];
        for (int i = 0; i < NodePositions.Length; i++)
        {
            NodePositions[i] = NodeParent.GetChild(i).position;
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        StartCoroutine(GameLoop());
        StartCoroutine(WaveSpawner());
    }

    IEnumerator WaveSpawner()
    {
        while (LoopShouldEnd == false && currentWave <= totalWaves)
        {
            if (isFirstWave)
            {
                yield return StartCoroutine(ShowCountdown(3));
                if (audioSource != null && backgroundMusic != null)
                {
                    audioSource.clip = backgroundMusic;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                isFirstWave = false;
            }
            else
            {
                yield return StartCoroutine(ShowCountdown((int)timeBetweenWaves));
            }

            int enemiesThisWave = initialEnemiesPerWave + (currentWave - 1) * enemiesIncrementPerWave;
            yield return StartCoroutine(SpawnWaveCoroutine(enemiesThisWave));
            currentWave++;
            PlayerStats.rounds++;
        }
        allWavesSpawned = true;
    }

    IEnumerator ShowCountdown(int seconds)
    {
        if (countdownText == null)
        {
            yield return new WaitForSeconds(seconds);
            yield break;
        }

        countdownText.gameObject.SetActive(true);
        float timeRemaining = seconds;
        while (timeRemaining > 0)
        {
            countdownText.text = string.Format("{0:00.00}", timeRemaining);
            yield return null;
            timeRemaining -= Time.deltaTime;
        }
        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
    }

    IEnumerator SpawnWaveCoroutine(int enemyCount)
    {
        int enemyTypeId = enemyType1Id;
        if (currentWave <= 3)
        {
            enemyTypeId = enemyType1Id;
        }
        else if (currentWave == 4)
        {
            enemyTypeId = enemyType2Id;
        }
        else if (currentWave == 5)
        {
            enemyTypeId = enemyType3Id;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            yield return new WaitForSeconds(0.2f);

            Vector3 direction = (NodePositions[0] - NodePositions[1]).normalized;
            float offset = i * spawnOffset;
            Vector3 spawnPosition = NodePositions[0] + direction * offset;

            Vector3 originalFirstNode = NodePositions[0];
            NodePositions[0] = spawnPosition;
            EnqueueEnemyToSummon(enemyTypeId);
            NodePositions[0] = originalFirstNode;
        }
    }

    IEnumerator GameLoop()
    {
        while (LoopShouldEnd == false)
        {
            if (EnemyIdsToSummon.Count > 0)
            {
                for (int i = 0; i < EnemyIdsToSummon.Count; i++)
                {
                    EntitySummoner.SummonEnemy(EnemyIdsToSummon.Dequeue());
                }
            }

            if (EntitySummoner.EnemiesInGame.Count > 0)
            {
                NativeArray<Vector3> NodeToUse = new NativeArray<Vector3>(NodePositions, Allocator.TempJob);
                NativeArray<float> EnemySpeeds = new NativeArray<float>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
                NativeArray<int> NodeIndices = new NativeArray<int>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
                TransformAccessArray EnemyAccess = new TransformAccessArray(EntitySummoner.EnemiesInGameTransforms.ToArray(), 2);

                for (int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++)
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

                for (int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++)
                {
                    EntitySummoner.EnemiesInGame[i].NodeIndex = NodeIndices[i];

                    if (EntitySummoner.EnemiesInGame[i].NodeIndex == NodePositions.Length)
                    {
                        PlayerStats.Lives = Mathf.Max(PlayerStats.Lives - 1, 0);
                        EnqueueEnemyToRemove(EntitySummoner.EnemiesInGame[i]);
                    }
                }

                NodeToUse.Dispose();
                EnemySpeeds.Dispose();
                NodeIndices.Dispose();
                EnemyAccess.Dispose();
            }

            if (EnemiesToRemove.Count > 0)
            {
                for (int i = 0; i < EnemiesToRemove.Count; i++)
                {
                    EntitySummoner.RemoveEnemy(EnemiesToRemove.Dequeue());
                }
            }

            // Check win condition: all waves spawned, no enemies queued to summon, and no enemies alive
            if (allWavesSpawned && EnemyIdsToSummon.Count == 0 && EntitySummoner.EnemiesInGame.Count == 0)
            {
                if (!GameManager.isGameOver && PlayerStats.Lives > 0)
                {
                    GameManager gameManager = FindObjectOfType<GameManager>();
                    if (gameManager != null)
                    {
                        gameManager.WinLevel();
                    }
                }
                LoopShouldEnd = true;
                yield break;
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
        if (NodeIndex[index] >= NodePositions.Length)
        {
            return;
        }

        Vector3 PositionToMoveTo = NodePositions[NodeIndex[index]];
        transform.position = Vector3.MoveTowards(transform.position, PositionToMoveTo, EnemySpeeds[index] * DeltaTime);

        if (transform.position == PositionToMoveTo)
        {
            if (NodeIndex[index] < NodePositions.Length)
            {
                NodeIndex[index]++;
            }
        }
    }
}
