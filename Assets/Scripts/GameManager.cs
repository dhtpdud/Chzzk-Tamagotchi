using Cysharp.Threading.Tasks;
using OSY;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;
using static Unity.Entities.EntitiesJournaling;

public class GameManager : Singleton<GameManager>
{
    public int targetFPS=0;
    [HideInInspector]
    public string EmptyString = "";

    public int ScreenWidth;
    public int ScreenHeight;
    [HideInInspector]
    public Rect ScreenRect;

    //캐싱용 변수
    public float deltaTime { get; private set; }
    public float captureDeltaTime { get; private set; }
    public float unscaledDeltaTime { get; private set; }
    public float targetFrameRate { get; private set; }
    public float timeScale { get; private set; }
    public float realTimeScale { get; private set; }

    public int originVSyncCount { get; private set; }
    public int originTargetFramerate { get; private set; }
    public int origincaptureFramerate { get; private set; }

    public Camera mainCam;

    public bool isMouseDown;
    public bool isMouse;
    public bool isMouseUp;

    public Vector2 onMouseDownPosition;
    public Vector2 mouseCurrentPosition;
    public Vector2 onMouseDragingPosition;


    public Vector2 onMouseDragedPositionLast;
    public Vector2 onMouseDragedPositionCurrent;
    public Vector2 mouseVelocity;
    public float dragPower;
    public float stabilityPower;
    public bool isDragging;

    public Entity dragingEntity;

    protected override void Awake()
    {
        base.Awake();
        var InitInstance = Instance;
        QualitySettings.vSyncCount = 0;
        QualitySettings.maxQueuedFrames = 4;
        Application.targetFrameRate = targetFPS;
        Screen.SetResolution(ScreenWidth, ScreenHeight, false);
        ScreenRect = new Rect(0, 0, ScreenWidth, ScreenHeight);
        Profiler.maxUsedMemory = 2000000000;//2GB
        originTargetFramerate = Application.targetFrameRate;
        origincaptureFramerate = Time.captureFramerate;
        originVSyncCount = QualitySettings.vSyncCount;
        mainCam ??= Camera.main;
        mainCam.enabled = true;
        UniTask.RunOnThreadPool(async () =>
        {
            await UniTask.SwitchToMainThread();
            while (!destroyCancellationToken.IsCancellationRequested)
            {
                if (mainCam != null)
                {
                    mouseCurrentPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
                    if (isMouseDown)
                        onMouseDownPosition = mouseCurrentPosition;
                    if (isMouse)
                        onMouseDragingPosition = mouseCurrentPosition;
                }
                await Utils.YieldCaches.UniTaskYield;
            }
        }, true, destroyCancellationToken).Forget();
        UniTask.RunOnThreadPool(async () =>
        {
            while (!destroyCancellationToken.IsCancellationRequested)
            {
                if (mainCam != null)
                {
                    if (isMouse && !isMouseDown && !isMouseUp)
                    {
                        onMouseDragedPositionCurrent = onMouseDragingPosition;
                        mouseVelocity = onMouseDragedPositionCurrent - onMouseDragedPositionLast;
                        onMouseDragedPositionLast = onMouseDragedPositionCurrent;
                    }
                }
                await Utils.YieldCaches.UniTaskYield;
            }
        }, true, destroyCancellationToken).Forget();
    }
    private void Update()
    {
        deltaTime = Time.deltaTime;
        targetFrameRate = Application.targetFrameRate;
        captureDeltaTime = Time.captureDeltaTime;
        timeScale = Time.timeScale;
        unscaledDeltaTime = Time.unscaledDeltaTime;
        realTimeScale = deltaTime / unscaledDeltaTime;
        isMouseDown = Input.GetMouseButtonDown(0);
        isMouse = Input.GetMouseButton(0);
        isMouseUp = Input.GetMouseButtonUp(0);
    }
    private void OnDestroy()
    {
        if (mainCam != null)
        {
            Destroy(mainCam);
        }
    }
    public void OpenPage(string url)
    {
        Application.OpenURL(url);
    }
    public void ShutDown()
    {
        Application.Quit();
    }
}
