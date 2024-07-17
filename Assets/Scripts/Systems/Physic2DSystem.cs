using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

#if false
[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
[RequireMatchingQueriesForUpdate]
public partial struct ConstrainPhysicsTo2D : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ref PhysicsWorldSingleton physics = ref SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW;

        state.Dependency = new ConstrainJob
        {
            Velocity = physics.MotionVelocities,
            Data = physics.MotionDatas
        }.Schedule(state.Dependency);
    }

    [BurstCompile]
    private unsafe struct ConstrainJob : IJob
    {
        public NativeArray<MotionVelocity> Velocity;
        public NativeArray<MotionData> Data;

        public void Execute()
        {
            // * Fixed zero value.
            var zC = 0;

            // * Fixed float2 zero value.
            float2 xyC = float2.zero;

            var vel = (MotionVelocity*)Velocity.GetUnsafePtr();

            // * Shift pointer to access Z variable of linear velocity and zero it out.
            UnsafeUtility.MemCpyStride(&vel->LinearVelocity.z, sizeof(MotionVelocity),
                &zC, 0, sizeof(int), Velocity.Length);

            // * Shift pointer to access XY fields of angular velocity and zero them out.
            UnsafeUtility.MemCpyStride(&vel->AngularVelocity, sizeof(MotionVelocity),
                &xyC, 0, sizeof(float2), Velocity.Length);

            var dat = (MotionData*)Data.GetUnsafePtr();
            //float4 xyQ = new float4(0, 0, dat->BodyFromMotion.rot.value.z, dat->BodyFromMotion.rot.value.w);

            // * Shift pointer to access WorldFromMotion (RigidTransform) and then the Z variable of its position.
            UnsafeUtility.MemCpyStride(&dat->WorldFromMotion.pos.z, sizeof(MotionData),
                &zC, 0, sizeof(int), Data.Length);

            UnsafeUtility.MemCpyStride(&dat->WorldFromMotion.rot.value, sizeof(MotionData),
                &xyC, 0, sizeof(int), Data.Length);
        }
    }
}
# else
[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
//[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
[RequireMatchingQueriesForUpdate]
public partial struct Physic2DSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameManagerSingletonComponent>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        GameManagerSingletonComponent gmComponent = SystemAPI.GetSingleton<GameManagerSingletonComponent>();
        new Physic2DJob { maxVelocity = gmComponent.physicMaxVelocity, gravity = gmComponent.gravity }.ScheduleParallel();
    }
    [BurstCompile]
    partial struct Physic2DJob : IJobEntity
    {
        [ReadOnly] public float maxVelocity;
        [ReadOnly] public float gravity;
        public void Execute(ref PhysicsVelocity velocity, ref LocalTransform localTransform, ref PhysicsGravityFactor gravityFactor)
        {
            gravityFactor.Value = gravity;
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
#endif