using OSY;
using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Material = Unity.Physics.Material;
using RaycastHit = Unity.Physics.RaycastHit;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct MouseInteractionSystem : ISystem, ISystemStartStop
{
    private PhysicsWorldSingleton _physicsWorldSingleton;
    private GameManagerSingleton gameManager;
    private EntityManager entityManager;
    Entity mouseRockEntity;
    TimeData time;
    float2 entityPositionOnDown;

    public bool isDraging;
    struct DragingEntityInfo
    {
        readonly public Entity entity;
        readonly public RigidBody rigidbody;
        readonly public ColliderKey colliderKey;
        readonly public Material material;

        public DragingEntityInfo(Entity entity, RigidBody rigidbody, ColliderKey colliderKey, Material material)
        {
            this.entity = entity;
            this.rigidbody = rigidbody;
            this.colliderKey = colliderKey;
            this.material = material;
        }
    }
    DragingEntityInfo dragingEntityInfo;

    public float2 mouseLastPosition;
    public float2 mouseVelocity;
    public float2 onMouseDownPosition;
    public float lastEntityRotation;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntityStoreComponent>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        entityManager = state.EntityManager;
        mouseRockEntity = entityManager.Instantiate(SystemAPI.GetSingleton<EntityStoreComponent>().mouseRock);
        entityManager.AddComponent<MouseRockTag>(mouseRockEntity);
        entityManager.SetEnabled(mouseRockEntity, false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        time = SystemAPI.Time;
        gameManager = SystemAPI.GetSingleton<GameManagerSingleton>();
        _physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }
        if (Input.GetMouseButton(0))
        {
            OnMouse();
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            var localTransform = entityManager.GetComponentData<LocalTransform>(mouseRockEntity);
            var velocity = entityManager.GetComponentData<PhysicsVelocity>(mouseRockEntity);

            velocity.Linear *= 0;
            localTransform.Position = gameManager.ScreenToWorldPointMainCam.ToFloat3();

            entityManager.SetComponentData(mouseRockEntity, localTransform);
            entityManager.SetComponentData(mouseRockEntity, velocity);
            entityManager.SetEnabled(mouseRockEntity, true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            entityManager.SetEnabled(mouseRockEntity, false);
        }
        new MouseRockJob { screenToWorldPointMainCam = gameManager.ScreenToWorldPointMainCam, maxVelocity = gameManager.physicMaxVelocity, time = time }.ScheduleParallel();
        //ecb.Playback(state.EntityManager);
    }

    private void OnMouseDown()
    {
        onMouseDownPosition = gameManager.ScreenToWorldPointMainCam;
        float3 rayStart = gameManager.ScreenPointToRayOfMainCam.origin;
        float3 rayEnd = gameManager.ScreenPointToRayOfMainCam.GetPoint(1000f);

        if (Raycast(rayStart, rayEnd, out RaycastHit raycastHit))
        {
            RigidBody hitRigidBody = _physicsWorldSingleton.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex];
            Entity hitEntity = hitRigidBody.Entity;
            if (entityManager.HasComponent<DragableTag>(hitEntity))
            {
                LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(hitEntity);
                lastEntityRotation = localTransform.Rotation.value.z;
                entityPositionOnDown = localTransform.Position.ToFloat2();
                isDraging = true;

                Material material = Utils.GetMaterial(hitRigidBody, raycastHit.ColliderKey);
                dragingEntityInfo = new DragingEntityInfo(hitEntity, hitRigidBody, raycastHit.ColliderKey, material);

                material.RestitutionCombinePolicy = Material.CombinePolicy.Minimum;
                Utils.SetMaterial(dragingEntityInfo.rigidbody, material, raycastHit.ColliderKey);
            }
            if (entityManager.HasComponent<PeepoComponent>(hitEntity))
            {
                PeepoComponent peepoComponent = entityManager.GetComponentData<PeepoComponent>(hitEntity);
                peepoComponent.currentState = PeepoState.Draged;
                entityManager.SetComponentData(dragingEntityInfo.entity, peepoComponent);
            }
        }
    }
    private void OnMouse()
    {
        if (!isDraging) return;

        PhysicsVelocity velocity = entityManager.GetComponentData<PhysicsVelocity>(dragingEntityInfo.entity);
        LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(dragingEntityInfo.entity);

        float2 entityPosition = localTransform.Position.ToFloat2();
        float2 entityPositionFromGrabingPoint = entityPosition - entityPositionOnDown;
        float2 mousePositionFromGrabingPoint = gameManager.ScreenToWorldPointMainCam - onMouseDownPosition;
        float2 entitiyToMouse = mousePositionFromGrabingPoint - entityPositionFromGrabingPoint;
        /*float2 mouseToEntity = entityPositionFromGrabingPoint - mousePositionFromGrabingPoint;

        float angularForce = lastEntityRotation - Vector2.Angle(Vector2.up, mouseToEntity);
        velocity.Angular += angularForce * time.DeltaTime;*/

        velocity.Linear = math.lerp(velocity.Linear, float3.zero, gameManager.stabilityPower * time.DeltaTime);
        velocity.Linear += (entitiyToMouse * gameManager.dragPower * time.DeltaTime).ToFloat3();

        entityManager.SetComponentData(dragingEntityInfo.entity, velocity);

        lastEntityRotation = localTransform.Rotation.value.z;
    }
    private void OnMouseUp()
    {
        if (!isDraging) return;
        isDraging = false;
        if (entityManager.HasComponent<PeepoComponent>(dragingEntityInfo.entity))
        {
            PeepoComponent peepoComponent = entityManager.GetComponentData<PeepoComponent>(dragingEntityInfo.entity);
            peepoComponent.currentState = PeepoState.Ragdoll;
            peepoComponent.switchTimerImpact = 0;
            Utils.SetMaterial(dragingEntityInfo.rigidbody, dragingEntityInfo.material, dragingEntityInfo.colliderKey);
            entityManager.SetComponentData(dragingEntityInfo.entity, peepoComponent);
        }
    }
    private bool Raycast(float3 rayStart, float3 rayEnd, out RaycastHit raycastHit)
    {
        var raycastInput = new RaycastInput
        {
            Start = rayStart,
            End = rayEnd,
            Filter = CollisionFilter.Default
        };
        return _physicsWorldSingleton.CastRay(raycastInput, out raycastHit);
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
    }
    [BurstCompile]
    public partial struct MouseRockJob : IJobEntity
    {
        [ReadOnly] public float2 screenToWorldPointMainCam;
        [ReadOnly] public float maxVelocity;
        [ReadOnly] public TimeData time;
        public void Execute(in MouseRockTag mouseRockTag, ref PhysicsVelocity velocity, ref LocalTransform localTransform)
        {
            float2 distance = screenToWorldPointMainCam - localTransform.Position.ToFloat2();
            velocity.Linear += distance.ToFloat3() * 500 * time.DeltaTime;
            velocity.Linear = math.lerp(velocity.Linear, float3.zero, 20 * time.DeltaTime);
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
