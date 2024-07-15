using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial class RestrictedColliderUIUpdateSystem : SystemBase
{
    protected override void OnUpdate()
    {
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
    }
}
