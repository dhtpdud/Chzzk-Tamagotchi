using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OSY;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;
using MessageEventArgs = WebSocketSharp.MessageEventArgs;
using WebSocket = WebSocketSharp.WebSocket;

public class ChzzkUnity : MonoBehaviour
{
    public CancellationTokenSource LiveCTS;
    LiveStatus liveStatus;
    //WSS(WS 말고 WSS) 쓰려면 필요함.
    private enum SslProtocolsHack
    {
        Tls = 192,
        Tls11 = 768,
        Tls12 = 3072
    }

    string cid;
    string token;
    //public string channelID;
    public TMP_InputField inputChannelID;

    WebSocket socket = null;
    string wsURL = "wss://kr-ss3.chat.naver.com/chat";

    float timer = 0f;
    bool running = false;

    string heartbeatRequest = "{\"ver\":\"2\",\"cmd\":0}";
    string heartbeatResponse = "{\"ver\":\"2\",\"cmd\":10000}";

    public Action<Profile, string> onChat = (profile, str) => { };
    public Action<Profile, string, DonationExtras> onDonation = (profile, str, extra) => { };

    // Start is called before the first frame update
    void Start()
    {
        UniTask.RunOnThreadPool(async () =>
        {
            await UniTask.SwitchToMainThread();
            bool isOnSettingUI = GameManager.instance.settingUI.activeInHierarchy;
            bool isOnChannelInfoUI = GameManager.instance.channelInfoUI.activeInHierarchy;
            while (!destroyCancellationToken.IsCancellationRequested)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift))
                {
                    GameManager.instance.settingUI.SetActive(!isOnSettingUI);
                }
                else if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftShift))
                {
                    isOnSettingUI = GameManager.instance.settingUI.activeInHierarchy;
                }

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.RightShift))
                {
                    GameManager.instance.channelInfoUI.SetActive(!isOnChannelInfoUI);
                }
                else if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightShift))
                {
                    isOnChannelInfoUI = GameManager.instance.channelInfoUI.activeInHierarchy;
                }
                await Utils.YieldCaches.UniTaskYield;
            }
        }, true, destroyCancellationToken).Forget();
        onChat += async (profile, chatText) =>
        {
            await UniTask.SwitchToMainThread();
            int hash = Animator.StringToHash(profile.nickname);
            bool isInit = !GameManager.instance.viewerInfos.ContainsKey(hash);

            if (isInit)
            {
                var nickNameColor = Color.white;
                //Debug.Log(nickNameColor.ToHexString());
                GameManager.instance.viewerInfos.Add(hash, new GameManager.ViewerInfo(profile.nickname, nickNameColor));
                GameManager.instance.spawnOrderQueue.Enqueue(new GameManager.SpawnOrder(hash,
                initForce: new float3(Utils.GetRandom(-10, 10), Utils.GetRandom(-10, 0), 0),
                spawnPosx: Utils.GetRandom(-16, 16),
                size: Utils.GetRandom(0.2f, 1.5f)));
                GameManager.instance.spawnTrigger = true;
            }
            GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(chatText));
        };
    }
    public void StartLive()
    {
        UniTask.RunOnThreadPool(async ()=> 
        {
            await Connect();
            UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                while (!LiveCTS.IsCancellationRequested)
                {
                    liveStatus = await GetLiveStatus(inputChannelID.text);
                    GameManager.instance.channelViewerCount.text = $"시청자 수: {liveStatus.content.concurrentUserCount}";
                    await UniTask.Delay(TimeSpan.FromSeconds(2), false, PlayerLoopTiming.FixedUpdate, LiveCTS.Token, true);
                }
            }, true, LiveCTS.Token).Forget();
        }, true, destroyCancellationToken).Forget();
    }
    public void StopLive()
    {
        if (socket != null && socket.IsAlive)
        {
            socket.Close();
            socket = null;
        }
    }
    public void removeAllOnMessageListener()
    {
        onChat = (profile, str) => { };
    }

    public void removeAllOnDonationListener()
    {
        onDonation = (profile, str, extra) => { };
    }

    //20초에 한번 HeartBeat 전송해야 함.
    //서버에서 먼저 요청하면 안 해도 됨.
    //TimeScale에 영향 안 받기 위해서 Fixed
    void FixedUpdate()
    {
        if (running)
        {
            timer += Time.unscaledDeltaTime;
            if (timer > 15)
            {
                socket.Send(heartbeatRequest);
                timer = 0;
            }
        }
    }

    public async UniTask<ChannelInfo> GetChannelInfo(string channelId)
    {
        string URL = $"https://api.chzzk.naver.com/service/v1/channels/{channelId}";
        UnityWebRequest request = UnityWebRequest.Get(URL);
        await request.SendWebRequest();
        ChannelInfo channelInfo = null;
        Debug.Log(request.downloadHandler.text);
        if (request.result == UnityWebRequest.Result.Success)
        {
            //Cid 획득
            channelInfo = JsonUtility.FromJson<ChannelInfo>(request.downloadHandler.text);
        }
        return channelInfo;
    }

    public async UniTask<LiveStatus> GetLiveStatus(string channelId)
    {
        string URL = $"https://api.chzzk.naver.com/polling/v2/channels/{channelId}/live-status";
        await UniTask.SwitchToMainThread();
        UnityWebRequest request = UnityWebRequest.Get(URL);
        await request.SendWebRequest();
        LiveStatus liveStatus = null;
        if (request.result == UnityWebRequest.Result.Success)
        {
            //Cid 획득
            liveStatus = JsonUtility.FromJson<LiveStatus>(request.downloadHandler.text);
        }
        return liveStatus;
    }

    public async UniTask<AccessTokenResult> GetAccessToken(string cid)
    {
        string URL = $"https://comm-api.game.naver.com/nng_main/v1/chats/access-token?channelId={cid}&chatType=STREAMING";
        await UniTask.SwitchToMainThread();
        UnityWebRequest request = UnityWebRequest.Get(URL);
        await request.SendWebRequest();
        AccessTokenResult accessTokenResult = null;
        if (request.result == UnityWebRequest.Result.Success)
        {
            //Cid 획득
            accessTokenResult = JsonUtility.FromJson<AccessTokenResult>(request.downloadHandler.text);
        }

        return accessTokenResult;
    }

    public async UniTask Connect()
    {
        if (socket != null && socket.IsAlive)
        {
            LiveCTS?.Cancel();
            socket.Close();
            socket = null;
        }
        LiveCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

        liveStatus = await GetLiveStatus(inputChannelID.text);
        cid = liveStatus.content.chatChannelId;
        AccessTokenResult accessTokenResult = await GetAccessToken(cid);
        token = accessTokenResult.content.accessToken;

        socket = new WebSocket(wsURL);
        //wss라서 ssl protocol을 활성화 해줘야 함.
        var sslProtocolHack = (System.Security.Authentication.SslProtocols)(SslProtocolsHack.Tls12 | SslProtocolsHack.Tls11 | SslProtocolsHack.Tls);
        socket.SslConfiguration.EnabledSslProtocols = sslProtocolHack;

        //이벤트 등록
        socket.OnMessage += Recv;
        socket.OnClose += CloseConnect;
        socket.OnOpen += OnStartChat;

        //연결
        socket.Connect();
    }

    public void Connect(string channelId)
    {
        inputChannelID.text = channelId;
        UniTask.RunOnThreadPool(Connect, true, destroyCancellationToken).Forget();
    }


    void Recv(object sender, MessageEventArgs e)
    {
        try
        {
            IDictionary<string, object> data = JsonConvert.DeserializeObject<IDictionary<string, object>>(e.Data);

            //Cmd에 따라서
            switch ((long)data["cmd"])
            {
                case 0://HeartBeat Request
                    //하트비트 응답해줌.
                    socket.Send(heartbeatResponse);
                    //서버가 먼저 요청해서 응답했으면 타이머 초기화해도 괜찮음.
                    timer = 0;
                    break;
                case 93101://Chat
                    JArray bdy = (JArray)data["bdy"];
                    JObject bdyObject = (JObject)bdy[0];

                    //프로필이.... json이 아니라 string으로 들어옴.
                    string profileText = bdyObject["profile"].ToString();
                    Debug.Log(profileText);
                    profileText = profileText.Replace("\\", "");
                    Profile profile = JsonUtility.FromJson<Profile>(profileText);
                    string chatTxt = bdyObject["msg"].ToString().Trim();
                    Debug.Log(profile.nickname + ": " + chatTxt);
                    onChat(profile, chatTxt);
                    break;
                case 93102://Donation
                    bdy = (JArray)data["bdy"];
                    bdyObject = (JObject)bdy[0];

                    //프로필 스트링 변환
                    profileText = bdyObject["profile"].ToString();
                    profileText = profileText.Replace("\\", "");
                    profile = JsonUtility.FromJson<Profile>(profileText);

                    Debug.Log(bdyObject);
                    //도네이션과 관련된 데이터는 extra
                    /*string extraText = bdyObject["extra"].ToString();
                    extraText = extraText.Replace("\\", "");
                    DonationExtras extras = JsonUtility.FromJson<DonationExtras>(extraText);


                    onDonation(profile, bdyObject["msg"].ToString(), extras);*/
                    break;
                case 94008://Blocked Message(CleanBot) 차단된 메세지.
                case 94201://Member Sync 멤버 목록 동기화.
                case 10000://HeartBeat Response 하트비트 응답.
                case 10100://Token ACC
                    break;//Nothing to do
                default:
                    //내가 놓친 cmd가 있나?
                    Debug.LogError(data["cmd"]);
                    Debug.LogError(e.Data);
                    break;
            }
        }

        catch (Exception er)
        {
            Debug.LogException(er);
        }
    }

    void CloseConnect(object sender, CloseEventArgs e)
    {
        Debug.Log(e.Reason);
        Debug.Log(e.Code);
        Debug.Log(e);

        try
        {
            if (socket == null) return;

            if (socket.IsAlive) socket.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.StackTrace);
        }
    }

    void OnStartChat(object sender, EventArgs e)
    {
        Debug.Log($"OPENED : {cid} + {token}");

        string message = $"{{\"ver\":\"2\",\"cmd\":100,\"svcid\":\"game\",\"cid\":\"{cid}\",\"bdy\":{{\"uid\":null,\"devType\":2001,\"accTkn\":\"{token}\",\"auth\":\"READ\"}},\"tid\":1}}";
        timer = 0;
        running = true;
        socket.Send(message);
    }

    private void OnDestroy()
    {
        removeAllOnDonationListener();
        removeAllOnMessageListener();
        LiveCTS?.Cancel();
        if (socket != null && socket.IsAlive)
        {
            socket.Close();
            socket = null;
        }
    }



    public void StopListening()
    {
        socket.Close();
        socket = null;
    }

    [Serializable]
    public class LiveStatus
    {
        public int code;
        public string message;
        public Content content;

        [Serializable]
        public class Content
        {
            public string liveTitle;
            public string status;
            public int concurrentUserCount;
            public int accumulateCount;
            public bool paidPromotion;
            public bool adult;
            public string chatChannelId;
            public string categoryType;
            public string liveCategory;
            public string liveCategoryValue;
            public string livePollingStatusJson;
            public string faultStatus;
            public string userAdultStatus;
            public bool chatActive;
            public string chatAvailableGroup;
            public string chatAvailableCondition;
            public int minFollowerMinute;
        }
    }

    [Serializable]
    public class AccessTokenResult
    {
        public int code;
        public string message;
        public Content content;
        [Serializable]
        public class Content
        {
            public string accessToken;

            [Serializable]
            public class TemporaryRestrict
            {
                public bool temporaryRestrict;
                public int times;
                public int duration;
                public int createdTime;
            }
            public bool realNameAuth;
            public string extraToken;
        }
    }

    [Serializable]
    public class Profile
    {
        public string userIdHash;
        public string nickname;
        public string profileImageUrl;
        public string userRoleCode;
        public string badge;
        public string title;
        public string verifiedMark;
        public List<String> activityBadges;
        public StreamingProperty streamingProperty;
        [Serializable]
        public class StreamingProperty
        {
        }
    }


    [Serializable]
    public class DonationExtras
    {
        System.Object emojis;
        public bool isAnonymous;
        public string payType;
        public int payAmount;
        public string streamingChannelId;
        public string nickname;
        public string osType;
        public string donationType;

        public List<WeeklyRank> weeklyRankList;
        [Serializable]
        public class WeeklyRank
        {
            public string userIdHash;
            public string nickName;
            public bool verifiedMark;
            public int donationAmount;
            public int ranking;
        }
        public WeeklyRank donationUserWeeklyRank;
    }

    [Serializable]
    public class ChannelInfo
    {
        public int code;
        public string message;
        public Content content;

        [Serializable]
        public class Content
        {
            public string channelId;
            public string channelName;
            public string channelImageUrl;
            public bool verifiedMark;
            public string channelType;
            public string channelDescription;
            public int followerCount;
            public bool openLive;
        }
    }
}