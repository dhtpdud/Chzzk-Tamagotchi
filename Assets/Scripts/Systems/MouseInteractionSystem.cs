using Unity.Burst;
using Unity.Core;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct MouseInteractionSystem : ISystem, ISystemStartStop
{
    [UpdateBefore(typeof(MouseInteractionSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public sealed partial class UpdateCameraInfoSystem : SystemBase
    {
        public Camera mainCam;
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            mainCam = Camera.main;
            if (!SystemAPI.HasSingleton<GameManagerComponent>())
                EntityManager.CreateSingleton<GameManagerComponent>(nameof(MouseInteractionSystem));
            ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerComponent>().ValueRW;
            gameManagerRW.stabilityPower = GameManager.Instance.stabilityPower;
            gameManagerRW.dragPower = GameManager.Instance.dragPower;
        }
        protected override void OnUpdate()
        {
            ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerComponent>().ValueRW;
            gameManagerRW.ScreenPointToRayOfMainCam = mainCam.ScreenPointToRay(Input.mousePosition);
            gameManagerRW.ScreenToWorldPointMainCam = mainCam.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private PhysicsWorldSingleton _physicsWorldSingleton;
    private GameManagerComponent gameManager;
    private EntityManager entityManager;
    Entity mouseRockEntity;
    TimeData time;
    float2 objectPositionOnDown;

    public bool isDraging;
    public Entity dragingEntity;

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
            entityManager.SetEnabled(mouseRockEntity, true);
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            var localTransform = entityManager.GetComponentData<LocalTransform>(mouseRockEntity);
            localTransform.Position = (Vector3)gameManager.ScreenToWorldPointMainCam;
            entityManager.SetComponentData(mouseRockEntity, localTransform);
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
            var hitEntity = _physicsWorldSingleton.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
            if (entityManager.HasComponent<DragableTag>(hitEntity))
            {
                var hitEntityPosition = entityManager.GetComponentData<LocalTransform>(hitEntity).Position;
                objectPositionOnDown = new float2(hitEntityPosition.x, hitEntityPosition.y);
                dragingEntity = hitEntity;
                isDraging = true;
            }
        }
    }
    private void OnMouse()
    {
        if (!isDraging) return;
        onMouseDragingPosition = gameManager.ScreenToWorldPointMainCam;

        if (entityManager.HasComponent<PeepoComponent>(dragingEntity))
        {
            var peepoComponent = entityManager.GetComponentData<PeepoComponent>(dragingEntity);
            peepoComponent.state = PeepoState.Ragdoll;
            peepoComponent.switchTime = 0;
            entityManager.SetComponentData(dragingEntity, peepoComponent);
        }

        var velocity = entityManager.GetComponentData<PhysicsVelocity>(dragingEntity);
        var localTransform = entityManager.GetComponentData<LocalTransform>(dragingEntity);

        velocity.Linear = Vector3.Lerp(velocity.Linear, Vector3.zero, gameManager.stabilityPower * time.DeltaTime);
        float2 power = objectPositionOnDown + (float2)(onMouseDragingPosition - onMouseDownPosition) - new float2(localTransform.Position.x, localTransform.Position.y);
        velocity.Linear += new float3(power.x, power.y, 0) * gameManager.dragPower * time.DeltaTime;

        entityManager.SetComponentData(dragingEntity, velocity);
    }
    private void OnMouseUp()
    {
        if (!isDraging) return;
        isDraging = false;
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
