using OSY;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial class PeepoEventSystem : SystemBase
{
    public Action OnSpawn;
    public Action<int, float> OnChat;
    public Action<int, float> OnDonation;
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
        OnChat = (hashID, addValueLife) =>
        {
            if (addValueLife != 0)
                new OnChatPeepoJob { hashID = hashID, addValue = addValueLife, peepoConfig = peepoConfig.Value }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
            for (int i = 0; i < 10; i++)
            {
                new SpawnCheezeJob { hashID = hashID, store = SystemAPI.GetSingleton<EntityStoreComponent>(), parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
            }
        };
        OnDead = (hashID) =>
        {
            new OnDestroyPeepoJob { hashID = hashID }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnCalm = () =>
        {
            new OnCalmPeepoJob().ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnDonation = (hashID, payAmount) =>
        {
            new OnDonationPeepoJob { hashID = hashID, payAmount = (uint)payAmount }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
            int cheezeCount = (int)payAmount / 10;
            for (int i = 0; i < cheezeCount; i++)
            {
                new SpawnCheezeJob { hashID = hashID, store = SystemAPI.GetSingleton<EntityStoreComponent>(), parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
            }
        };
    }

    protected override void OnUpdate()
    {
        if (GameManager.instance.spawnOrderQueue.Count > 0)
            new PeepoInitJob { peepoConfig = peepoConfig.Value, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter(), spawnOrder = GameManager.instance.spawnOrderQueue.Dequeue(), spawnPosition = GameManager.instance.mainCam.ScreenToWorldPoint(Utils.GetRandomPosition_Float2(GameManager.instance.peepoSpawnRect).ToFloat3()) }.ScheduleParallel();
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

        public void Execute([ChunkIndexInQuery] int chunkIndex, in EntityStoreComponent store)
        {
            //Debug.Log("스폰");
            parallelWriter.Instantiate(chunkIndex, store.peepo);
        }
    }
    partial struct PeepoInitJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        [ReadOnly] public GameManager.SpawnOrder spawnOrder;
        [ReadOnly] public float3 spawnPosition;
        [ReadOnly] public PeepoConfig peepoConfig;
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref PeepoComponent peepoComponent, ref PhysicsVelocity velocity, ref LocalTransform localTransform)
        {
            if (peepoComponent.currentState != PeepoState.Born) return;
            peepoComponent.hashID = spawnOrder.hash;
            velocity.Linear = spawnOrder.initForce;
            localTransform.Scale = 0;
            localTransform.Position = spawnPosition;

            parallelWriter.AddComponent(chunkIndex, entity, new TimeLimitedLifeComponent
            {
                lifeTime = peepoConfig.DefalutLifeTime
            });
            parallelWriter.AddComponent(chunkIndex, entity, new DragableTag());
            parallelWriter.AddComponent(chunkIndex, entity, new RandomDataComponent
            {
                Random = new Unity.Mathematics.Random((uint)Utils.GetRandom(uint.MinValue, uint.MaxValue))
            });

            peepoComponent.currentState = PeepoState.Ragdoll;
        }
    }

    [BurstCompile]
    partial struct OnChatPeepoJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        [ReadOnly] public float addValue;
        [ReadOnly] public PeepoConfig peepoConfig;
        public void Execute(ref TimeLimitedLifeComponent timeLimitedLifeComponent, in PeepoComponent peepoComponent)
        {
            //Debug.Log("채팅");
            if (peepoComponent.hashID == hashID)
                timeLimitedLifeComponent.lifeTime = math.clamp(timeLimitedLifeComponent.lifeTime + addValue, 0, peepoConfig.MaxLifeTime);
        }
    }

    [BurstCompile]
    partial struct OnDonationPeepoJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        [ReadOnly] public uint payAmount;
        public void Execute(ref PeepoComponent peepoComponent)
        {
            Debug.Log("후원");
            if (peepoComponent.hashID == hashID)
                peepoComponent.totalDonation += payAmount;
        }
    }
    [BurstCompile]
    partial struct SpawnCheezeJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        [ReadOnly] public EntityStoreComponent store;
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public void Execute([ChunkIndexInQuery] int chunkIndex, in PeepoComponent peepoComponent, in LocalTransform peepoLocalTransform, in RandomDataComponent randomDataComponent)
        {
            if (peepoComponent.hashID == hashID)
            {
                Entity spawnedCheeze = parallelWriter.Instantiate(chunkIndex, store.cheeze);
                var initTransform = new LocalTransform { Position = peepoLocalTransform.Position, Rotation = quaternion.identity, Scale = randomDataComponent.Random.NextFloat(0.5f, 1.2f) };
                var initVelocity = new PhysicsVelocity { Linear = new float3(randomDataComponent.Random.NextFloat(-0.1f, 0.1f), randomDataComponent.Random.NextFloat(0.5f, 1.2f), 0) };
                parallelWriter.SetComponent(chunkIndex, spawnedCheeze, initTransform);
                parallelWriter.SetComponent(chunkIndex, spawnedCheeze, initVelocity);
            }
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
