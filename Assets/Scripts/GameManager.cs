using Cysharp.Threading.Tasks;
using OSY;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Entities.CodeGeneratedJobForEach;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

public class GameManager : Singleton<GameManager>
{
    public Camera mainCam;
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
        public float switchIdleAnimationTime;

        public float moveSpeedMin;
        public float moveSpeedMax;
        public float movingTimeMin;
        public float movingTimeMax;
        public float IdlingTimeMin;
        public float IdlingTimeMax;
    }
    public PeepoConfig peepoConfig;
    public Dictionary<int, Texture2D> thumbnailsCacheDic = new Dictionary<int, Texture2D>();

    public Transform canvasTransform;

    [Header("GameObject Caches")]
    public GameObject peepo;
    public Transform subscene;
    public GameObject chatBubble;

    public class ChatInfo
    {
        public string id;
        public GameObject bubbleObject;
        public DateTime dateTime;
        public string text;
        public ChatInfo(string text, GameObject bubbleObject)
        {
            id = Utils.GetRandomHexNumber(10);
            dateTime = DateTime.Now;
            this.text = text;
            this.bubbleObject = bubbleObject;
            var bubbleCTD = bubbleObject.GetCancellationTokenOnDestroy();
            UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                var tmp = bubbleObject.GetComponentInChildren<TMP_Text>();
                tmp.text = text;
                await bubbleObject.transform.DoScaleAsync(Vector3.one, 0.5f, Utils.YieldCaches.UniTaskYield);
                /*await UniTask.Delay(TimeSpan.FromSeconds(3));
                var invisible = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a);
                invisible.a = 0;
                await tmp.DoColorAsync(invisible, 1, Utils.YieldCaches.UniTaskYield);
                Destroy(bubbleObject);*/
            }, true, bubbleCTD).Forget();
        }
    }
    public class ViewerInfo
    {
        public string nickName;
        public List<ChatInfo> chatInfos;
        public ViewerInfo(string nickName)
        {
            this.nickName = nickName;
            chatInfos = new List<ChatInfo>();
        }
    }
    //캐싱 변수
    public Dictionary<int, ViewerInfo> viewerInfos = new Dictionary<int, ViewerInfo>();

    public struct SpawnOrder
    {
        public int hash;
        public float spawnPosx;
        public float3 initForce;
        public float size;

        public SpawnOrder(int hash, float3 initForce, float spawnPosx = 0, float size = 1)
        {
            this.hash = hash;
            this.spawnPosx = spawnPosx;
            this.initForce = initForce;
            this.size = size;
        }
    }
    public Queue<SpawnOrder> spawnOrderQueue = new Queue<SpawnOrder>();

    protected override void Awake()
    {
        base.Awake();
        mainCam ??= Camera.main;
        var initToken = destroyCancellationToken;
        var InitInstance = Instance;
        QualitySettings.vSyncCount = 0;
        //QualitySettings.maxQueuedFrames = 4;
        Application.targetFrameRate = targetFPS;
        //Screen.SetResolution(ScreenWidth, ScreenHeight, false);
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
