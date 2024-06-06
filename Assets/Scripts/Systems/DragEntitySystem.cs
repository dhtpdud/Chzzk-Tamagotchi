using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[BurstCompile]
public partial struct DragEntitySystem : ISystem, ISystemStartStop
{
    private PhysicsWorldSingleton _physicsWorldSingleton;
    private EntityManager entityManager;

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        entityManager = state.EntityManager;
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
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
            GameManager.Instance.dragingEntity = hitEntity;
            GameManager.Instance.isDragging = true;
        }
    }
    private void OnMouseDrag()
    {
        if (!GameManager.Instance.isDragging) return;
        var dragingEntity = GameManager.Instance.dragingEntity;

        var velocity = entityManager.GetComponentData<PhysicsVelocity>(dragingEntity);
        var localTransform = entityManager.GetComponentData<LocalTransform>(dragingEntity);

        velocity.Linear *= 0;
        velocity.Linear += ((float3)((Vector3)GameManager.Instance.onMouseDragingPosition) - localTransform.Position) * GameManager.Instance.dragPower;

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
}
