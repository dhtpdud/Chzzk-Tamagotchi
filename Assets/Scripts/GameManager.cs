using OSY;
using System;
using UnityEngine;
using UnityEngine.Profiling;

public class GameManager : Singleton<GameManager>
{
    public int targetFPS = 0;
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

    public float dragPower;
    public float stabilityPower;
    public float physicMaxVelocity;

    [Serializable]
    public class PeepoConfig
    {
        public float switchTimeImpact;

        public float moveSpeedMin;
        public float moveSpeedMax;
        public float movingTimeMin;
        public float movingTimeMax;
        public float IdlingTimeMin;
        public float IdlingTimeMax;
    }
    public PeepoConfig peepoConfig;

    protected override void Awake()
    {
        base.Awake();
        var initToken = destroyCancellationToken;
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
    }
    private void Update()
    {
        deltaTime = Time.deltaTime;
        targetFrameRate = Application.targetFrameRate;
        captureDeltaTime = Time.captureDeltaTime;
        timeScale = Time.timeScale;
        unscaledDeltaTime = Time.unscaledDeltaTime;
        realTimeScale = deltaTime / unscaledDeltaTime;
    }
}
