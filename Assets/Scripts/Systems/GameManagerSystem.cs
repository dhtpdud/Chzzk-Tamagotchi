using OSY;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
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
    }
    protected override void OnDestroy()
    {
        base.OnDestroy(); 
        peepoConfigRef.Dispose();
    }
}