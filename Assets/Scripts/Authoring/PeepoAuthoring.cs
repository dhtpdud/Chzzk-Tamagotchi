using Unity.Entities;
using UnityEngine;

public class PeepoAuthoring : MonoBehaviour
{
    public string userName;
    public int totalDonation;
    public string lastChat;
    public bool isMute;

    public class PeepoBaker : Baker<PeepoAuthoring>
    {
        public override void Bake(PeepoAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PeepoComponent
            {
                totalDonation = authoring.totalDonation,
                isMute = authoring.isMute,
            });
            AddComponent(entity, new DragableTag());
        }
    }
}
