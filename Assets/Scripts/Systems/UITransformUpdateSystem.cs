using Cysharp.Threading.Tasks;
using System.Linq;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public partial struct UITransformUpdateSystem : ISystem
{
    public JobHandle eventDepedency;
    public void OnUpdate(ref SystemState state)
    {
        if(GameManager.instance.chatBubbleUICanvasTransform.gameObject.activeInHierarchy)
            new UpdateChatBubbleHUDJob().ScheduleParallel();
        if (GameManager.instance.nameTagUICanvasTransform.gameObject.activeInHierarchy)
            new UpdateNameTagHUDJob().ScheduleParallel(state.Dependency).Complete();
    }
    public partial struct UpdateChatBubbleHUDJob : IJobEntity
    {
        public void Execute(in PeepoComponent peepo, in LocalTransform localTransform)
        {
            if (GameManager.instance.viewerInfos != null)
                if (GameManager.instance.viewerInfos.ContainsKey(peepo.hashID))
                {
                    UnitaskExecute(peepo, localTransform);
                }
        }
        public void UnitaskExecute(PeepoComponent peepo, LocalTransform localTransform)
        {
            UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                if (GameManager.instance.viewerInfos.ContainsKey(peepo.hashID))
                    foreach (var bubbleTransform in GameManager.instance.viewerInfos[peepo.hashID].chatInfos.Where(chat => chat.bubbleObject != null).Select(chat => chat.bubbleObject.GetComponent<RectTransform>()))
                        if (bubbleTransform != null)
                        {
                            var targetPosition = GameManager.instance.mainCam.WorldToScreenPoint(localTransform.Position, Camera.MonoOrStereoscopicEye.Mono);
                            targetPosition.y += 80;
                            bubbleTransform.localPosition = targetPosition;
                        }
                //new TransformJob { targetPosition = localTransform.Position }.Schedule(bubbleTransform);
            }, true, GameManager.instance.destroyCancellationToken).Forget();
        }
    }
    public partial struct UpdateNameTagHUDJob : IJobEntity
    {
        public void Execute(in PeepoComponent peepo, in LocalTransform localTransform)
        {
            if (GameManager.instance.viewerInfos != null)
                if (GameManager.instance.viewerInfos.ContainsKey(peepo.hashID))
                {
                    UnitaskExecute(peepo, localTransform);
                }
        }
        public void UnitaskExecute(PeepoComponent peepo, LocalTransform localTransform)
        {
            UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                var targetPosition = GameManager.instance.mainCam.WorldToScreenPoint(localTransform.Position, Camera.MonoOrStereoscopicEye.Mono);
                targetPosition.y -= 15;
                if (GameManager.instance.viewerInfos.ContainsKey(peepo.hashID) && GameManager.instance.viewerInfos[peepo.hashID].nameTagObject != null)
                    GameManager.instance.viewerInfos[peepo.hashID].nameTagObject.transform.localPosition = targetPosition;
                //new TransformJob { targetPosition = localTransform.Position }.Schedule(bubbleTransform);
            }, true, GameManager.instance.destroyCancellationToken).Forget();
        }
    }
}
