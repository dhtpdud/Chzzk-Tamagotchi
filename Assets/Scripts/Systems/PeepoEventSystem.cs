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
    public Action<int, int> OnDonation;
    public Action<int, int> onSubscription;
    public Action<int> OnBan;
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
        string StringBonobono = "보노 보노";
        string StringTropicanan = "트로피카난";
        string StringSeokev = "서깨비";
        OnSpawn = () =>
        {
            EntityStoreComponent store = SystemAPI.GetSingleton<EntityStoreComponent>();
            GameManager.SpawnOrder spawnOrder = GameManager.instance.spawnOrderQueue.Dequeue();
            Entity spawnedPeepo = default;
            if (Utils.hashMemory[spawnOrder.hash].Contains(StringBonobono))
            {
                spawnedPeepo = EntityManager.Instantiate(store.bonobono);
            }
            else if (Utils.hashMemory[spawnOrder.hash].Contains(StringTropicanan))
            {
                spawnedPeepo = EntityManager.Instantiate(store.tropicanan);
            }
            else if (Utils.hashMemory[spawnOrder.hash].Contains(StringSeokev))
            {
                spawnedPeepo = EntityManager.Instantiate(store.seokev);
            }
            else
            {
                spawnedPeepo = EntityManager.Instantiate(store.peepo);
            }
            var peepoComponent = EntityManager.GetComponentData<PeepoComponent>(spawnedPeepo);
            var hash = EntityManager.GetComponentData<HashIDComponent>(spawnedPeepo);
            var velocity = EntityManager.GetComponentData<PhysicsVelocity>(spawnedPeepo);
            var localTransform = EntityManager.GetComponentData<LocalTransform>(spawnedPeepo);
            var spawnPosition = Utils.GetRandomPosition_Float2(GameManager.instance.peepoSpawnRect).ToFloat3() * GameManager.instance.rootCanvas.transform.localScale.x;

            peepoComponent.currentState = PeepoState.Ragdoll;
            hash.ID = spawnOrder.hash;
            velocity.Linear = spawnOrder.initForce;
            localTransform.Scale = 0;
            localTransform.Position = spawnPosition;

            EntityManager.AddComponentData(spawnedPeepo, new TimeLimitedLifeComponent
            {
                lifeTime = peepoConfig.Value.DefalutLifeTime
            });
            EntityManager.SetComponentData(spawnedPeepo, peepoComponent);
            EntityManager.SetComponentData(spawnedPeepo, hash);
            EntityManager.SetComponentData(spawnedPeepo, velocity);
            EntityManager.SetComponentData(spawnedPeepo, localTransform);

            //await Utils.YieldCaches.UniTaskYield;
            //new PeepoInitJob { peepoConfig = peepoConfig.Value, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter(), spawnOrder = spawnOrder, spawnPosition = Utils.GetRandomPosition_Float2(GameManager.instance.peepoSpawnRect).ToFloat3() * GameManager.instance.rootCanvas.transform.localScale.x }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnChat = (hashID, addValueLife) =>
        {
            if (addValueLife != 0)
                new OnChatPeepoJob { hashID = hashID, addValue = addValueLife, peepoConfig = peepoConfig.Value }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnBan = (hashID) =>
        {
            new OnBanJob { hashID = hashID }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnCalm = () =>
        {
            new OnCalmPeepoJob().ScheduleParallel(CheckedStateRef.Dependency).Complete();
        };
        OnDonation = async (hashID, payAmount) =>
        {
            int cheezeCount = (int)(payAmount * donationConfig.Value.objectCountFactor);

            if(hashID == -1) // 익명 후원일 경우
            {
                for (int i = 0; i < cheezeCount; i++)
                {
                    new SpawnDonationObjectUnknownJob {topRightScreenPoint = GameManager.instance.mainCam.ScreenToWorldPoint(new float3(Screen.width, Screen.height,0), Camera.MonoOrStereoscopicEye.Mono).ToFloat2(), donationConfig = donationConfig.Value, spawnObject = SystemAPI.GetSingleton<EntityStoreComponent>().cheeze, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
                    await Utils.YieldCaches.UniTaskYield;
                }
            }
            else
            {
                for (int i = 0; i < cheezeCount; i++)
                {
                    new SpawnDonationObjectJob { donationConfig = donationConfig.Value, hashID = hashID, spawnObject = SystemAPI.GetSingleton<EntityStoreComponent>().cheeze, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
                    await Utils.YieldCaches.UniTaskYield;
                }
            }
        };
        onSubscription = async (hashID, subMonth) =>
        {
            for (int i = 0; i < subMonth; i++)
            {
                new SpawnDonationObjectJob { donationConfig = donationConfig.Value, hashID = hashID, spawnObject = SystemAPI.GetSingleton<EntityStoreComponent>().cheeze, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
                new SpawnDonationObjectJob { donationConfig = donationConfig.Value, hashID = hashID, spawnObject = SystemAPI.GetSingleton<EntityStoreComponent>().cheeze, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
                new SpawnDonationObjectJob { donationConfig = donationConfig.Value, hashID = hashID, spawnObject = SystemAPI.GetSingleton<EntityStoreComponent>().cheeze, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
                new SpawnDonationObjectJob { donationConfig = donationConfig.Value, hashID = hashID, spawnObject = SystemAPI.GetSingleton<EntityStoreComponent>().cheeze, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
                new SpawnDonationObjectJob { donationConfig = donationConfig.Value, hashID = hashID, spawnObject = SystemAPI.GetSingleton<EntityStoreComponent>().cheeze, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(CheckedStateRef.Dependency).Complete();
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
    partial struct PeepoInitJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        [ReadOnly] public GameManager.SpawnOrder spawnOrder;
        [ReadOnly] public float3 spawnPosition;
        [ReadOnly] public PeepoConfig peepoConfig;
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref PeepoComponent peepoComponent, ref PhysicsVelocity velocity, ref LocalTransform localTransform, ref HashIDComponent hash)
        {
            if (peepoComponent.currentState != PeepoState.Born) return;
            peepoComponent.currentState = PeepoState.Ragdoll;
            hash.ID = spawnOrder.hash;
            velocity.Linear = spawnOrder.initForce;
            localTransform.Scale = 0;
            localTransform.Position = spawnPosition;

            parallelWriter.AddComponent(chunkIndex, entity, new TimeLimitedLifeComponent
            {
                lifeTime = peepoConfig.DefalutLifeTime
            });
        }
    }

    [BurstCompile]
    partial struct OnChatPeepoJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        [ReadOnly] public float addValue;
        [ReadOnly] public PeepoConfig peepoConfig;
        public void Execute(ref TimeLimitedLifeComponent timeLimitedLifeComponent, in HashIDComponent hash)
        {
            //Debug.Log("채팅");
            if (hash.ID == hashID)
                timeLimitedLifeComponent.lifeTime = math.clamp(timeLimitedLifeComponent.lifeTime + addValue, 0, peepoConfig.MaxLifeTime);
        }
    }

    [BurstCompile]
    partial struct SpawnDonationObjectJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        [ReadOnly] public Entity spawnObject;
        [ReadOnly] public DonationConfig donationConfig;
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public void Execute([ChunkIndexInQuery] int chunkIndex, in LocalTransform peepoLocalTransform, ref RandomDataComponent randomDataComponent, ref PeepoComponent peepoComponent, in HashIDComponent hash)
        {
            if (hash.ID == hashID)
            {
                peepoComponent.currentState = PeepoState.Ragdoll;
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
    partial struct SpawnDonationObjectUnknownJob : IJobEntity
    {
        [ReadOnly] public Entity spawnObject;
        [ReadOnly] public DonationConfig donationConfig;
        [ReadOnly] public float2 topRightScreenPoint;
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public void Execute([ChunkIndexInQuery] int chunkIndex, ref RandomDataComponent randomDataComponent, SpawnerComponent spawner)
        {
            Entity spawnedCheeze = parallelWriter.Instantiate(chunkIndex, spawnObject);
            randomDataComponent.Random = new Unity.Mathematics.Random(randomDataComponent.Random.NextUInt(uint.MinValue, uint.MaxValue));
            var initTransform = new LocalTransform { Position = new float3(randomDataComponent.Random.NextFloat(-topRightScreenPoint.x, topRightScreenPoint.x), topRightScreenPoint.y, 0), Rotation = quaternion.identity, Scale = randomDataComponent.Random.NextFloat(donationConfig.MinSize, donationConfig.MaxSize) };
            var initVelocity = new PhysicsVelocity { Linear = new float3(randomDataComponent.Random.NextFloat(-5f, 5f), 0, 0) };
            parallelWriter.SetComponent(chunkIndex, spawnedCheeze, initTransform);
            parallelWriter.SetComponent(chunkIndex, spawnedCheeze, initVelocity);
            parallelWriter.AddComponent(chunkIndex, spawnedCheeze, new TimeLimitedLifeComponent
            {
                lifeTime = donationConfig.objectLifeTime
            });
        }
    }

    [BurstCompile]
    public partial struct OnBanJob : IJobEntity
    {
        [ReadOnly] public int hashID;
        public EntityCommandBuffer.ParallelWriter parallelWriter;

        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref TimeLimitedLifeComponent timeLimitedLifeComponent, in HashIDComponent hash)
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
