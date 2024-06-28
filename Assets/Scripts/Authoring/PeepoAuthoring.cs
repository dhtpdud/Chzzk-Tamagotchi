using Unity.Entities;
using UnityEngine;

public class PeepoAuthoring : MonoBehaviour
{

    public class PeepoBaker : Baker<PeepoAuthoring>
    {
        public override void Bake(PeepoAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PeepoComponent());
            AddComponent(entity, new TimeLimitedLifeComponent
            {
                lifeTime = 60
            });
            AddComponent(entity, new DragableTag());
            AddComponent(entity, new RandomDataComponent
            {
                Random = new Unity.Mathematics.Random((uint)Random.Range(int.MinValue, int.MaxValue))
            });

        }
    }
}
