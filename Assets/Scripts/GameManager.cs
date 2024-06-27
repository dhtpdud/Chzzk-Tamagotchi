using Cysharp.Threading.Tasks;
using OSY;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Camera mainCam;
    public int targetFPS = 0;
    [HideInInspector]
    public string EmptyString = "";

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

    [Header("UI")]
    public GameObject settingUI;
    public GameObject channelInfoUI;
    public TMP_Text channelViewerCount;

    [Header("GameObject Caches")]
    public GameObject peepo;
    public Transform subscene;
    public GameObject chatBubble;
    public GameObject nameTag;

    public class ChatInfo
    {
        public string id;
        public GameObject bubbleObject;
        public DateTime dateTime;
        public string text;
        public ChatInfo(string text)
        {
            id = Utils.GetRandomHexNumber(10);
            dateTime = DateTime.Now;
            this.text = text;
            this.bubbleObject = Instantiate(instance.chatBubble, instance.canvasTransform);
            var bubbleCTD = bubbleObject.GetCancellationTokenOnDestroy();
            UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                var tmp = bubbleObject.GetComponentInChildren<TMP_Text>();
                tmp.text = text;
                await bubbleObject.transform.DoScaleAsync(Vector3.zero, Vector3.one, 0.5f, Utils.YieldCaches.UniTaskYield);
                await UniTask.Delay(TimeSpan.FromSeconds(3));
                var invisible = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a);
                invisible.a = 0;
                await tmp.DoColorAsync(invisible, 1, Utils.YieldCaches.UniTaskYield);
                Destroy(bubbleObject);
            }, true, bubbleCTD).Forget();
        }
    }
    public class ViewerInfo
    {
        public string nickName;
        public List<ChatInfo> chatInfos;
        public GameObject nameTagObject;
        public ViewerInfo(string nickName, Color nicknameColor)
        {
            this.nickName = nickName;
            chatInfos = new List<ChatInfo>();
            this.nameTagObject = Instantiate(instance.nameTag, instance.canvasTransform);
            var tmp = nameTagObject.GetComponentInChildren<TMP_Text>();
            tmp.text = nickName;
            //Debug.Log(nicknameColor.ToHexString());
            tmp.color = nicknameColor;
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
    public bool spawnTrigger;

    protected void Awake()
    {
        instance = this;
        mainCam ??= Camera.main;
        originTargetFramerate = Application.targetFrameRate;
        origincaptureFramerate = Time.captureFramerate;
        originVSyncCount = QualitySettings.vSyncCount;
    }
    private void Start()
    {
        QualitySettings.maxQueuedFrames = 4;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
        Profiler.maxUsedMemory = 2000000000;//2GB
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
