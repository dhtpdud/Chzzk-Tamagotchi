using OSY;
using Unity.Burst;
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
    private EntityManager entityManager;
    RaycastHit raycastHit;
    Entity mouseRockEntity;
    TimeData time;
    float2 entityPositionOnDown;

    public bool isDraging;

    public float2 mouseLastPosition;
    public float2 mouseVelocity;
    public float2 onMouseDownPosition;
    public float lastEntityRotation;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameManagerSingletonComponent>();
        state.RequireForUpdate<EntityStoreComponent>();
        entityManager = state.EntityManager;
    }


    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        mouseRockEntity = entityManager.Instantiate(SystemAPI.GetSingleton<EntityStoreComponent>().mouseRock);
        entityManager.AddComponent<MouseRockTag>(mouseRockEntity);
        entityManager.SetEnabled(mouseRockEntity, false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        time = SystemAPI.Time;
        _physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        if (Input.GetMouseButtonDown(0))
        {
            //RefRW는 매 프레임 마다. 사용할 때 호출해서 사용해야함.
            OnMouseDown(ref SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRW);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp(ref SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRW);
        }
        if (Input.GetMouseButton(0))
        {
            OnMouse(ref SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRW);
        }

        var localTransform = entityManager.GetComponentData<LocalTransform>(mouseRockEntity);
        var velocity = entityManager.GetComponentData<PhysicsVelocity>(mouseRockEntity);
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {

            velocity.Linear *= 0;
            localTransform.Position = SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRO.ScreenToWorldPointMainCam.ToFloat3();

            entityManager.SetComponentData(mouseRockEntity, localTransform);
            entityManager.SetComponentData(mouseRockEntity, velocity);
            entityManager.SetEnabled(mouseRockEntity, true);
        }
        if (!Input.GetKeyDown(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftAlt))
        {
            entityManager.SetEnabled(mouseRockEntity, false);
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            float3 rockToMouse = SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRO.ScreenToWorldPointMainCam.ToFloat3() - localTransform.Position;
            velocity.Linear += rockToMouse * 500 * time.DeltaTime;
            velocity.Linear = math.lerp(velocity.Linear, float3.zero, 20 * time.DeltaTime);
            entityManager.SetComponentData(mouseRockEntity, velocity);
            //왜인지 job을 쓰면 튕겨버림(버그로 추정됨)
        }
    }
    private void OnMouseDown(ref GameManagerSingletonComponent gameManagerRW)
    {
        onMouseDownPosition = gameManagerRW.ScreenToWorldPointMainCam;

        float3 rayStart = gameManagerRW.ScreenPointToRayOfMainCam.origin;
        float3 rayEnd = gameManagerRW.ScreenPointToRayOfMainCam.GetPoint(1000f);
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
                gameManagerRW.dragingEntityInfo = new GameManagerSingletonComponent.DragingEntityInfo(hitEntity, hitRigidBody, raycastHit.ColliderKey, material);

                material.RestitutionCombinePolicy = Material.CombinePolicy.Minimum;
                Utils.SetMaterial(gameManagerRW.dragingEntityInfo.rigidbody, material, raycastHit.ColliderKey);
            }
            if (entityManager.HasComponent<PeepoComponent>(hitEntity))
            {
                PeepoComponent peepoComponent = entityManager.GetComponentData<PeepoComponent>(hitEntity);
                peepoComponent.currentState = PeepoState.Draged;
                entityManager.SetComponentData(gameManagerRW.dragingEntityInfo.entity, peepoComponent);
            }
        }
    }
    private void OnMouse(ref GameManagerSingletonComponent gameManagerRW)
    {
        if (!isDraging) return;
        PhysicsVelocity velocity = entityManager.GetComponentData<PhysicsVelocity>(gameManagerRW.dragingEntityInfo.entity);
        LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(gameManagerRW.dragingEntityInfo.entity);

        float2 entityPosition = localTransform.Position.ToFloat2();
        float2 entityPositionFromGrabingPoint = entityPosition - entityPositionOnDown;
        float2 mousePositionFromGrabingPoint = gameManagerRW.ScreenToWorldPointMainCam - onMouseDownPosition;
        float2 entitiyToMouse = mousePositionFromGrabingPoint - entityPositionFromGrabingPoint;
        /*float2 mouseToEntity = entityPositionFromGrabingPoint - mousePositionFromGrabingPoint;

        float angularForce = lastEntityRotation - Vector2.Angle(Vector2.up, mouseToEntity);
        velocity.Angular += angularForce * time.DeltaTime;*/

        velocity.Linear = math.lerp(velocity.Linear, float3.zero, gameManagerRW.stabilityPower * time.DeltaTime);
        velocity.Linear += (entitiyToMouse * gameManagerRW.dragPower * time.DeltaTime).ToFloat3();

        entityManager.SetComponentData(gameManagerRW.dragingEntityInfo.entity, velocity);

        lastEntityRotation = localTransform.Rotation.value.z;
    }
    private void OnMouseUp(ref GameManagerSingletonComponent gameManagerRW)
    {
        if (!isDraging) return;
        isDraging = false;
        if (entityManager.HasComponent<PeepoComponent>(gameManagerRW.dragingEntityInfo.entity))
        {
            PeepoComponent peepoComponent = entityManager.GetComponentData<PeepoComponent>(gameManagerRW.dragingEntityInfo.entity);
            peepoComponent.currentState = PeepoState.Ragdoll;
            peepoComponent.switchTimerImpact = 0;
            Utils.SetMaterial(gameManagerRW.dragingEntityInfo.rigidbody, gameManagerRW.dragingEntityInfo.material, gameManagerRW.dragingEntityInfo.colliderKey);
            entityManager.SetComponentData(gameManagerRW.dragingEntityInfo.entity, peepoComponent);
        }
        gameManagerRW.dragingEntityInfo = default;
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
}
