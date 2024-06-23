using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct PeepoInitSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (GameManager.instance.spawnTrigger)
        {
            new SpawnJob { parallelWriter = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel();
            GameManager.instance.spawnTrigger = false;
        }
        new PeepoInitJob().ScheduleParallel();
    }


    [BurstCompile]
    public partial struct SpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter parallelWriter;

        public void Execute([ChunkIndexInQuery] int chunkIndex, ref SpawnerComponent spawnerComponent)
        {
            Entity spawnedEntity = parallelWriter.Instantiate(chunkIndex, spawnerComponent.spawnPrefab);
            spawnerComponent.spawnedCount++;
        }
    }

    partial struct PeepoInitJob : IJobEntity
    {
        public void Execute(ref PeepoComponent peepoComponent, ref PhysicsVelocity velocity, ref LocalTransform localTransform)
        {
            if (peepoComponent.currentState != PeepoState.Born) return;
            var spawnOrder = GameManager.instance.spawnOrderQueue.Dequeue();
            peepoComponent.hashID = spawnOrder.hash;
            velocity.Linear = spawnOrder.initForce;
            localTransform.Position = new float3(spawnOrder.spawnPosx, 9, 0);
            localTransform.Scale = spawnOrder.size;
            peepoComponent.currentState = PeepoState.Ragdoll;
        }
    }
}
