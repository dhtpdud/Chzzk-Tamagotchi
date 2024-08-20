using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[CreateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[BurstCompile]
partial struct OutEntityComeBackSystem : ISystem, ISystemStartStop
{
    float2 topRightScreenPoint;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameManagerSingletonComponent>();
    }
    public void OnStartRunning(ref SystemState state)
    {
        float scaleFactor = GameManager.instance.rootCanvas.transform.localScale.x;
        topRightScreenPoint = new float2(Screen.width, Screen.height) / 2 * scaleFactor;
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new OutEnityComeBackJob { topRightScreenPoint = this.topRightScreenPoint }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct OutEnityComeBackJob : IJobEntity
    {
        [ReadOnly] public float2 topRightScreenPoint;
        public void Execute(in DragableTag dragable, ref LocalTransform localTransform, ref PhysicsVelocity velocity)
        {
            if (localTransform.Position.x > topRightScreenPoint.x)
            {
                localTransform.Position.x = topRightScreenPoint.x;
                velocity.Linear.x /= 2;
            }
            else if (localTransform.Position.x < -topRightScreenPoint.x)
            {
                localTransform.Position.x = -topRightScreenPoint.x;
                velocity.Linear.x /= 2;
            }
            if (localTransform.Position.y > topRightScreenPoint.y)
            {
                localTransform.Position.y = topRightScreenPoint.y;
                velocity.Linear.y /= 2;
            }
            else if (localTransform.Position.y < -topRightScreenPoint.y + .5f)
            {
                localTransform.Position.y = -topRightScreenPoint.y + .5f;
                velocity.Linear.y /= 2;
            }
        }
    }
}
