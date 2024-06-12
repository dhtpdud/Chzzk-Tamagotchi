using Unity.Entities;
using UnityEngine;

namespace NSprites
{
    public class AnimationSettingsAuthoring : MonoBehaviour
    {
        private class AnimationSettingsBaker : Baker<AnimationSettingsAuthoring>
        {
            public override void Bake(AnimationSettingsAuthoring authoring)
            {
                AddComponent(GetEntity(TransformUsageFlags.None), new AnimationSettings
                {
                    IdleHash = Animator.StringToHash("Idle"),
                    IdleSub1Hash = Animator.StringToHash("IdleSub1"),
                    IdleSub2Hash = Animator.StringToHash("IdleSub2"),
                    MoveHash = Animator.StringToHash("Move"),
                    RagdollHash = Animator.StringToHash("Ragdoll")
                });
            }
        }
    }
}