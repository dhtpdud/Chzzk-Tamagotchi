using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct DragableComponent : IComponentData
{

}
public enum PeepoState
{
    Ragdoll,
    Idle,
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
    public bool isRandomSize;
    public float minSize;
    public float maxSize;
}
public struct MouseInfoComponent : IComponentData
{
    public bool isMouseDown;
    public bool isMouse;
    public bool isMouseUp;
    public bool isDragging;

    public Vector2 onMouseDownPosition;
    public Vector2 mouseCurrentPosition;
    public Vector2 onMouseDragingPosition;

    public Vector2 onMouseDragedPositionLast;
    public Vector2 onMouseDragedPositionCurrent;
    public Vector2 mouseVelocity;
    public float dragPower;

    public Entity dragingEntity;
}
public struct RandomDataComponent : IComponentData
{
    public Random Random;
}
