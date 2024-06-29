using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial class PeepoEventSystem : SystemBase
{
    public Action OnSpawn;
    public Action<int, int> OnChat;
    public Action<int> OnDead;

    public Action OnCalm;
    BlobAssetReference<PeepoConfig> peepoConfig;
    protected override void OnCreate()
    {
        base.OnCreate();
        CheckedStateRef.RequireForUpdate<GameManagerSingletonComponent>();
    }
    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        peepoConfig = SystemAPI.GetSingleton<GameManagerSingletonComponent>().peepoConfig;
        OnSpawn = () =>
        {
            new OnSpawnPeepoJob { parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnChat = (hashID, addValue) =>
        {
            new OnChatPeepoJob { hashID = hashID, addValue = addValue, peepoConfig = peepoConfig.Value }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnDead = (hashID) =>
        {
            new OnDestroyPeepoJob { hashID = hashID }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnCalm = () =>
        {
            new OnCalmPeepoJob().ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
    }

    protected override void OnUpdate()
    {
        new PeepoInitJob().ScheduleParallel();
    }
    [BurstCompile]
    public partial struct OnCalmPeepoJob : IJobEntity
    {
        public void Execute(ref PeepoComponent peepoComponent)
        {
            //Debug.Log("진정");
            peepoComponent.currentState = PeepoState.Idle;
        }
    }

    [BurstCompile]
    public partial struct OnSpawnPeepoJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter parallelWriter;

        public void Execute([ChunkIndexInQuery] int chunkIndex, ref SpawnerComponent spawnerComponent)
        {
            //Debug.Log("스폰");
            parallelWriter.Instantiate(chunkIndex, spawnerComponent.spawnPrefab);
            spawnerComponent.spawnedCount++;
        }
    }

    [BurstCompile]
    partial struct OnChatPeepoJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        [ReadOnly] public int addValue;
        [ReadOnly] public PeepoConfig peepoConfig;
        public void Execute(ref TimeLimitedLifeComponent timeLimitedLifeComponent, in PeepoComponent peepoComponent)
        {
            //Debug.Log("채팅");
            if (peepoComponent.hashID == hashID)
                timeLimitedLifeComponent.lifeTime = math.clamp(timeLimitedLifeComponent.lifeTime + addValue, 0, peepoConfig.MaxLifeTime + peepoConfig.MaxLifeTime / 2);
        }
    }

    [BurstCompile]
    public partial struct OnDestroyPeepoJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        public EntityCommandBuffer.ParallelWriter parallelWriter;

        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref TimeLimitedLifeComponent timeLimitedLifeComponent, in PeepoComponent peepoComponent)
        {
            if (peepoComponent.hashID == hashID)
            {
                timeLimitedLifeComponent.lifeTime = 0;
            }
        }
    }

    partial struct PeepoInitJob : IJobEntity
    {
        public void Execute(ref PeepoComponent peepoComponent, ref PhysicsVelocity velocity, ref LocalTransform localTransform)
        {
            if (peepoComponent.currentState != PeepoState.Born || GameManager.instance.spawnOrderQueue.Count <= 0) return;
            var spawnOrder = GameManager.instance.spawnOrderQueue.Dequeue();
            peepoComponent.hashID = spawnOrder.hash;
            velocity.Linear = spawnOrder.initForce;
            localTransform.Position = new float3(spawnOrder.spawnPosx, 9, 0);
            localTransform.Scale = 1;
            peepoComponent.currentState = PeepoState.Ragdoll;
        }
    }
    [BurstCompile]
    partial struct PeepoDefaultJob : IJobEntity
    {
        public void Execute(ref PeepoComponent peepoComponent)
        {
            if (peepoComponent.currentState != PeepoState.Born) return;
            peepoComponent.currentState = PeepoState.Ragdoll;
        }
    }
}
