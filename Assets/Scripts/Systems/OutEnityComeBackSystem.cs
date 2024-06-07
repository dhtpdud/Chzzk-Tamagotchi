using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile, UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
partial struct OutEnityComeBackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (dragable, localTransform) in SystemAPI.Query<RefRO<DragableComponent>, RefRW<LocalTransform>>())
        {
            LocalTransform localTransformRO = localTransform.ValueRO;
            if (localTransformRO.Position.x > 8.5f)
            {
                localTransform.ValueRW.Position.x = 7.5f;
            }
            else if (localTransformRO.Position.x < -8.5f)
            {
                localTransform.ValueRW.Position.x = -7.5f;
            }
            if (localTransformRO.Position.y > 5f)
            {
                localTransform.ValueRW.Position.y = 4f;
            }
            else if (localTransformRO.Position.y < -5f)
            {
                localTransform.ValueRW.Position.y = -4f;
            }
        }
    }
}
