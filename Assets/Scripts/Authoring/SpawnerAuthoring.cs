using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject spawnPrefab;
    public int totalCount;
    public float intervalSec;
    public int batchCount = 50;
    public Vector3 minPos;
    public Vector3 maxPos;
    public bool isRandomSize;
    public float minSize;
    public float maxSize;
    public class SpawnerAuthoringBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpawnerComponent
            {
                targetEntity = GetEntity(authoring.spawnPrefab, TransformUsageFlags.Dynamic),
                maxCount = authoring.totalCount,
                spawnIntervalSec = authoring.intervalSec,
                isRandomSize = authoring.isRandomSize,
                minPos = authoring.minPos,
                maxPos = authoring.maxPos,
                minSize = authoring.minSize,
                maxSize = authoring.maxSize,
                batchCount = authoring.batchCount
            });
            AddComponent(entity, new RandomDataComponent
            {
                Random = new Unity.Mathematics.Random((uint)Random.Range(int.MinValue, int.MaxValue))
            });
        }
    }
}
