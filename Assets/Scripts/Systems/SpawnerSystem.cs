using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        new SpawnweJob { time = SystemAPI.Time, parallelWriter = ecb.AsParallelWriter() }.ScheduleParallel();
    }
    [BurstCompile]
    partial struct SpawnweJob : IJobEntity
    {
        [ReadOnly] public TimeData time;
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public void Execute([ChunkIndexInQuery] int chunkIndex, ref SpawnerComponent spawnerComponent, ref RandomDataComponent randomDataComponent, in LocalTransform spawnerTransformComponent)
        {
            if (spawnerComponent.spawnedCount >= spawnerComponent.maxCount) return;
            if (spawnerComponent.currentSec < spawnerComponent.spawnIntervalSec)
            {
                spawnerComponent.currentSec += time.DeltaTime;
                return;
            }
            spawnerComponent.currentSec = 0;
            randomDataComponent.Random = new Random((uint)randomDataComponent.Random.NextInt(int.MinValue, int.MaxValue));
            Entity spawnedEntity = parallelWriter.Instantiate(chunkIndex, spawnerComponent.spawnPrefab);
            var initTransform = new LocalTransform { Position = spawnerTransformComponent.Position, Rotation = spawnerTransformComponent.Rotation, Scale = spawnerComponent.isRandomSize ? randomDataComponent.Random.NextFloat(spawnerComponent.minSize, spawnerComponent.maxSize) : 1 };
            parallelWriter.SetComponent(chunkIndex, spawnedEntity, initTransform);
            spawnerComponent.spawnedCount++;
        }
    }
}
