using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
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
    Ragdoll,
    Idle,
    Draging,
    Dance
}
public struct PeepoComponent : IComponentData
{
    public PeepoState state;

    public float3 lastVelocity;
    public float3 currentVelocity;

    public float lastAngularVelocity;
    public float currentAngularVelocity;


    public float currentImpact;

    public float switchTime;

    public int totalDonation;
    public bool isChatBubble;
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
public struct GameManagerComponent : IComponentData
{
    public Ray ScreenPointToRayOfMainCam;
    public Vector2 ScreenToWorldPointMainCam;

    public float dragPower;
    public float stabilityPower;

    public float physicMaxVelocity;
}
public struct RandomDataComponent : IComponentData
{
    public Random Random;
}
