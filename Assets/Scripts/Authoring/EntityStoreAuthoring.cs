using Unity.Entities;
using UnityEngine;

public class EntityStoreAuthoring : MonoBehaviour
{
    public GameObject peepo;
    public GameObject cheeze;
    public GameObject mouseRock;
    public GameObject boxCollider;
    public class EntityStoreAuthoringBaker : Baker<EntityStoreAuthoring>
    {
        public override void Bake(EntityStoreAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new EntityStoreComponent(
                GetEntity(authoring.peepo, TransformUsageFlags.Dynamic),
                GetEntity(authoring.cheeze, TransformUsageFlags.Dynamic),
                GetEntity(authoring.mouseRock, TransformUsageFlags.Dynamic),
                GetEntity(authoring.boxCollider, TransformUsageFlags.Dynamic)));
        }
    }
}
