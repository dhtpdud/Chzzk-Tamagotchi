using Unity.Burst;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct SpawnerSystem : ISystem
{/*
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // The system make use of an EntityCommandBuffer, therefore, it needs the system handling the entity command buffer to be initialized to run 
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }*/
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (spawnerComponent, randomDataComponent, spawnerTransformComponent) in SystemAPI.Query<RefRW<SpawnerComponent>, RefRW<RandomDataComponent>, RefRO<LocalTransform>>().WithAll<SpawnerComponent>())
        {
            SpawnerComponent spawnerComponentRO = spawnerComponent.ValueRO;
            if (spawnerComponentRO.spawnedCount >= spawnerComponentRO.maxCount) return;
            randomDataComponent.ValueRW.Random = new Random((uint)randomDataComponent.ValueRO.Random.NextInt(int.MinValue, int.MaxValue));
            Entity spawnedEntity = ecb.Instantiate(spawnerComponentRO.spawnPrefab);
            LocalTransform spanwerTransRO = spawnerTransformComponent.ValueRO;
            var initTransform = new LocalTransform { Position = spanwerTransRO.Position, Rotation = spanwerTransRO.Rotation, Scale = spawnerComponentRO.isRandomSize ? randomDataComponent.ValueRO.Random.NextFloat(spawnerComponentRO.minSize, spawnerComponentRO.maxSize) : spanwerTransRO.Scale };
            ecb.SetComponent(spawnedEntity, initTransform);
            spawnerComponent.ValueRW.spawnedCount++;
        }*/
        var commandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
        SpawnweJob job = new SpawnweJob { parallelWriter = commandBuffer.AsParallelWriter() };
        var handle = job.ScheduleParallel(state.Dependency);

        handle.Complete(); //차후 최적화

        commandBuffer.Playback(state.EntityManager);
    }
    [BurstCompile]
    partial struct SpawnweJob : IJobEntity
    {
        public TimeData time;
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public void Execute(ref SpawnerComponent spawnerComponent, ref RandomDataComponent randomDataComponent, in LocalTransform spawnerTransformComponent)
        {
            if (spawnerComponent.spawnedCount >= spawnerComponent.maxCount) return;
            if (spawnerComponent.currentSec < spawnerComponent.spawnIntervalSec)
            {
                spawnerComponent.currentSec += time.DeltaTime;
                return;
            }
            randomDataComponent.Random = new Random((uint)randomDataComponent.Random.NextInt(int.MinValue, int.MaxValue));
            Entity spawnedEntity = parallelWriter.Instantiate(0, spawnerComponent.spawnPrefab);
            var initTransform = new LocalTransform { Position = spawnerTransformComponent.Position, Rotation = spawnerTransformComponent.Rotation, Scale = spawnerComponent.isRandomSize ? randomDataComponent.Random.NextFloat(spawnerComponent.minSize, spawnerComponent.maxSize) : spawnerTransformComponent.Scale };
            parallelWriter.SetComponent(0, spawnedEntity, initTransform);
            spawnerComponent.spawnedCount++;
            spawnerComponent.currentSec = 0;
        }
    }
}
