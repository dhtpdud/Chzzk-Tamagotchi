using Unity.Entities;
using UnityEngine;

public class EntityStoreAuthoring : MonoBehaviour
{
    public GameObject peepo;
    public GameObject mouseRock;
    public class SpawnerAuthoringBaker : Baker<EntityStoreAuthoring>
    {
        public override void Bake(EntityStoreAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new EntityStoreComponent
            {
                peepo = GetEntity(authoring.peepo, TransformUsageFlags.Dynamic),
                mouseRock = GetEntity(authoring.mouseRock, TransformUsageFlags.Dynamic)
            });
        }
    }
}
