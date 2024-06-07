using Cysharp.Threading.Tasks;
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
    struct MouseRock
    {
        public Entity entity;
        public float size;
        public float dragPower;
        public float stabilityPower;
        public PhysicsVelocity velocity;
        public LocalTransform localTransform;

        public MouseRock(Entity entity, PhysicsVelocity velocity, LocalTransform localTransform, float size, float dragPower, float stabilityPower)
        {
            this.entity = entity;
            this.velocity = velocity;
            this.localTransform = localTransform;
            this.size = size;
            this.dragPower = dragPower;
            this.stabilityPower = stabilityPower;
        }
    }
    MouseRock mouseRock;
    TimeData time;
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        entityManager = state.EntityManager;
        var MouseRockEntity = entityManager.Instantiate(SystemAPI.GetSingleton<SpawnerComponent>().spawnPrefab);
        var MouseRockVelocity = entityManager.GetComponentData<PhysicsVelocity>(mouseRock.entity);
        var MouseRockLocalTransform = entityManager.GetComponentData<LocalTransform>(mouseRock.entity);
        mouseRock = new MouseRock(MouseRockEntity, MouseRockVelocity, MouseRockLocalTransform, 1, 500, 5);
        entityManager.SetEnabled(mouseRock.entity, false);
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
            entityManager.SetEnabled(mouseRock.entity, true);
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            mouseRock.velocity.Linear = Vector3.Lerp(mouseRock.velocity.Linear, Vector3.zero, 5 * time.DeltaTime);
            mouseRock.velocity.Linear += ((float3)((Vector3)GameManager.Instance.onMouseDragingPosition) - mouseRock.localTransform.Position) * 200 * time.DeltaTime;

            entityManager.SetComponentData(mouseRock.entity, mouseRock.velocity);
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            entityManager.SetEnabled(mouseRock.entity, false);
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
