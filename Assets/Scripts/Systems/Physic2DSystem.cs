using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct Physic2DSystem : ISystem, ISystemStartStop
{
    GameManagerSingleton gameManager;
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        gameManager = SystemAPI.GetSingleton<GameManagerSingleton>();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new Physic2DJob { maxVelocity = gameManager.physicMaxVelocity }.ScheduleParallel();
    }
    [BurstCompile]
    partial struct Physic2DJob : IJobEntity
    {
        [ReadOnly] public float maxVelocity;
        public void Execute(ref PhysicsVelocity velocity, ref LocalTransform localTransform)
        {
            localTransform.Position.z = 0;

            localTransform.Rotation = new Unity.Mathematics.quaternion(0, 0, localTransform.Rotation.value.z, localTransform.Rotation.value.w);

            velocity.Linear.z = 0;
            velocity.Angular.x = 0;
            velocity.Angular.y = 0;

            if (velocity.Linear.x > maxVelocity)
                velocity.Linear.x = maxVelocity;
            else if (velocity.Linear.x < -maxVelocity)
                velocity.Linear.x = -maxVelocity;

            if (velocity.Linear.y > maxVelocity)
                velocity.Linear.y = maxVelocity;
            else if (velocity.Linear.y < -maxVelocity)
                velocity.Linear.y = -maxVelocity;
        }
    }
}
