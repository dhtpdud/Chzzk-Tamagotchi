using Kirurobo;
using OSY;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(InitializationSystemGroup))]
public sealed partial class UpdateGameManagerInfoSystem : SystemBase
{
    public Camera mainCam;
    public UniWindowController uniWindowController;
    public BlobAssetReference<PeepoConfig> peepoConfigRef;
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        mainCam = Camera.main;
        uniWindowController= mainCam.GetComponent<UniWindowController>();
        if (!SystemAPI.HasSingleton<GameManagerSingletonComponent>())
            EntityManager.CreateSingleton<GameManagerSingletonComponent>();
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRW;
        gameManagerRW.stabilityPower = GameManager.instance.stabilityPower;
        gameManagerRW.dragPower = GameManager.instance.dragPower;
        gameManagerRW.physicMaxVelocity = GameManager.instance.physicMaxVelocity;
        //Debug.Log("최대 속도는?: " + GameManager.Instance.physicMaxVelocity + "/" + gameManagerRW.physicMaxVelocity);


        var builder = new BlobBuilder(Allocator.TempJob);

        ref PeepoConfig peepoConfig = ref builder.ConstructRoot<PeepoConfig>();
        peepoConfig.MaxLifeTime = GameManager.instance.peepoConfig.MaxLifeTime;
        peepoConfig.DefalutLifeTime = GameManager.instance.peepoConfig.DefalutLifeTime;
        peepoConfig.MaxSize = GameManager.instance.peepoConfig.MaxSize;
        peepoConfig.MinSize = GameManager.instance.peepoConfig.MinSize;
        peepoConfig.switchIdleAnimationTime = GameManager.instance.peepoConfig.switchIdleAnimationTime;
        peepoConfig.switchTimeImpact = GameManager.instance.peepoConfig.switchTimeImpact;
        peepoConfig.moveSpeedMin = GameManager.instance.peepoConfig.moveSpeedMin;
        peepoConfig.moveSpeedMax = GameManager.instance.peepoConfig.moveSpeedMax;
        peepoConfig.movingTimeMin = GameManager.instance.peepoConfig.movingTimeMin;
        peepoConfig.movingTimeMax = GameManager.instance.peepoConfig.movingTimeMax;
        peepoConfig.IdlingTimeMin = GameManager.instance.peepoConfig.IdlingTimeMin;
        peepoConfig.IdlingTimeMax = GameManager.instance.peepoConfig.IdlingTimeMax;

        peepoConfigRef = builder.CreateBlobAssetReference<PeepoConfig>(Allocator.Persistent);

        gameManagerRW.peepoConfig = peepoConfigRef;

        builder.Dispose();
    }
    protected override void OnUpdate()
    {
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRW;
        gameManagerRW.ScreenPointToRayOfMainCam = mainCam.ScreenPointToRay(Input.mousePosition);
        gameManagerRW.ScreenToWorldPointMainCam = mainCam.ScreenToWorldPoint(Input.mousePosition).ToFloat2();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        peepoConfigRef.Dispose();
    }
}