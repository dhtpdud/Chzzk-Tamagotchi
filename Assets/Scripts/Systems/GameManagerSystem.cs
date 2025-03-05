using Kirurobo;
using OSY;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(Unity.Entities.InitializationSystemGroup))]
public sealed partial class GameManagerInfoSystem : SystemBase
{
    public bool isReady;
    public Camera mainCam;
    public UniWindowController uniWindowController;
    public BlobAssetReference<PeepoConfig> peepoConfigRef;
    public BlobAssetReference<DonationConfig> donationConfigRef;

    [BurstCompile]
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
        ref DonationConfig donationConfig = ref builder.ConstructRoot<DonationConfig>();

        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingletonComponent>().ValueRW;

        peepoConfigRef = builder.CreateBlobAssetReference<PeepoConfig>(Allocator.Persistent);
        donationConfigRef = builder.CreateBlobAssetReference<DonationConfig>(Allocator.Persistent);

        gameManagerRW.peepoConfig = peepoConfigRef;
        gameManagerRW.donationConfig = donationConfigRef;
        isReady = true;

        builder.Dispose();
    }

    [BurstCompile]
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
        ref var donationConfigRW = ref donationConfigRef.Value;

        gameManagerRW.stabilityPower = GameManager.instance.stabilityPower;
        gameManagerRW.dragPower = GameManager.instance.dragPower;
        gameManagerRW.physicMaxVelocity = GameManager.instance.physicMaxVelocity;
        gameManagerRW.gravity = GameManager.instance.gravity;
        gameManagerRW.SpawnMinSpeed = GameManager.instance.SpawnMinSpeed;
        gameManagerRW.SpawnMaxSpeed = GameManager.instance.SpawnMaxSpeed;
        peepoConfigRW.DefalutLifeTime = GameManager.instance.peepoConfig.defalutLifeTime;
        peepoConfigRW.MaxLifeTime = GameManager.instance.peepoConfig.maxLifeTime;
        peepoConfigRW.AddLifeTime = GameManager.instance.peepoConfig.addLifeTime;
        
        peepoConfigRW.DefaultSize = GameManager.instance.peepoConfig.defaultSize;
        peepoConfigRW.MaxSize = GameManager.instance.peepoConfig.maxSize;
        peepoConfigRW.MinSize = GameManager.instance.peepoConfig.minSize;
        peepoConfigRW.DefalutLifeTime = GameManager.instance.peepoConfig.defalutLifeTime;
        peepoConfigRW.MaxLifeTime = GameManager.instance.peepoConfig.maxLifeTime;
        peepoConfigRW.AddLifeTime = GameManager.instance.peepoConfig.addLifeTime;

        peepoConfigRW.switchIdleAnimationTime = GameManager.instance.peepoConfig.switchIdleAnimationTime;
        peepoConfigRW.switchTimeImpact = GameManager.instance.peepoConfig.switchTimeImpact;
        peepoConfigRW.moveSpeedMin = GameManager.instance.peepoConfig.moveSpeedMin;
        peepoConfigRW.moveSpeedMax = GameManager.instance.peepoConfig.moveSpeedMax;
        peepoConfigRW.movingTimeMin = GameManager.instance.peepoConfig.movingTimeMin;
        peepoConfigRW.movingTimeMax = GameManager.instance.peepoConfig.movingTimeMax;
        peepoConfigRW.IdlingTimeMin = GameManager.instance.peepoConfig.IdlingTimeMin;
        peepoConfigRW.IdlingTimeMax = GameManager.instance.peepoConfig.IdlingTimeMax;

        donationConfigRW.objectCountFactor = GameManager.instance.donationConfig.objectCountFactor;
        donationConfigRW.objectLifeTime = GameManager.instance.donationConfig.objectLifeTime;
        donationConfigRW.MinSize = GameManager.instance.donationConfig.minSize;
        donationConfigRW.MaxSize = GameManager.instance.donationConfig.maxSize;
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
        base.OnDestroy();
        peepoConfigRef.Dispose();
        donationConfigRef.Dispose();
    }
}