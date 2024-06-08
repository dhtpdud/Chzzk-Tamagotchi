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
public partial struct MouseInteractionSystem : ISystem, ISystemStartStop
{
    private PhysicsWorldSingleton _physicsWorldSingleton;
    private EntityManager entityManager;
    Entity mouseRockEntity;
    TimeData time;
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        entityManager = state.EntityManager;
        mouseRockEntity = entityManager.Instantiate(SystemAPI.GetSingleton<EntityStoreComponent>().mouseRock);
        entityManager.SetEnabled(mouseRockEntity, false);
    }


    public void OnUpdate(ref SystemState state)
    {
        _physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        time = SystemAPI.Time;
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        if (Input.GetMouseButton(0))
        {
            OnMouseDrag();
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
            localTransform.Position = (Vector3)GameManager.Instance.mouseCurrentPosition;
            entityManager.SetComponentData(mouseRockEntity, localTransform);
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            entityManager.SetEnabled(mouseRockEntity, false);
        }
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
    }

    private void OnMouseDown()
    {
        var ray = GameManager.Instance.mainCam.ScreenPointToRay(Input.mousePosition);
        var rayStart = ray.origin;
        var rayEnd = ray.GetPoint(1000f);

        if (Raycast(rayStart, rayEnd, out var raycastHit))
        {
            var hitEntity = _physicsWorldSingleton.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
            if (entityManager.HasComponent<DragableComponent>(hitEntity))
            {
                GameManager.Instance.dragingEntity = hitEntity;
                GameManager.Instance.isDragging = true;
            }
        }
    }
    private void OnMouseDrag()
    {
        if (!GameManager.Instance.isDragging) return;
        var dragingEntity = GameManager.Instance.dragingEntity;

        var velocity = entityManager.GetComponentData<PhysicsVelocity>(dragingEntity);
        var localTransform = entityManager.GetComponentData<LocalTransform>(dragingEntity);

        velocity.Linear = Vector3.Lerp(velocity.Linear, Vector3.zero, GameManager.Instance.stabilityPower * time.DeltaTime);
        velocity.Linear += ((float3)((Vector3)GameManager.Instance.onMouseDragingPosition) - localTransform.Position) * GameManager.Instance.dragPower * time.DeltaTime;

        entityManager.SetComponentData(dragingEntity, velocity);
    }
    private void OnMouseUp()
    {
        if (!GameManager.Instance.isDragging) return;
        GameManager.Instance.isDragging = false;
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
    public partial struct TaskJob : IJob
    {
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}
