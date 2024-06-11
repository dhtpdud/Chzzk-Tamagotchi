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

            var builder = new BlobBuilder(Allocator.Temp);
            ref PeepoConfig peepoConfig = ref builder.ConstructRoot<PeepoConfig>();
            peepoConfig.switchTimeImpact = GameManager.Instance.peepoConfig.switchTimeImpact;
            peepoConfig.moveSpeedMin = GameManager.Instance.peepoConfig.moveSpeedMin;
            peepoConfig.moveSpeedMax = GameManager.Instance.peepoConfig.moveSpeedMax;
            peepoConfig.movingTimeMin = GameManager.Instance.peepoConfig.movingTimeMin;
            peepoConfig.movingTimeMax = GameManager.Instance.peepoConfig.movingTimeMax;
            peepoConfig.IdlingTimeMin = GameManager.Instance.peepoConfig.IdlingTimeMin;
            peepoConfig.IdlingTimeMax = GameManager.Instance.peepoConfig.IdlingTimeMax;

            var result = builder.CreateBlobAssetReference<PeepoConfig>(Allocator.Persistent);
            builder.Dispose();

            AddComponent(entity, new PeepoComponent
            {
                totalDonation = authoring.totalDonation,
                isMute = authoring.isMute,
                config = result
            });
            AddComponent(entity, new DragableTag());
            AddComponent(entity, new RandomDataComponent
            {
                Random = new Unity.Mathematics.Random((uint)Random.Range(int.MinValue, int.MaxValue))
            });

        }
    }
}
