using NSprites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
[UpdateBefore(typeof(SpriteUVAnimationSystem))]
public partial struct AnimationControlSystem : ISystem
{
    [BurstCompile]
    private struct SystemData : IComponentData
    {
        public EntityQuery MovableQuery;
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var systemData = new SystemData();
        var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PeepoComponent>()
            .WithAspect<AnimatorAspect>()
            .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState);
        var movableQuery = state.GetEntityQuery(queryBuilder);
        movableQuery.AddChangedVersionFilter(ComponentType.ReadOnly<PeepoComponent>());
        systemData.MovableQuery = movableQuery;

        _ = state.EntityManager.AddComponentData(state.SystemHandle, systemData);

        queryBuilder.Dispose();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var systemData = SystemAPI.GetComponent<SystemData>(state.SystemHandle);
        if (!SystemAPI.TryGetSingleton<AnimationSettings>(out var animationSettings))
            return;
        var time = SystemAPI.Time.ElapsedTime;

        var animationSwitchJob = new ChangeAnimationJob
        {
            AnimationSettings = animationSettings,
            Time = time
        };
        animationSwitchJob.ScheduleParallelByRef(systemData.MovableQuery);
    }

    [BurstCompile]
    private partial struct ChangeAnimationJob : IJobEntity
    {
        public AnimationSettings AnimationSettings;
        public double Time;

        private void Execute(AnimatorAspect animator, in PeepoComponent peepoComponent)
        {
            switch (peepoComponent.currentState)
            {
                case PeepoState.Idle:
                    switch (peepoComponent.IdleAnimationIndex)
                    {
                        case 0:
                            animator.SetAnimation(AnimationSettings.IdleHash, Time);
                            break;
                        case 1:
                            animator.SetAnimation(AnimationSettings.IdleSub1Hash, Time);
                            break;
                        case 2:
                            animator.SetAnimation(AnimationSettings.IdleSub2Hash, Time);
                            break;
                        default:
                            animator.SetAnimation(AnimationSettings.IdleHash, Time);
                            break;
                    }
                    break;

                case PeepoState.Draged:
                case PeepoState.Ragdoll:
                    animator.SetAnimation(AnimationSettings.RagdollHash, Time);
                    break;
                case PeepoState.Move:
                    animator.SetAnimation(AnimationSettings.MoveHash, Time);
                    break;
            }
        }
    }
}