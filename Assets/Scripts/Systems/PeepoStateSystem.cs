using OSY;
using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Collider = Unity.Physics.Collider;

[BurstCompile]
partial struct PeepoStateSystem : ISystem, ISystemStartStop
{
    BlobAssetReference<Collider> onRagdollCollider;
    BlobAssetReference<Collider> onIdleCollider;
    BlobAssetReference<Collider> onDragingCollider;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PeepoComponent>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        //var onRagdollFilter = new CollisionFilter { BelongsTo = 1u, CollidesWith = uint.MaxValue, GroupIndex = 0 };
        var onIdleFilter = new CollisionFilter { BelongsTo = 2u, CollidesWith = ~2u, GroupIndex = 0 };
        onRagdollCollider = SystemAPI.GetComponent<PhysicsCollider>(SystemAPI.GetSingleton<EntityStoreComponent>().peepo).Value.Value.Clone();
        onIdleCollider = onRagdollCollider.Value.Clone();
        onDragingCollider = onRagdollCollider.Value.Clone();
        onDragingCollider.Value.SetRestitution(0);
        onIdleCollider.Value.SetCollisionFilter(onIdleFilter);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new PeepoJob { time = SystemAPI.Time, onIdleCollider = onIdleCollider, onRagdollCollider = onRagdollCollider, onDragingCollider = onDragingCollider }.ScheduleParallel();
    }
    [BurstCompile]
    partial struct PeepoJob : IJobEntity
    {
        [ReadOnly] public TimeData time;
        [ReadOnly] public BlobAssetReference<Collider> onRagdollCollider;
        [ReadOnly] public BlobAssetReference<Collider> onIdleCollider;
        [ReadOnly] public BlobAssetReference<Collider> onDragingCollider;

        public void Execute(ref PeepoComponent peepoComponent, in PhysicsVelocity velocity, ref PhysicsCollider collider, ref LocalTransform localTransform)
        {
            float2 currentVelocity = velocity.Linear.ToFloat2();
            float currentAngularVelocity = velocity.Angular.z;
            /*float LinerImpact = math.lengthsq(peepoComponent.lastVelocity - currentVelocity);
            float AngularImpact = math.abs(peepoComponent.lastAngularVelocity - currentAngularVelocity);*/
            //Debug.Log(LinerImpact + "+" + AngularImpact);
            peepoComponent.currentImpact = (math.lengthsq(peepoComponent.lastVelocity - currentVelocity) + math.abs(peepoComponent.lastAngularVelocity - currentAngularVelocity)) * time.DeltaTime;
            switch (peepoComponent.state)
            {
                case PeepoState.Ragdoll:
                    collider.Value = onRagdollCollider;
                    if (peepoComponent.currentImpact <= 0.2f)
                    {
                        peepoComponent.switchTime += time.DeltaTime;
                        if (peepoComponent.switchTime > 3)
                        {
                            peepoComponent.switchTime = 0;
                            peepoComponent.state = PeepoState.Idle;
                        }
                    }
                    else
                    {
                        peepoComponent.switchTime = 0;
                    }
                    break;

                case PeepoState.Idle:
                    collider.Value = onIdleCollider;
                    if (peepoComponent.currentImpact > 0.05f)
                    {
                        peepoComponent.switchTime += time.DeltaTime;
                        if (peepoComponent.currentImpact > 10f || peepoComponent.switchTime > 1)
                        {
                            peepoComponent.switchTime = 0;
                            peepoComponent.state = PeepoState.Ragdoll;
                        }
                    }
                    else
                    {
                        peepoComponent.switchTime = 0;
                    }
                    localTransform.Rotation = math.nlerp(localTransform.Rotation, quaternion.identity, 10 * time.DeltaTime);
                    break;
                case PeepoState.Draging:
                    collider.Value = onDragingCollider;
                    break;
                case PeepoState.Dance:
                    break;
            }

            peepoComponent.lastVelocity = currentVelocity;
            peepoComponent.lastAngularVelocity = currentAngularVelocity;
        }
    }
}
