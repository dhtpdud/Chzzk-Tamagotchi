using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile, UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct SpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entityManager = state.EntityManager;
        foreach (var (spawnerComponent, spawnerTransformComponent, randomDataComponent) in SystemAPI.Query<RefRW<SpawnerComponent>, RefRO<LocalTransform>, RefRW<RandomDataComponent>>())
        {
            randomDataComponent.ValueRW.Random = new Random((uint)randomDataComponent.ValueRO.Random.NextInt(int.MinValue, int.MaxValue));
            SpawnerComponent spawnerComponentRO = spawnerComponent.ValueRO;
            if (spawnerComponentRO.spawnedCount >= spawnerComponentRO.maxCount) return;
            Entity spawnedEntity = entityManager.Instantiate(spawnerComponentRO.spawnPrefab);
            LocalTransform spanwerTransRO = spawnerTransformComponent.ValueRO;
            var initTransform = new LocalTransform { Position = spanwerTransRO.Position, Rotation = spanwerTransRO.Rotation, Scale = spawnerComponentRO.isRandomSize ? randomDataComponent.ValueRO.Random.NextFloat(spawnerComponentRO.minSize, spawnerComponentRO.maxSize) : spanwerTransRO.Scale };
            entityManager.SetComponentData(spawnedEntity, initTransform);
            spawnerComponent.ValueRW.spawnedCount++;
        }
    }
}
