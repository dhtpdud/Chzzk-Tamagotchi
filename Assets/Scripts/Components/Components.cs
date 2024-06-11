using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using Ray = UnityEngine.Ray;

public struct DragableTag : IComponentData
{

}
public struct MouseRockTag : IComponentData
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
public struct PeepoComponent : IComponentData
{
    public PeepoState lastState;
    public PeepoState currentState;

    public float2 lastVelocity;
    public float lastAngularVelocity;

    public float currentImpact;
    public float switchTimerImpact;

    public BlobAssetReference<PeepoConfig> config;
    public float switchTimeMove;
    public float moveVelocity;
    public float switchTimerMove;

    public int totalDonation;
    public bool isMute;
}
public struct PeepoConfig
{
    public float switchTimeImpact;

    public float moveSpeedMin;
    public float moveSpeedMax;
    public float movingTimeMin;
    public float movingTimeMax;
    public float IdlingTimeMin;
    public float IdlingTimeMax;
}
public struct EntityStoreComponent : IComponentData
{
    public Entity peepo;
    public Entity mouseRock;
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
public struct GameManagerSingleton : IComponentData
{
    public Ray ScreenPointToRayOfMainCam;
    public float2 ScreenToWorldPointMainCam;

    public float dragPower;
    public float stabilityPower;

    public float physicMaxVelocity;
    public NativeList<SpawnerComponent> spawnerInfos;
}
public struct RandomDataComponent : IComponentData
{
    public Random Random;
}
