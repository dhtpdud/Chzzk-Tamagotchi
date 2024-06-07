using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile, UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct Physic2DSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (velocity, localTransform) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRW<LocalTransform>>())
        {
            localTransform.ValueRW.Position.z = 0;

            localTransform.ValueRW.Rotation = new Unity.Mathematics.quaternion(0, 0, localTransform.ValueRO.Rotation.value.z, localTransform.ValueRO.Rotation.value.w);

            velocity.ValueRW.Linear.z = 0;
            velocity.ValueRW.Angular.x = 0;
            velocity.ValueRW.Angular.y = 0;
        }
    }
}
