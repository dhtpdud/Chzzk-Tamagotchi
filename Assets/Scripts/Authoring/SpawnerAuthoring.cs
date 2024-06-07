using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject spawnPrefab;
    public int totalCount;
    public int interval;
    public bool isRandomSize;
    public float minSize;
    public float maxSize;
    public class SpawnerAuthoringBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            /*var transform = authoring.transform;
            AddComponent(entity, new LocalTransform
            {
                Position = transform.position,
                Rotation = transform.rotation,
                Scale = transform.localScale.x
            });*/
            AddComponent(entity, new SpawnerComponent
            {
                spawnPrefab = GetEntity(authoring.spawnPrefab, TransformUsageFlags.None),
                maxCount = authoring.totalCount,
                spawnIntervalSec = authoring.interval,
                isRandomSize = authoring.isRandomSize,
                minSize = authoring.minSize,
                maxSize = authoring.maxSize
            });
            AddComponent(entity, new RandomDataComponent
            {
                Random = new Unity.Mathematics.Random((uint)Random.Range(int.MinValue, int.MaxValue))
            });
        }
    }
}
