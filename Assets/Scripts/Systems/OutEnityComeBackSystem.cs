using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile, UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
partial struct OutEnityComeBackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*foreach (var (dragable, localTransform) in SystemAPI.Query<RefRO<DragableComponent>, RefRW<LocalTransform>>())
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
        }*/
        new OutEnityComeBackJob().ScheduleParallel();
    }

    [BurstCompile]
    partial struct OutEnityComeBackJob : IJobEntity
    {
        public void Execute(in DragableTag dragable, ref LocalTransform localTransform)
        {
            if (localTransform.Position.x > 8.5f * 2)
            {
                localTransform.Position.x = 7.5f * 2;
            }
            else if (localTransform.Position.x < -8.5f * 2)
            {
                localTransform.Position.x = -7.5f * 2;
            }
            if (localTransform.Position.y > 5f * 2)
            {
                localTransform.Position.y = 4f * 2;
            }
            else if (localTransform.Position.y < -5f * 2)
            {
                localTransform.Position.y = -4f * 2;
            }
        }
    }
}
