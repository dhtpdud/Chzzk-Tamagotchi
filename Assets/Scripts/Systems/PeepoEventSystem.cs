using Cysharp.Threading.Tasks;
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
    BlobAssetReference<DonationConfig> donationConfig;
    protected override void OnCreate()
    {
        base.OnCreate();
        CheckedStateRef.RequireForUpdate<GameManagerSingletonComponent>();
    }
    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        peepoConfig = SystemAPI.GetSingleton<GameManagerSingletonComponent>().peepoConfig;
        donationConfig = SystemAPI.GetSingleton<GameManagerSingletonComponent>().donationConfig;
        string Bonobono = "보노 보노";
        OnSpawn = async () =>
        {
            var spawnOrder = GameManager.instance.spawnOrderQueue.Dequeue();
            new OnSpawnCharacterJob
            {
                spawnEntity = spawnOrder.hash.Equals(Animator.StringToHash(Bonobono)) ? SystemAPI.GetSingleton<EntityStoreComponent>().bonobono : SystemAPI.GetSingleton<EntityStoreComponent>().peepo,
                parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
            await Utils.YieldCaches.UniTaskYield;
            new PeepoInitJob { peepoConfig = peepoConfig.Value, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter(), spawnOrder = spawnOrder, spawnPosition = Utils.GetRandomPosition_Float2(GameManager.instance.peepoSpawnRect).ToFloat3()*GameManager.instance.rootCanvas.transform.localScale.x }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnChat = (hashID, addValueLife) =>
        {
            if (addValueLife != 0)
                new OnChatPeepoJob { hashID = hashID, addValue = addValueLife, peepoConfig = peepoConfig.Value }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
            /*int cheezeCount = 200;
            for (int i = 0; i < cheezeCount; i++)
            {
                new SpawnCheezeJob { hashID = hashID, store = SystemAPI.GetSingleton<EntityStoreComponent>(), parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
                await Utils.YieldCaches.UniTaskYield;
            }*/
        };
        OnDead = (hashID) =>
        {
            new OnDestroyPeepoJob { hashID = hashID }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnCalm = () =>
        {
            new OnCalmPeepoJob().ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnDonation = async (hashID, payAmount) =>
        {
            new OnDonationPeepoJob { hashID = hashID, payAmount = (uint)payAmount }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
            int cheezeCount = (int)(payAmount * donationConfig.Value.objectCountFactor);
            for (int i = 0; i < cheezeCount; i++)
            {
                new SpawnDonationObjectJob {donationConfig = donationConfig.Value, hashID = hashID, spawnObject = SystemAPI.GetSingleton<EntityStoreComponent>().cheeze, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
                await Utils.YieldCaches.UniTaskYield;
            }
        };
    }

    protected override void OnUpdate()
    {
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
    public partial struct OnSpawnCharacterJob : IJobEntity
    {
        [ReadOnly] public Entity spawnEntity;
        public EntityCommandBuffer.ParallelWriter parallelWriter;

        public void Execute([ChunkIndexInQuery] int chunkIndex, in EntityStoreComponent store)
        {
            //Debug.Log("스폰");
            parallelWriter.Instantiate(chunkIndex, spawnEntity);
        }
    }
    partial struct PeepoInitJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        [ReadOnly] public GameManager.SpawnOrder spawnOrder;
        [ReadOnly] public float3 spawnPosition;
        [ReadOnly] public PeepoConfig peepoConfig;
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref PeepoComponent peepoComponent, ref PhysicsVelocity velocity, ref LocalTransform localTransform, ref HashIDComponent hash)
        {
            if (peepoComponent.currentState != PeepoState.Born) return;
            hash.ID = spawnOrder.hash;
            velocity.Linear = spawnOrder.initForce;
            localTransform.Scale = 0;
            localTransform.Position = spawnPosition;

            parallelWriter.AddComponent(chunkIndex, entity, new TimeLimitedLifeComponent
            {
                lifeTime = peepoConfig.DefalutLifeTime
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
        public void Execute(ref TimeLimitedLifeComponent timeLimitedLifeComponent, in PeepoComponent peepoComponent, in HashIDComponent hash)
        {
            //Debug.Log("채팅");
            if (hash.ID == hashID)
                timeLimitedLifeComponent.lifeTime = math.clamp(timeLimitedLifeComponent.lifeTime + addValue, 0, peepoConfig.MaxLifeTime);
        }
    }

    [BurstCompile]
    partial struct OnDonationPeepoJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        [ReadOnly] public uint payAmount;
        public void Execute(ref PeepoComponent peepoComponent, in HashIDComponent hash)
        {
            Debug.Log("후원");
            if (hash.ID == hashID)
                peepoComponent.totalDonation += payAmount;
        }
    }
    [BurstCompile]
    partial struct SpawnDonationObjectJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        [ReadOnly] public Entity spawnObject;
        [ReadOnly] public DonationConfig donationConfig;
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public void Execute([ChunkIndexInQuery] int chunkIndex, in PeepoComponent peepoComponent, in LocalTransform peepoLocalTransform, ref RandomDataComponent randomDataComponent, in HashIDComponent hash)
        {
            if (hash.ID == hashID)
            {
                Entity spawnedCheeze = parallelWriter.Instantiate(chunkIndex, spawnObject);
                randomDataComponent.Random = new Unity.Mathematics.Random(randomDataComponent.Random.NextUInt(uint.MinValue, uint.MaxValue));
                var initTransform = new LocalTransform { Position = peepoLocalTransform.Position, Rotation = quaternion.identity, Scale = randomDataComponent.Random.NextFloat(donationConfig.MinSize, donationConfig.MaxSize) };
                var initVelocity = new PhysicsVelocity { Linear = new float3(randomDataComponent.Random.NextFloat(-5f, 5f), 0, 0) };
                parallelWriter.SetComponent(chunkIndex, spawnedCheeze, initTransform);
                parallelWriter.SetComponent(chunkIndex, spawnedCheeze, initVelocity);
                parallelWriter.AddComponent(chunkIndex, spawnedCheeze, new TimeLimitedLifeComponent
                {
                    lifeTime = donationConfig.objectLifeTime
                });
            }
        }
    }

    [BurstCompile]
    public partial struct OnDestroyPeepoJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        public EntityCommandBuffer.ParallelWriter parallelWriter;

        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref TimeLimitedLifeComponent timeLimitedLifeComponent, in PeepoComponent peepoComponent, in HashIDComponent hash)
        {
            if (hash.ID == hashID)
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
