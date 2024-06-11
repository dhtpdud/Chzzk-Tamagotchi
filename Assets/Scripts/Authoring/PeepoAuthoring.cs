using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class PeepoAuthoring : MonoBehaviour
{
    public int totalDonation;
    public bool isMute;

    public class PeepoBaker : Baker<PeepoAuthoring>
    {
        public override void Bake(PeepoAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PeepoComponent
            {
                totalDonation = authoring.totalDonation,
                isMute = authoring.isMute
            });
            AddComponent(entity, new DragableTag());
            AddComponent(entity, new RandomDataComponent
            {
                Random = new Unity.Mathematics.Random((uint)Random.Range(int.MinValue, int.MaxValue))
            });

        }
    }
}
