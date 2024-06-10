using OSY;
using Unity.Burst;
using Unity.Core;
using Unity.Entities;
using Unity.Jobs;
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
    private GameManagerComponent gameManager;
    private EntityManager entityManager;
    Entity mouseRockEntity;
    TimeData time;
    float2 objectPositionOnDown;

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

    public Vector2 mouseLastPosition;
    public Vector2 mouseVelocity;
    public Vector2 onMouseDragingPosition;
    public Vector2 onMouseDownPosition;

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
        entityManager.SetEnabled(mouseRockEntity, false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        time = SystemAPI.Time;
        gameManager = SystemAPI.GetSingleton<GameManagerComponent>();
        _physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        if (Input.GetMouseButton(0))
        {
            OnMouse();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            var localTransform = entityManager.GetComponentData<LocalTransform>(mouseRockEntity);
            var velocity = entityManager.GetComponentData<PhysicsVelocity>(mouseRockEntity);

            velocity.Linear *= 0;
            localTransform.Position = (Vector3)gameManager.ScreenToWorldPointMainCam;

            entityManager.SetComponentData(mouseRockEntity, localTransform);
            entityManager.SetComponentData(mouseRockEntity, velocity);
            entityManager.SetEnabled(mouseRockEntity, true);
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            var velocity = entityManager.GetComponentData<PhysicsVelocity>(mouseRockEntity);
            var localTransform = entityManager.GetComponentData<LocalTransform>(mouseRockEntity);

            velocity.Linear = Vector3.Lerp(velocity.Linear, Vector3.zero, 20 * time.DeltaTime);
            float2 power = (float2)(gameManager.ScreenToWorldPointMainCam) - new float2(localTransform.Position.x, localTransform.Position.y);
            velocity.Linear += new float3(power.x, power.y, 0) * 300 * time.DeltaTime;

            entityManager.SetComponentData(mouseRockEntity, velocity);
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            entityManager.SetEnabled(mouseRockEntity, false);
        }
    }

    private void OnMouseDown()
    {
        onMouseDownPosition = gameManager.ScreenToWorldPointMainCam;
        float3 rayStart = gameManager.ScreenPointToRayOfMainCam.origin;
        float3 rayEnd = gameManager.ScreenPointToRayOfMainCam.GetPoint(1000f);

        if (Raycast(rayStart, rayEnd, out var raycastHit))
        {
            var hitRigidBody = _physicsWorldSingleton.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex];
            var hitEntity = hitRigidBody.Entity;
            if (entityManager.HasComponent<DragableTag>(hitEntity))
            {
                var hitEntityPosition = entityManager.GetComponentData<LocalTransform>(hitEntity).Position;
                objectPositionOnDown = new float2(hitEntityPosition.x, hitEntityPosition.y);
                isDraging = true;

                Material material = Utils.GetMaterial(hitRigidBody, raycastHit.ColliderKey);
                dragingEntityInfo = new DragingEntityInfo(hitEntity, hitRigidBody, raycastHit.ColliderKey, material);

                material.RestitutionCombinePolicy = Material.CombinePolicy.Minimum;
                Utils.SetMaterial(dragingEntityInfo.rigidbody, material, raycastHit.ColliderKey);
            }
            if (entityManager.HasComponent<PeepoComponent>(hitEntity))
            {
                var peepoComponent = entityManager.GetComponentData<PeepoComponent>(hitEntity);
                peepoComponent.state = PeepoState.Draging;
                entityManager.SetComponentData(dragingEntityInfo.entity, peepoComponent);
            }
        }
    }
    private void OnMouse()
    {
        if (!isDraging) return;
        onMouseDragingPosition = gameManager.ScreenToWorldPointMainCam;

        var velocity = entityManager.GetComponentData<PhysicsVelocity>(dragingEntityInfo.entity);
        var localTransform = entityManager.GetComponentData<LocalTransform>(dragingEntityInfo.entity);

        velocity.Linear = Vector3.Lerp(velocity.Linear, Vector3.zero, gameManager.stabilityPower * time.DeltaTime);
        float2 power = objectPositionOnDown + (float2)(onMouseDragingPosition - onMouseDownPosition) - new float2(localTransform.Position.x, localTransform.Position.y);
        velocity.Linear += new float3(power.x, power.y, 0) * gameManager.dragPower * time.DeltaTime;

        entityManager.SetComponentData(dragingEntityInfo.entity, velocity);
    }
    private void OnMouseUp()
    {
        if (!isDraging) return;
        isDraging = false;
        if (entityManager.HasComponent<PeepoComponent>(dragingEntityInfo.entity))
        {
            var peepoComponent = entityManager.GetComponentData<PeepoComponent>(dragingEntityInfo.entity);
            peepoComponent.state = PeepoState.Ragdoll;
            peepoComponent.switchTime = 0;
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
    public partial struct TaskJob : IJob
    {
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}
