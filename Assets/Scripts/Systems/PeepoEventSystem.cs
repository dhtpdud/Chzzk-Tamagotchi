using Cysharp.Threading.Tasks;
using OSY;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial class PeepoEventSystem : SystemBase
{
    public Action OnSpawn;
    public Action<int, float> OnChat;
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

        public void Execute([ChunkIndexInQuery] int chunkIndex, ref SpawnerComponent spawnerComponent)
        {
            //Debug.Log("스폰");
            parallelWriter.Instantiate(chunkIndex, spawnerComponent.spawnPrefab);
            spawnerComponent.spawnedCount++;
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
