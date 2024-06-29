using NSprites;
using OSY;
using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Collider = Unity.Physics.Collider;
using Random = Unity.Mathematics.Random;

[BurstCompile]
partial struct PeepoStateSystem : ISystem, ISystemStartStop
{
    BlobAssetReference<Collider> onRagdollCollider;
    BlobAssetReference<Collider> onIdleCollider;
    BlobAssetReference<Collider> onDragingCollider;
    BlobAssetReference<PeepoConfig> peepoConfig;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameManagerSingletonComponent>();
        state.RequireForUpdate<PeepoComponent>();
        state.RequireForUpdate<EntityStoreComponent>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var onIdleFilter = new CollisionFilter { BelongsTo = 2u, CollidesWith = ~2u, GroupIndex = 0 };
        onRagdollCollider = SystemAPI.GetComponent<PhysicsCollider>(SystemAPI.GetSingleton<EntityStoreComponent>().peepo).Value.Value.Clone();
        onIdleCollider = onRagdollCollider.Value.Clone();
        onDragingCollider = onRagdollCollider.Value.Clone();
        onDragingCollider.Value.SetRestitution(0);
        onIdleCollider.Value.SetCollisionFilter(onIdleFilter);
        peepoConfig = SystemAPI.GetSingleton<GameManagerSingletonComponent>().peepoConfig;
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new StateJob
        {
            time = SystemAPI.Time,
            onIdleCollider = onIdleCollider,
            onRagdollCollider = onRagdollCollider,
            onDragingCollider = onDragingCollider,
            peepoConfig = peepoConfig
        }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct StateJob : IJobEntity
    {
        [ReadOnly] public TimeData time;

        [ReadOnly] public BlobAssetReference<Collider> onRagdollCollider;
        [ReadOnly] public BlobAssetReference<Collider> onIdleCollider;
        [ReadOnly] public BlobAssetReference<Collider> onDragingCollider;
        [ReadOnly] public BlobAssetReference<PeepoConfig> peepoConfig;

        public void Execute([ChunkIndexInQuery] int chunkIndex, ref PeepoComponent peepoComponent, ref RandomDataComponent randomDataComponent, ref PhysicsVelocity velocity, ref PhysicsCollider collider, ref LocalTransform localTransform, ref Flip flip)
        {
            float2 currentVelocity = velocity.Linear.ToFloat2();
            float currentAngularVelocity = math.abs(velocity.Angular.z);
            //float LinerImpact = math.lengthsq(peepoComponent.lastVelocity - currentVelocity);
            //Debug.Log(LinerImpact + "+" + AngularImpact);
            peepoComponent.currentImpact = (math.lengthsq(peepoComponent.lastVelocity - currentVelocity) * 2 + currentAngularVelocity * 10) * time.DeltaTime;
            switch (peepoComponent.currentState)
            {
                case PeepoState.Born:
                    peepoComponent.lastState = PeepoState.Born;
                    break;

                case PeepoState.Idle:
                    //init
                    if (peepoComponent.lastState != PeepoState.Idle)
                    {
                        peepoComponent.IdleAnimationIndex = 0;
                        randomDataComponent.Random = new Random((uint)(randomDataComponent.Random.NextInt(int.MinValue, int.MaxValue) + chunkIndex));
                        peepoComponent.switchTimeMove = randomDataComponent.Random.NextFloat(peepoConfig.Value.IdlingTimeMin, peepoConfig.Value.IdlingTimeMax);
                        peepoComponent.switchTimerImpact = 0;
                        peepoComponent.switchTimerMove = 0;
                        collider.Value = onIdleCollider;
                        peepoComponent.lastState = PeepoState.Idle;
                    }
                    //update
                    localTransform.Rotation = math.nlerp(localTransform.Rotation, quaternion.identity, 10 * time.DeltaTime);
                    if (peepoComponent.currentImpact > 0.5f)   //일정 충격량 이상
                    {
                        peepoComponent.switchTimerImpact += time.DeltaTime;
                        if (peepoComponent.currentImpact > 20f || peepoComponent.switchTimerImpact > peepoConfig.Value.switchTimeImpact)
                        {
                            peepoComponent.currentState = PeepoState.Ragdoll;
                        }
                    }
                    else                                        //고요할 때
                    {
                        peepoComponent.switchTimerImpact = 0;
                        if (peepoComponent.switchTimerMove > peepoComponent.switchTimeMove)
                        {
                            peepoComponent.currentState = PeepoState.Move;
                            break;
                        }
                        if (peepoComponent.switchTimerMove > peepoConfig.Value.switchIdleAnimationTime && peepoComponent.IdleAnimationIndex.Equals(0))
                        {
                            if (randomDataComponent.Random.NextInt(0, 3) < 1)
                            {
                                peepoComponent.switchTimerMove = 0;
                                peepoComponent.IdleAnimationIndex = randomDataComponent.Random.NextInt(1, 3);
                            }
                            else
                                peepoComponent.IdleAnimationIndex = 2;
                        }
                        peepoComponent.switchTimerMove += time.DeltaTime;
                    }
                    break;
                case PeepoState.Ragdoll:
                    //init
                    if (peepoComponent.lastState != PeepoState.Ragdoll)
                    {
                        peepoComponent.switchTimerImpact = 0;
                        collider.Value = onRagdollCollider;
                        peepoComponent.lastState = PeepoState.Ragdoll;
                    }
                    //update
                    if (peepoComponent.currentImpact <= 1f)
                    {
                        peepoComponent.switchTimerImpact += time.DeltaTime;
                        if (peepoComponent.switchTimerImpact > 3)
                        {
                            peepoComponent.currentState = PeepoState.Idle;
                        }
                    }
                    else
                    {
                        peepoComponent.switchTimerImpact = 0;
                    }
                    break;
                case PeepoState.Move:
                    //init
                    if (peepoComponent.lastState != PeepoState.Move)
                    {
                        peepoComponent.switchTimerMove = 0;
                        randomDataComponent.Random = new Random((uint)(randomDataComponent.Random.NextInt(int.MinValue, int.MaxValue) + chunkIndex));
                        peepoComponent.switchTimeMove = randomDataComponent.Random.NextFloat(peepoConfig.Value.movingTimeMin, peepoConfig.Value.movingTimeMax);
                        peepoComponent.moveVelocity = randomDataComponent.Random.NextFloat(peepoConfig.Value.moveSpeedMin, peepoConfig.Value.moveSpeedMax);
                        flip.Value.x = peepoComponent.moveVelocity > 0 ? 0 : -1;
                        peepoComponent.lastState = PeepoState.Move;
                    }
                    //update
                    if (peepoComponent.switchTimerMove > peepoComponent.switchTimeMove)
                    {
                        peepoComponent.currentState = PeepoState.Idle;
                        break;
                    }
                    peepoComponent.switchTimerMove += time.DeltaTime;
                    if (currentAngularVelocity < 120)
                        localTransform.Position.x += peepoComponent.moveVelocity * time.DeltaTime;
                    break;
                case PeepoState.Draged:
                    //Init
                    if (peepoComponent.lastState != PeepoState.Draged)
                    {
                        peepoComponent.lastState = PeepoState.Draged;
                        collider.Value = onDragingCollider;
                    }
                    break;
            }

            peepoComponent.lastVelocity = currentVelocity;
        }
    }
}
