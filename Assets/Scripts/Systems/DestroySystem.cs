using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;

public partial struct DestroySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new TimeLimitedJob { parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(), time = SystemAPI.Time }.ScheduleParallel();
        new DestroyJob { parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct DestroyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, DestroyMark mark)
        {
            parallelWriter.DestroyEntity(chunkIndex, entity);
        }
    }
    [BurstCompile]
    [WithNone(typeof(PeepoComponent))]
    partial struct TimeLimitedJob : IJobEntity
    {
        [ReadOnly] public TimeData time;
        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref TimeLimitedLifeComponent timeLimitedLifeComponent)
        {
            timeLimitedLifeComponent.lifeTime -= time.DeltaTime;
            if (timeLimitedLifeComponent.lifeTime <= 0)
                parallelWriter.AddComponent(chunkIndex, entity, new DestroyMark());
        }
    }
}
