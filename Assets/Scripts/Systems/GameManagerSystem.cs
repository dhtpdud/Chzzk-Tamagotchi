
using OSY;
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
        if (!SystemAPI.HasSingleton<GameManagerSingleton>())
            EntityManager.CreateSingleton<GameManagerSingleton>(nameof(MouseInteractionSystem));
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingleton>().ValueRW;
        gameManagerRW.stabilityPower = GameManager.Instance.stabilityPower;
        gameManagerRW.dragPower = GameManager.Instance.dragPower;
        gameManagerRW.physicMaxVelocity = GameManager.Instance.physicMaxVelocity;
    }
    protected override void OnUpdate()
    {
        ref var gameManagerRW = ref SystemAPI.GetSingletonRW<GameManagerSingleton>().ValueRW;
        gameManagerRW.ScreenPointToRayOfMainCam = mainCam.ScreenPointToRay(Input.mousePosition);
        gameManagerRW.ScreenToWorldPointMainCam = mainCam.ScreenToWorldPoint(Input.mousePosition).ToFloat2();
    }
}