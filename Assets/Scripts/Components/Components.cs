using Unity.Entities;
using UnityEngine;

public struct PeepoComponent : IComponentData
{
    public int totalDonation;
    public bool isChatBubble;
}

public struct SpawnerComponent : IComponentData
{
    public Entity spawnPrefab;
    public int maxCount;
    public int spawnedCount;
    public float spawnIntervalSec;
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
