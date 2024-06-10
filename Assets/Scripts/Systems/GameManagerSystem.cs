
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(MouseInteractionSystem))]
[UpdateBefore(typeof(Physic2DSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public sealed partial class UpdateCameraInfoSystem : SystemBase
{
    public Camera mainCam;
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        mainCam = Camera.main;
        if (!SystemAPI.HasSingleton<GameManagerComponent>())
            EntityManager.CreateSingleton<GameManagerComponent>(nameof(MouseInteractionSystem));
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerComponent>().ValueRW;
        gameManagerRW.stabilityPower = GameManager.Instance.stabilityPower;
        gameManagerRW.dragPower = GameManager.Instance.dragPower;
        gameManagerRW.physicMaxVelocity = GameManager.Instance.physicMaxVelocity;
    }
    protected override void OnUpdate()
    {
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerComponent>().ValueRW;
        gameManagerRW.ScreenPointToRayOfMainCam = mainCam.ScreenPointToRay(Input.mousePosition);
        gameManagerRW.ScreenToWorldPointMainCam = mainCam.ScreenToWorldPoint(Input.mousePosition);
    }
}