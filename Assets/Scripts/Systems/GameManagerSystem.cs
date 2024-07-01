using Kirurobo;
using OSY;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(InitializationSystemGroup))]
public sealed partial class GameManagerInfoSystem : SystemBase
{
    public Camera mainCam;
    public UniWindowController uniWindowController;
    public BlobAssetReference<PeepoConfig> peepoConfigRef;
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        mainCam = Camera.main;
        uniWindowController = mainCam.GetComponent<UniWindowController>();
        if (!SystemAPI.HasSingleton<GameManagerSingletonComponent>())
            EntityManager.CreateSingleton<GameManagerSingletonComponent>();
        //Debug.Log("최대 속도는?: " + GameManager.Instance.physicMaxVelocity + "/" + gameManagerRW.physicMaxVelocity);


        var builder = new BlobBuilder(Allocator.TempJob);

        ref PeepoConfig peepoConfig = ref builder.ConstructRoot<PeepoConfig>();
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRW;

        peepoConfigRef = builder.CreateBlobAssetReference<PeepoConfig>(Allocator.Persistent);

        gameManagerRW.peepoConfig = peepoConfigRef;
        UpdateSetting();

        builder.Dispose();
    }
    protected override void OnUpdate()
    {
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRW;
        gameManagerRW.ScreenPointToRayOfMainCam = mainCam.ScreenPointToRay(Input.mousePosition);
        gameManagerRW.ScreenToWorldPointMainCam = mainCam.ScreenToWorldPoint(Input.mousePosition).ToFloat2();
    }
    public void UpdateSetting()
    {
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRW;
        ref var peepoConfigRW = ref peepoConfigRef.Value;
        gameManagerRW.stabilityPower = GameManager.instance.stabilityPower;
        gameManagerRW.dragPower = GameManager.instance.dragPower;
        gameManagerRW.physicMaxVelocity = GameManager.instance.physicMaxVelocity;
        gameManagerRW.gravity = GameManager.instance.gravity;
        gameManagerRW.SpawnMinSpeed = GameManager.instance.SpawnMinSpeed;
        gameManagerRW.SpawnMaxSpeed = GameManager.instance.SpawnMaxSpeed;
        peepoConfigRW.DefalutLifeTime = GameManager.instance.peepoConfig.DefalutLifeTime;
        peepoConfigRW.MaxLifeTime = GameManager.instance.peepoConfig.MaxLifeTime;
        peepoConfigRW.AddLifeTime = GameManager.instance.peepoConfig.AddLifeTime;
        
        peepoConfigRW.DefaultSize = GameManager.instance.peepoConfig.DefaultSize;
        peepoConfigRW.MaxSize = GameManager.instance.peepoConfig.MaxSize;
        peepoConfigRW.MinSize = GameManager.instance.peepoConfig.MinSize;
        peepoConfigRW.DefalutLifeTime = GameManager.instance.peepoConfig.DefalutLifeTime;
        peepoConfigRW.MaxLifeTime = GameManager.instance.peepoConfig.MaxLifeTime;
        peepoConfigRW.AddLifeTime = GameManager.instance.peepoConfig.AddLifeTime;

        peepoConfigRW.DefaultSize = GameManager.instance.peepoConfig.DefaultSize;
        peepoConfigRW.MaxSize = GameManager.instance.peepoConfig.MaxSize;
        peepoConfigRW.MinSize = GameManager.instance.peepoConfig.MinSize;

        peepoConfigRW.switchIdleAnimationTime = GameManager.instance.peepoConfig.switchIdleAnimationTime;
        peepoConfigRW.switchTimeImpact = GameManager.instance.peepoConfig.switchTimeImpact;
        peepoConfigRW.moveSpeedMin = GameManager.instance.peepoConfig.moveSpeedMin;
        peepoConfigRW.moveSpeedMax = GameManager.instance.peepoConfig.moveSpeedMax;
        peepoConfigRW.movingTimeMin = GameManager.instance.peepoConfig.movingTimeMin;
        peepoConfigRW.movingTimeMax = GameManager.instance.peepoConfig.movingTimeMax;
        peepoConfigRW.IdlingTimeMin = GameManager.instance.peepoConfig.IdlingTimeMin;
        peepoConfigRW.IdlingTimeMax = GameManager.instance.peepoConfig.IdlingTimeMax;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        peepoConfigRef.Dispose();
    }
}