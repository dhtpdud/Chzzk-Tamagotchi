using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct UITransformUpdateSystem : ISystem, ISystemStartStop
{
    public JobHandle eventDepedency;
    float2 topRightScreenPoint;
    public void OnStartRunning(ref SystemState state)
    {
        topRightScreenPoint = new float2(Screen.width, Screen.height);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (GameManager.instance.chatBubbleUICanvasTransform.gameObject.activeInHierarchy)
            new UpdateChatBubbleHUDJob { topRightScreenPoint = this.topRightScreenPoint }.ScheduleParallel();
        if (GameManager.instance.nameTagUICanvasTransform.gameObject.activeInHierarchy)
            new UpdateNameTagHUDJob { topRightScreenPoint = this.topRightScreenPoint }.ScheduleParallel(state.Dependency).Complete();
    }
    public partial struct UpdateChatBubbleHUDJob : IJobEntity
    {
        [ReadOnly] public float2 topRightScreenPoint;
        public void Execute(in LocalTransform localTransform, in HashIDComponent hash)
        {
            if (GameManager.instance.viewerInfos != null)
                if (GameManager.instance.viewerInfos.ContainsKey(hash.ID))
                {
                    UnitaskExecute(localTransform.Position, hash.ID);
                }
        }
        public void UnitaskExecute(float3 position, int hashID)
        {
            float2 maxVal = topRightScreenPoint;
            UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                RectTransform bubbleTransform = (RectTransform)GameManager.instance.viewerInfos[hashID]?.chatBubbleObjects?.transform;
                if (bubbleTransform != null)
                {
                    Vector2 targetPosition = GameManager.instance.mainCam.WorldToScreenPoint(position, Camera.MonoOrStereoscopicEye.Mono);
                    targetPosition.y += 80;
                    float MinX = bubbleTransform.rect.width / 2;
                    float MaxX = maxVal.x - bubbleTransform.rect.width / 2;
                    float MinY = 0;
                    float MaxY = maxVal.y - bubbleTransform.rect.height;
                    bubbleTransform.localPosition = (Vector2)math.clamp(targetPosition, new float2(MinX, MinY), new float2(MaxX, MaxY));
                    //bubbleTransform.localPosition = targetPosition;
                }
                //new TransformJob { targetPosition = localTransform.Position }.Schedule(bubbleTransform);
            }, true, GameManager.instance.destroyCancellationToken).Forget();
        }
    }
    public partial struct UpdateNameTagHUDJob : IJobEntity
    {
        [ReadOnly] public float2 topRightScreenPoint;
        public void Execute(in LocalTransform localTransform, in HashIDComponent hash)
        {
            if (GameManager.instance.viewerInfos != null)
                if (GameManager.instance.viewerInfos.ContainsKey(hash.ID))
                {
                    UnitaskExecute(localTransform.Position, hash.ID);
                }
        }
        public void UnitaskExecute(float3 position, int hashID)
        {
            UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                Transform nameTagTransform = GameManager.instance.viewerInfos[hashID]?.nameTagObject?.transform;
                if(nameTagTransform != null)
                {
                    var targetPosition = GameManager.instance.mainCam.WorldToScreenPoint(position, Camera.MonoOrStereoscopicEye.Mono);
                    targetPosition.y -= 15;
                    nameTagTransform.localPosition = targetPosition;
                }
            }, true, GameManager.instance.destroyCancellationToken).Forget();
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
