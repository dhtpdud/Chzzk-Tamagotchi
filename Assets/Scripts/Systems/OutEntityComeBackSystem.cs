using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
partial struct OutEntityComeBackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new OutEnityComeBackJob().ScheduleParallel();
    }

    [BurstCompile]
    partial struct OutEnityComeBackJob : IJobEntity
    {
        public void Execute(in DragableTag dragable, ref LocalTransform localTransform, ref PhysicsVelocity velocity)
        {
            if (localTransform.Position.x > 8.5f * 2)
            {
                localTransform.Position.x = 7.5f * 2;
                velocity.Linear.x /= 2;
            }
            else if (localTransform.Position.x < -8.5f * 2)
            {
                localTransform.Position.x = -7.5f * 2;
                velocity.Linear.x /= 2;
            }
            if (localTransform.Position.y > 5f * 2)
            {
                localTransform.Position.y = 4f * 2;
                velocity.Linear.y /= 2;
            }
            else if (localTransform.Position.y < -5f * 2)
            {
                localTransform.Position.y = -4f * 2;
                velocity.Linear.y /= 2;
            }
        }
    }
}
