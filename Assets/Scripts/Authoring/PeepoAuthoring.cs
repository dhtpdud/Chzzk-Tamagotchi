using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class PeepoAuthoring : MonoBehaviour
{
    public class PeepoBaker : Baker<PeepoAuthoring>
    {
        public override void Bake(PeepoAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PeepoComponent());
            AddComponent(entity, new PhysicsGravityFactor { Value = 1});
        }
    }
}
