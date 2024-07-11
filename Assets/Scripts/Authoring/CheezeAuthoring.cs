using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class CheezeAuthoring : MonoBehaviour
{
    public class CheezeBaker : Baker<CheezeAuthoring>
    {
        public override void Bake(CheezeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PhysicsGravityFactor { Value = 1 });
            AddComponent(entity, new DragableTag());
        }
    }
}
