using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial class RestrictedColliderUIUpdateSystem : SystemBase
{
    public Entity top;
    public Entity bottom;
    public Entity left;
    public Entity right;
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        UpdateResolution();
    }
    public void UpdateResolution()
    {
        float scaleFactor = GameManager.instance.rootCanvas.transform.localScale.x;
        float2 topRightScreenPoint = new float2(Screen.width, Screen.height) / 2 * scaleFactor;
        float thickness = 10f;

        UpdateColliderEntity(ref top, new float3(0f, topRightScreenPoint.y + 0.5f, 0f), new float3(Screen.width * scaleFactor + 1, 1f, thickness));
        UpdateColliderEntity(ref bottom, new float3(0f, -topRightScreenPoint.y, 0f), new float3(Screen.width * scaleFactor + 1, 1f, thickness));
        UpdateColliderEntity(ref left, new float3(-topRightScreenPoint.x - 0.5f, 0, 0f), new float3(1f, Screen.height * scaleFactor + 1, thickness));
        UpdateColliderEntity(ref right, new float3(topRightScreenPoint.x + 0.5f, 0, 0f), new float3(1f, Screen.height * scaleFactor + 1, thickness));
    }

    public unsafe void UpdateColliderEntity(ref Entity colliderEntity, float3 position, float3 size)
    {
        BlobAssetReference<Unity.Physics.Collider> tempBlobAssetCollider;
        if (!EntityManager.Exists(colliderEntity) || colliderEntity == default)
        {
            colliderEntity = EntityManager.Instantiate(SystemAPI.GetSingletonRW<EntityStoreComponent>().ValueRO.boxCollider);
        }

        tempBlobAssetCollider = EntityManager.GetComponentData<PhysicsCollider>(colliderEntity).Value.Value.Clone();
        tempBlobAssetCollider.Value.SetCollisionFilter(new CollisionFilter { BelongsTo = 1u << 3, CollidesWith = uint.MaxValue, GroupIndex = 0 });

        LocalTransform entityTransform = EntityManager.GetComponentData<LocalTransform>(colliderEntity);
        entityTransform.Position = position;

        Unity.Physics.BoxCollider* collider = (Unity.Physics.BoxCollider*)tempBlobAssetCollider.GetUnsafePtr();

        BoxGeometry geometry = collider->Geometry;

        // Change the dimensions to whatever you like.
        geometry.Size = size;

        /*var bevelRadius = 0.05f;
        // Clamp the bevel radius to less than half the size of the smallest dimension.
        // Otherwise Unity will yell at us.
        geometry.BevelRadius = math.min(math.cmin(geometry.Size) * 0.499f, bevelRadius);*/

        collider->Geometry = geometry;
        EntityManager.SetComponentData(colliderEntity, entityTransform);
        EntityManager.SetComponentData(colliderEntity, new PhysicsCollider { Value = tempBlobAssetCollider });
        if (!EntityManager.HasComponent(colliderEntity, typeof(Static)))
            EntityManager.AddComponent(colliderEntity, typeof(Static));
    }
    protected override void OnUpdate()
    {
    }
}
