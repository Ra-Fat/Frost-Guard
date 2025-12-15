using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Jobs;

public class GameLoopManager : MonoBehaviour
{

    public static Vector3[] NodePositions;
    public static Queue<Enemy> EnemiesToRemove;
    public static Queue<int> EnemyIdsToSummon;
    public static bool LoopShouldEnd;
    public Transform NodeParent;



    private void Start()
    {
       LoopShouldEnd = false;
       EnemyIdsToSummon = new Queue<int>();
       EnemiesToRemove = new Queue<Enemy>();
       EntitySummoner.Init();

        NodePositions = new Vector3[NodeParent.childCount];

        for(int i=0; i<NodePositions.Length; i++)
        {
            NodePositions[i] = NodeParent.GetChild(i).position;
        }

       StartCoroutine(GameLoop());
       InvokeRepeating("SummonTest", 0f, 1f);
    }

    void RemoveTest()
    {
        if (EntitySummoner.EnemiesInGame.Count > 0)
        {
            EntitySummoner.RemoveEnemy(EntitySummoner.EnemiesInGame[Random.Range(0, EntitySummoner.EnemiesInGame.Count)]);
        }
    }

    void SummonTest()
    {
        EnqueueEnemyToSummon(1);
    }



    IEnumerator GameLoop()
    {
        while(LoopShouldEnd == false)
        {
            if(EnemyIdsToSummon.Count > 0)
            {
                for(int i = 0; i < EnemyIdsToSummon.Count; i++ )
                {
                    EntitySummoner.SummonEnemy(EnemyIdsToSummon.Dequeue());
                }
            }


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

            for(int i=0; i < EntitySummoner.EnemiesInGame.Count; i++)
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
        Vector3 PositionTOMoveTo = NodePositions[NodeIndex[index]];
        transform.position = Vector3.MoveTowards(transform.position, PositionTOMoveTo, EnemySpeeds[index] * DeltaTime);

        if(transform.position == PositionTOMoveTo)
        {
            //Update Node Index
            if(NodeIndex[index] < NodePositions.Length)
            {
                NodeIndex[index]++;
            }
        }
    }
}
