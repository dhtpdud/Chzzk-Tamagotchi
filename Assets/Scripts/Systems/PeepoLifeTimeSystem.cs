using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct PeepoLifeTimeSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new TimeLimitedPeepoJob { time = SystemAPI.Time, parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel();
    }

    partial struct TimeLimitedPeepoJob : IJobEntity
    {
        [ReadOnly] public TimeData time;
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref TimeLimitedLifeComponent timeLimitedLifeComponent, in PeepoComponent peepoComponent, ref LocalTransform localTransform)
        {
            localTransform.Scale = Mathf.Lerp(localTransform.Scale, timeLimitedLifeComponent.lifeTime / GameManager.instance.peepoConfig.MaxLifeTime, time.DeltaTime);
            timeLimitedLifeComponent.lifeTime -= time.DeltaTime;
            if (timeLimitedLifeComponent.lifeTime <= 0)
            {
                Debug.Log($"»èÁ¦: {peepoComponent.hashID}");
                GameManager.instance.viewerInfos[peepoComponent.hashID].OnDestroy();
                parallelWriter.AddComponent(chunkIndex, entity, new DestroyMark());
            }
        }
    }
}
