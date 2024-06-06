using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entityManager = state.EntityManager;
        foreach (var (spawnerComponent, spawnerTransform) in SystemAPI.Query<RefRW<SpawnerComponent>, RefRO<LocalTransform>>())
        {
            if (spawnerComponent.ValueRO.spawnedCount >= spawnerComponent.ValueRO.maxCount) return;
            Entity spawnedEntity = entityManager.Instantiate(spawnerComponent.ValueRO.spawnPrefab);
            var sapwnedEnityTrans = entityManager.GetComponentData<LocalTransform>(spawnedEntity);
            var initTransform = new LocalTransform { Position = spawnerTransform.ValueRO.Position, Rotation = spawnerTransform.ValueRO.Rotation, Scale = sapwnedEnityTrans.Scale };
            entityManager.SetComponentData(spawnedEntity, initTransform);
            spawnerComponent.ValueRW.spawnedCount++;
        }
    }
}
