using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct Physic2DSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new Physic2DJob().ScheduleParallel();
    }
    [BurstCompile]
    partial struct Physic2DJob : IJobEntity
    {
        public void Execute(ref PhysicsVelocity velocity, ref LocalTransform localTransform)
        {
            localTransform.Position.z = 0;

            localTransform.Rotation = new Unity.Mathematics.quaternion(0, 0, localTransform.Rotation.value.z, localTransform.Rotation.value.w);

            velocity.Linear.z = 0;
            velocity.Angular.x = 0;
            velocity.Angular.y = 0;
        }
    }
}
