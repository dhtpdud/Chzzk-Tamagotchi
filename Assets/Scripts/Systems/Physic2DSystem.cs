using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateBefore(typeof(PhysicsSystemGroup))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct Physic2DSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*foreach (var (velocity, localTransform) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRW<LocalTransform>>())
        {
            localTransform.ValueRW.Position.z = 0;

            localTransform.ValueRW.Rotation = new Unity.Mathematics.quaternion(0, 0, localTransform.ValueRO.Rotation.value.z, localTransform.ValueRO.Rotation.value.w);

            velocity.ValueRW.Linear.z = 0;
            velocity.ValueRW.Angular.x = 0;
            velocity.ValueRW.Angular.y = 0;
        }*/
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
