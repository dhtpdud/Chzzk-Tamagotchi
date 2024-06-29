using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Random = Unity.Mathematics.Random;
using Ray = UnityEngine.Ray;

public struct DragableTag : IComponentData
{

}
public struct MouseRockTag : IComponentData
{

}
public struct DestroyMark : IComponentData
{

}
public enum PeepoState
{
    Born,
    Idle,
    Ragdoll,
    Move,
    Draged,
    Dance
}
public struct TimeLimitedLifeComponent : IComponentData
{
    public float lifeTime;
}
public struct PeepoComponent : IComponentData
{
    public int hashID;
    public int IdleAnimationIndex;
    public PeepoState lastState;
    public PeepoState currentState;

    public float2 lastVelocity;
    public float lastAngularVelocity;

    public float currentImpact;
    public float switchTimerImpact;

    public float switchTimeMove;
    public float moveVelocity;
    public float switchTimerMove;

    /*public uint totalDonation;
    public bool isMute;*/
}
public struct PeepoConfig
{
    public float DefalutLifeTime;
    public float AddLifeTime;
    public float MaxLifeTime;
    public float MinSize;
    public float MaxSize;

    public float switchTimeImpact;
    public float switchIdleAnimationTime;

    public float moveSpeedMin;
    public float moveSpeedMax;
    public float movingTimeMin;
    public float movingTimeMax;
    public float IdlingTimeMin;
    public float IdlingTimeMax;
}
public struct EntityStoreComponent : IComponentData
{
    public readonly Entity peepo;
    public readonly Entity mouseRock;

    public EntityStoreComponent(Entity peepo, Entity mouseRock)
    {
        this.peepo = peepo;
        this.mouseRock = mouseRock;
    }
}
public struct SpawnerComponent : IComponentData
{
    public Entity spawnPrefab;
    public int maxCount;
    public int spawnedCount;

    public float spawnIntervalSec;
    public float currentSec;

    public bool isRandomSize;
    public float minSize;
    public float maxSize;
}
public struct GameManagerSingletonComponent : IComponentData
{
    public struct DragingEntityInfo
    {
        readonly public Entity entity;
        readonly public RigidBody rigidbody;
        readonly public ColliderKey colliderKey;
        readonly public Material material;

        public DragingEntityInfo(Entity entity, RigidBody rigidbody, ColliderKey colliderKey, Material material)
        {
            this.entity = entity;
            this.rigidbody = rigidbody;
            this.colliderKey = colliderKey;
            this.material = material;
        }
    }
    public DragingEntityInfo dragingEntityInfo;

    public Ray ScreenPointToRayOfMainCam;
    public float2 ScreenToWorldPointMainCam;

    public float dragPower;
    public float stabilityPower;

    public float physicMaxVelocity;
    public BlobAssetReference<PeepoConfig> peepoConfig;
}
public struct RandomDataComponent : IComponentData
{
    public Random Random;
}
public struct AnimationSettings : IComponentData
{
    public int IdleHash;
    public int IdleSub1Hash;
    public int IdleSub2Hash;
    public int MoveHash;
    public int RagdollHash;
}
