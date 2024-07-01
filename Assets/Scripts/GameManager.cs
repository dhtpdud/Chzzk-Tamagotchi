using Cysharp.Threading.Tasks;
using OSY;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameManagerInfoSystem gameManagerSystem;
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

    public float gravity;
    public void SetGravity(string val)
    {
        gravity = float.Parse(val);
        gameManagerSystem.UpdateSetting();
    }
    public float dragPower;
    public float stabilityPower;
    public float physicMaxVelocity;

    [Serializable]
    public class PeepoConfig
    {
        public float DefalutLifeTime;
        public float AddLifeTime;
        public float MaxLifeTime;
        public float DefaultSize;
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
    public PeepoConfig peepoConfig;
    public void SetDefalutLifeTime(string val)
    {
        peepoConfig.DefalutLifeTime = float.Parse(val);
        gameManagerSystem.UpdateSetting();
    }
    public void SetAddLifeTime(string val)
    {
        peepoConfig.AddLifeTime = float.Parse(val);
        gameManagerSystem.UpdateSetting();
    }
    public void SetMaxLifeTime(string val)
    {
        peepoConfig.MaxLifeTime = float.Parse(val);
        gameManagerSystem.UpdateSetting();
    }
    public void SetDefaultSize(string val)
    {
        peepoConfig.DefaultSize = float.Parse(val);
        gameManagerSystem.UpdateSetting();
    }
    public void SetMinSize(string val)
    {
        peepoConfig.MinSize = float.Parse(val);
        gameManagerSystem.UpdateSetting();
    }
    public void SetMaxSize(string val)
    {
        peepoConfig.MaxSize = float.Parse(val);
        gameManagerSystem.UpdateSetting();
    }
    public Dictionary<int, Texture2D> thumbnailsCacheDic = new Dictionary<int, Texture2D>();

    [Header("UI")]
    public Transform nameTagUICanvasTransform;
    public Transform chatBubbleUICanvasTransform;
    public GameObject settingUI;
    public GameObject channelInfoUI;
    public TMP_Text channelViewerCount;
    public TMP_Text peepoCountText;
    public GameObject ErrorPOPUP;
    public TMP_InputField ErrorPOPUPText;
    public RectTransform peepoSpawnRect;

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
            this.bubbleObject = Instantiate(instance.chatBubble, instance.chatBubbleUICanvasTransform);
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
                DestroyImmediate(bubbleObject);
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
            this.nameTagObject = Instantiate(instance.nameTag, instance.nameTagUICanvasTransform);
            var tmp = nameTagObject.GetComponentInChildren<TMP_Text>();
            tmp.text = nickName;
            //Debug.Log(nicknameColor.ToHexString());
            tmp.color = nicknameColor;
        }
        public void OnDestroy()
        {
            UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                chatInfos.Clear();
                Destroy(nameTagObject);
                await Utils.YieldCaches.UniTaskYield;
                GameManager.instance.viewerInfos.Remove(Animator.StringToHash(nickName));
            },true,GameManager.instance.destroyCancellationToken).Forget();
        }
    }
    //캐싱 변수
    public Dictionary<int, ViewerInfo> viewerInfos = new Dictionary<int, ViewerInfo>();

    public struct SpawnOrder
    {
        public int hash;
        public float3 initForce;

        public SpawnOrder(int hash, float3 initForce)
        {
            this.hash = hash;
            this.initForce = initForce;
        }
    }
    public Queue<SpawnOrder> spawnOrderQueue = new Queue<SpawnOrder>();

    public Vector2 onMouseDownPosition;
    public Vector2 onMouseDragPosition;
    public GameObject dragingObject;

    protected void Awake()
    {
        instance = this;
        var tokenInit = destroyCancellationToken;
        mainCam ??= Camera.main;
        originTargetFramerate = Application.targetFrameRate;
        origincaptureFramerate = Time.captureFramerate;
        originVSyncCount = QualitySettings.vSyncCount;
    }
    private void Start()
    {
        gameManagerSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GameManagerInfoSystem>();
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
        if (mainCam != null)
        {
            if (Input.GetMouseButtonDown(0))
                onMouseDownPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButton(0))
                onMouseDragPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}
