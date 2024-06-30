using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct PeepoLifeTimeSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameManagerSingletonComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new TimeLimitedPeepoJob { time = SystemAPI.Time, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(), gameManager = SystemAPI.GetSingleton<GameManagerSingletonComponent>() }.ScheduleParallel();
    }

    partial struct TimeLimitedPeepoJob : IJobEntity
    {
        [ReadOnly] public TimeData time;
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        [ReadOnly] public GameManagerSingletonComponent gameManager;
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref TimeLimitedLifeComponent timeLimitedLifeComponent, in PeepoComponent peepoComponent, ref LocalTransform localTransform)
        {
            PeepoConfig peepoConfig = gameManager.peepoConfig.Value;
            localTransform.Scale = math.clamp(math.lerp(localTransform.Scale, timeLimitedLifeComponent.lifeTime / peepoConfig.DefalutLifeTime * peepoConfig.DefaultSize, time.DeltaTime), peepoConfig.MinSize, peepoConfig.MaxSize);
            timeLimitedLifeComponent.lifeTime -= time.DeltaTime;
            if (timeLimitedLifeComponent.lifeTime <= 0 && (gameManager.dragingEntityInfo.entity != entity))
            {
                //Debug.Log($"»èÁ¦: {peepoComponent.hashID}");
                GameManager.instance.viewerInfos[peepoComponent.hashID].OnDestroy();
                parallelWriter.AddComponent(chunkIndex, entity, new DestroyMark());
            }
        }
    }
}
