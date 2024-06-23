using Cysharp.Threading.Tasks;
using OSY;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using static GameManager;

public sealed partial class UpdateGameManagerInfoSystem : SystemBase
{
    public Camera mainCam;
    public BlobAssetReference<PeepoConfig> peepoConfigRef;
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        mainCam = Camera.main;
        if (!SystemAPI.HasSingleton<GameManagerSingleton>())
            EntityManager.CreateSingleton<GameManagerSingleton>();
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingleton>().ValueRW;
        gameManagerRW.stabilityPower = GameManager.Instance.stabilityPower;
        gameManagerRW.dragPower = GameManager.Instance.dragPower;
        gameManagerRW.physicMaxVelocity = GameManager.Instance.physicMaxVelocity;
        //Debug.Log("최대 속도는?: " + GameManager.Instance.physicMaxVelocity + "/" + gameManagerRW.physicMaxVelocity);


        var builder = new BlobBuilder(Allocator.TempJob);

        ref PeepoConfig peepoConfig = ref builder.ConstructRoot<PeepoConfig>();
        peepoConfig.switchIdleAnimationTime = GameManager.Instance.peepoConfig.switchIdleAnimationTime;
        peepoConfig.switchTimeImpact = GameManager.Instance.peepoConfig.switchTimeImpact;
        peepoConfig.moveSpeedMin = GameManager.Instance.peepoConfig.moveSpeedMin;
        peepoConfig.moveSpeedMax = GameManager.Instance.peepoConfig.moveSpeedMax;
        peepoConfig.movingTimeMin = GameManager.Instance.peepoConfig.movingTimeMin;
        peepoConfig.movingTimeMax = GameManager.Instance.peepoConfig.movingTimeMax;
        peepoConfig.IdlingTimeMin = GameManager.Instance.peepoConfig.IdlingTimeMin;
        peepoConfig.IdlingTimeMax = GameManager.Instance.peepoConfig.IdlingTimeMax;

        peepoConfigRef = builder.CreateBlobAssetReference<PeepoConfig>(Allocator.Persistent);

        gameManagerRW.peepoConfig = peepoConfigRef;

        builder.Dispose();
    }
    protected override void OnUpdate()
    {
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingleton>().ValueRW;
        gameManagerRW.ScreenPointToRayOfMainCam = mainCam.ScreenPointToRay(Input.mousePosition);
        gameManagerRW.ScreenToWorldPointMainCam = mainCam.ScreenToWorldPoint(Input.mousePosition).ToFloat2();
        if (GameManager.Instance.spawnOrderQueue.Count > 0)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(CheckedStateRef.WorldUnmanaged);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();

            new SpawnJob { parallelWriter = parallelWriter}.ScheduleParallel(CheckedStateRef.Dependency).Complete();
        }
        if (GameManager.Instance.viewerInfos != null)
        {
            new UpdateUIJob().ScheduleParallel();
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        peepoConfigRef.Dispose();
    }

    [BurstCompile]
    public partial struct SpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter parallelWriter;

        public void Execute([ChunkIndexInQuery] int chunkIndex, ref SpawnerComponent spawnerComponent)
        {
            Entity spawnedEntity = parallelWriter.Instantiate(chunkIndex, spawnerComponent.spawnPrefab);
            spawnerComponent.spawnedCount++;
        }
    }

    public partial struct UpdateUIJob : IJobEntity
    {
        public void Execute(in PeepoComponent peepo, in LocalTransform localTransform)
        {
            lock (GameManager.Instance.viewerInfos)
                if (GameManager.Instance.viewerInfos.ContainsKey(peepo.hashID))
                {
                    UnitaskExecute(peepo, localTransform);
                }
        }
        public void UnitaskExecute(PeepoComponent peepo, LocalTransform localTransform)
        {
            UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                foreach (var bubbleTransform in GameManager.Instance.viewerInfos[peepo.hashID].chatInfos.Where(chat => chat.bubbleObject != null).Select(chat => chat.bubbleObject.GetComponent<RectTransform>()))
                    if (bubbleTransform != null)
                    {
                        var targetPosition = GameManager.Instance.mainCam.WorldToScreenPoint(localTransform.Position, Camera.MonoOrStereoscopicEye.Mono);
                        bubbleTransform.localPosition = targetPosition;
                    }
                //new TransformJob { targetPosition = localTransform.Position }.Schedule(bubbleTransform);
            }, true, GameManager.Instance.destroyCancellationToken).Forget();
        }
    }
}