using Cysharp.Threading.Tasks;
using OSY;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class StreamingEventManager : MonoBehaviour
{
    public CancellationTokenSource ChzzkCTS;
    // Start is called before the first frame update
    public void Start()
    {
        PeepoEventSystem peepoEventSystemHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PeepoEventSystem>();
        UniTask.RunOnThreadPool(async () =>
        {
            await UniTask.SwitchToMainThread();
            await Utils.WaitUntil(() => GameManager.instance?.settingUI != null, Utils.YieldCaches.UniTaskYield, destroyCancellationToken);
            bool isOnSettingUI = GameManager.instance.settingUI.activeInHierarchy;
            bool isOnChannelInfoUI = GameManager.instance.channelInfoUI.activeInHierarchy;
            while (!destroyCancellationToken.IsCancellationRequested)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift))
                {
                    GameManager.instance.settingUI.SetActive(!isOnSettingUI);
                    GameManager.instance.channelInfoUI.GetComponent<Image>().color = GameManager.instance.settingUI.activeInHierarchy ? new Color(0, 0, 0, 0.7f) : new Color(0, 0, 0, 0.3f);
                    GameManager.instance.peepoSpawnRect.gameObject.SetActive(GameManager.instance.settingUI.activeInHierarchy);
                    GameManager.instance.restrictedAreaRoot.gameObject.SetActive(GameManager.instance.settingUI.activeInHierarchy);
                    GameManager.instance.unknownDonationParentsTransform.parent.GetComponent<Image>().enabled = GameManager.instance.settingUI.activeInHierarchy;
                    GameManager.instance.unknownDonationParentsTransform.parent.GetComponentInChildren<TMP_Text>().enabled = GameManager.instance.settingUI.activeInHierarchy;
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

                if (Input.GetKeyDown(KeyCode.RightControl))
                {
                    peepoEventSystemHandle.OnCalm.Invoke();
                }
                await Utils.YieldCaches.UniTaskYield;
                try
                {
                    if (destroyCancellationToken.IsCancellationRequested) return;
                }
                catch (MissingReferenceException ex)
                {
                    return;
                }
            }
        }, true, destroyCancellationToken).Forget();

        #region ※치지직 이벤트
        if (ChzzkUnity.instance != null)
        {
            ChzzkUnity.instance.OnConnectError = (ex) =>
            {
                GameManager.instance.ErrorPOPUP.SetActive(true);
                GameManager.instance.ErrorPOPUPText.text = $"따흐흑ㅠㅠ 에러!!\n\n{ex.Message}";
            };
            ChzzkUnity.instance.OnChat = async (profile, chatID, chatText) =>
            {
                await UniTask.SwitchToMainThread();
                int hash = Animator.StringToHash(profile.nickname);
                await OnInit(peepoEventSystemHandle, hash, profile, profile.streamingProperty?.subscription?.accumulativeMonth ?? 0);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(chatID, chatText, 5f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };
            ChzzkUnity.instance.OnDonation = async (profile, chatID, chatText, extra) =>
            {
                await UniTask.SwitchToMainThread();
                if (profile == null) //익명 후원
                {
                    peepoEventSystemHandle.OnDonation.Invoke(-1, extra.payAmount);
                    new GameManager.ChatInfo(chatID, "<b><color=orange>" + chatText + "</color></b>", 10f, GameManager.instance.unknownDonationParentsTransform, true);
                    return;
                }
                int hash = Animator.StringToHash(profile.nickname);
                await OnInit(peepoEventSystemHandle, hash, profile, profile.streamingProperty?.subscription?.accumulativeMonth ?? 0);
                peepoEventSystemHandle.OnDonation.Invoke(hash, extra.payAmount);

                //new GameManager.ChatInfo(chatID, "<b><color=orange>" + chatText + "</color></b>", 10f, GameManager.instance.unknownDonationParentsTransform, true);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(chatID, "<b><color=orange>" + chatText + "</color></b>", 10f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };
            ChzzkUnity.instance.onSubscription = async (profile, chatID, chatText, extra) =>
            {
                await UniTask.SwitchToMainThread();
                if (profile == null) return;
                int hash = Animator.StringToHash(profile.nickname);
                await OnInit(peepoEventSystemHandle, hash, profile, extra.month);
                peepoEventSystemHandle.onSubscription.Invoke(hash, extra.month);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(chatID, "<b><color=red>" + chatText + "</color></b>", 10f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };

            async UniTask OnInit(PeepoEventSystem peepoEventSystemHandle, int hash, ChzzkUnity.Profile profile, int subMonth)
            {
                bool isInit = !GameManager.instance.viewerInfos.ContainsKey(hash);
                float addLifeTime = 0;

                if (isInit)
                {
                    GameManager.instance.viewerInfos.Add(hash, new GameManager.ViewerInfo(profile.nickname, subMonth));
                    GameManager.instance.spawnOrderQueue.Enqueue(new GameManager.SpawnOrder(hash,
                        initForce: new float3(Utils.GetRandom(GameManager.instance.SpawnMinSpeed.x, GameManager.instance.SpawnMaxSpeed.x), Utils.GetRandom(GameManager.instance.SpawnMinSpeed.y, GameManager.instance.SpawnMaxSpeed.y), 0)));
                    peepoEventSystemHandle.OnSpawn.Invoke();
                    await Utils.YieldCaches.UniTaskYield;
                }
                else
                    addLifeTime = GameManager.instance.peepoConfig.addLifeTime;

                peepoEventSystemHandle.OnChat.Invoke(hash, addLifeTime);
                GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform.localScale = Vector3.one * GameManager.instance.chatBubbleSize;
            }
        }
        #endregion

        #region ※유튜브 이벤트
        if (YoutubeUnity.instance != null)
        {
            YoutubeUnity.instance.OnChatEvent = async (chatInfo) =>
            {
                await UniTask.SwitchToMainThread();
                int hash = Animator.StringToHash($"{chatInfo.authorDetails.channelId}{GameManager.instance.nameSpliter}{chatInfo.authorDetails.displayName}");
                await OnInit(peepoEventSystemHandle, hash, chatInfo.authorDetails);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(chatInfo.id, chatInfo.snippet.displayMessage, 5f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };
            YoutubeUnity.instance.OnSuperChatEvent = async (chatInfo) =>
            {
                await UniTask.SwitchToMainThread();
                int hash = Animator.StringToHash($"{chatInfo.authorDetails.channelId}{GameManager.instance.nameSpliter}{chatInfo.authorDetails.displayName}");
                await OnInit(peepoEventSystemHandle, hash, chatInfo.authorDetails);
                peepoEventSystemHandle.OnDonation.Invoke(hash, int.Parse(Regex.Replace(chatInfo.snippet.superChatDetails.amountDisplayString, @"\D", "")));

                //new GameManager.ChatInfo(chatID, "<b><color=orange>" + chatText + "</color></b>", 10f, GameManager.instance.unknownDonationParentsTransform, true);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(chatInfo.id, "<b><color=orange>" + chatInfo.snippet.displayMessage + "</color></b>", 10f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };
            YoutubeUnity.instance.OnSuperStickerEvent = async (chatInfo) =>
            {
                await UniTask.SwitchToMainThread();
                int hash = Animator.StringToHash($"{chatInfo.authorDetails.channelId}{GameManager.instance.nameSpliter}{chatInfo.authorDetails.displayName}");
                await OnInit(peepoEventSystemHandle, hash, chatInfo.authorDetails);
                peepoEventSystemHandle.OnDonation.Invoke(hash, int.Parse(Regex.Replace(chatInfo.snippet.superStickerDetails.amountDisplayString, @"\D", "")));

                //new GameManager.ChatInfo(chatID, "<b><color=orange>" + chatText + "</color></b>", 10f, GameManager.instance.unknownDonationParentsTransform, true);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(chatInfo.id, "<b><color=orange>" + chatInfo.snippet.displayMessage + "</color></b>", 10f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };
            YoutubeUnity.instance.OnNewSponsorEvent = async (chatInfo) =>
            {
                await UniTask.SwitchToMainThread();
                int hash = Animator.StringToHash($"{chatInfo.authorDetails.channelId}{GameManager.instance.nameSpliter}{chatInfo.authorDetails.displayName}");
                await OnInit(peepoEventSystemHandle, hash, chatInfo.authorDetails);
                peepoEventSystemHandle.onSubscription.Invoke(hash, chatInfo.snippet.memberMilestoneChatDetails.memeberMonth);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(chatInfo.id, "<b><color=red>" + chatInfo.snippet.displayMessage + "</color></b>", 10f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };
            YoutubeUnity.instance.OnMemberMilestoneChatEvent = async (chatInfo) =>
            {
                await UniTask.SwitchToMainThread();
                int hash = Animator.StringToHash($"{chatInfo.authorDetails.channelId}{GameManager.instance.nameSpliter}{chatInfo.authorDetails.displayName}");
                await OnInit(peepoEventSystemHandle, hash, chatInfo.authorDetails, chatInfo.snippet.memberMilestoneChatDetails.memeberMonth);
                peepoEventSystemHandle.onSubscription.Invoke(hash, chatInfo.snippet.memberMilestoneChatDetails.memeberMonth);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(chatInfo.id, "<b><color=red>" + chatInfo.snippet.displayMessage + "</color></b>", 10f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };

            async UniTask OnInit(PeepoEventSystem peepoEventSystemHandle, int hash, YoutubeUnity.LiveChatInfo.Chat.AuthorDetails authorDetails, int subMonth = 0)
            {
                bool isInit = !GameManager.instance.viewerInfos.ContainsKey(hash);
                float addLifeTime = 0;

                if (isInit)
                {
                    GameManager.instance.viewerInfos.Add(hash, new GameManager.ViewerInfo($"{authorDetails.channelId}{GameManager.instance.nameSpliter}{authorDetails.displayName}", subMonth));
                    GameManager.instance.spawnOrderQueue.Enqueue(new GameManager.SpawnOrder(hash,
                        initForce: new float3(Utils.GetRandom(GameManager.instance.SpawnMinSpeed.x, GameManager.instance.SpawnMaxSpeed.x), Utils.GetRandom(GameManager.instance.SpawnMinSpeed.y, GameManager.instance.SpawnMaxSpeed.y), 0)));
                    peepoEventSystemHandle.OnSpawn.Invoke();
                    await Utils.YieldCaches.UniTaskYield;
                }
                else
                    addLifeTime = GameManager.instance.peepoConfig.addLifeTime;

                peepoEventSystemHandle.OnChat.Invoke(hash, addLifeTime);
                GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform.localScale = Vector3.one * GameManager.instance.chatBubbleSize;
            }
        }
        #endregion

        #region 아프리카 도우미 이벤트

        if (AHSeleniumUnity.instance != null)
        {
            AHSeleniumUnity.instance.OnChat = async (authorName, chatText) =>
            {
                await UniTask.SwitchToMainThread();
                int hash = Animator.StringToHash(authorName);
                await OnInit(peepoEventSystemHandle, hash, authorName, 0);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(string.Empty, chatText, 5f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };
            AHSeleniumUnity.instance.OnDonation = async (authorName, chatText, amount) =>
            {
                await UniTask.SwitchToMainThread();
                int hash = Animator.StringToHash(authorName);
                await OnInit(peepoEventSystemHandle, hash, authorName, 0);
                peepoEventSystemHandle.OnDonation.Invoke(hash, amount);

                //new GameManager.ChatInfo(chatID, "<b><color=orange>" + chatText + "</color></b>", 10f, GameManager.instance.unknownDonationParentsTransform, true);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(string.Empty, "<b><color=orange>" + chatText + "</color></b>", 10f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };
            AHSeleniumUnity.instance.onSubscription = async (authorName, chatText, month) =>
            {
                await UniTask.SwitchToMainThread();
                int hash = Animator.StringToHash(authorName);
                await OnInit(peepoEventSystemHandle, hash, authorName, month);
                peepoEventSystemHandle.onSubscription.Invoke(hash, month);
                GameManager.instance.viewerInfos[hash].chatInfos.Add(new GameManager.ChatInfo(string.Empty, "<b><color=red>" + chatText + "</color></b>", 10f, GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform));
            };

            async UniTask OnInit(PeepoEventSystem peepoEventSystemHandle, int hash, string authorName, int subMonth = 0)
            {
                Utils.hashMemory.TryAdd(hash, authorName);
                bool isInit = !GameManager.instance.viewerInfos.ContainsKey(hash);
                float addLifeTime = 0;

                if (isInit)
                {
                    GameManager.instance.viewerInfos.Add(hash, new GameManager.ViewerInfo(authorName, subMonth));
                    GameManager.instance.spawnOrderQueue.Enqueue(new GameManager.SpawnOrder(hash,
                        initForce: new float3(Utils.GetRandom(GameManager.instance.SpawnMinSpeed.x, GameManager.instance.SpawnMaxSpeed.x), Utils.GetRandom(GameManager.instance.SpawnMinSpeed.y, GameManager.instance.SpawnMaxSpeed.y), 0)));
                    peepoEventSystemHandle.OnSpawn.Invoke();
                    await Utils.YieldCaches.UniTaskYield;
                }
                else
                    addLifeTime = GameManager.instance.peepoConfig.addLifeTime;

                peepoEventSystemHandle.OnChat.Invoke(hash, addLifeTime);
                GameManager.instance.viewerInfos[hash].chatBubbleObjects.transform.localScale = Vector3.one * GameManager.instance.chatBubbleSize;
            }
        }
        #endregion
    }
    public void StartChzzk()
    {
        UniTask.RunOnThreadPool(async () =>
        {
            if (ChzzkUnity.instance != null)
            {
                if (ChzzkUnity.instance.socket != null && ChzzkUnity.instance.socket.IsAlive)
                {
                    ChzzkCTS?.Cancel();
                    ChzzkUnity.instance.socket.Close();
                    ChzzkUnity.instance.socket = null;
                }
                ChzzkCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
                await ChzzkUnity.instance.Connect();
                UniTask.RunOnThreadPool(async () =>
                {
                    await UniTask.SwitchToMainThread();
                    while (!ChzzkCTS.IsCancellationRequested)
                    {
                        ChzzkUnity.instance.liveStatus = await ChzzkUnity.instance.GetLiveStatus(ChzzkUnity.instance.inputChannelID.text);
                        GameManager.instance.channelViewerCount.text = $"시청자 수: {ChzzkUnity.instance.liveStatus.content.concurrentUserCount}";
                        GameManager.instance.peepoCountText.text = $"채팅 참여자 수: {GameManager.instance.viewerInfos.Count}";
                        await UniTask.Delay(TimeSpan.FromSeconds(2), false, PlayerLoopTiming.FixedUpdate, ChzzkCTS.Token, true);
                    }
                }, true, ChzzkCTS.Token).Forget();
            }
        }, true, destroyCancellationToken).Forget();
    }
    public void StopChzzk()
    {
        if (ChzzkUnity.instance != null)
        {
            if (ChzzkUnity.instance.socket != null && ChzzkUnity.instance.socket.IsAlive)
            {
                ChzzkUnity.instance.socket.Close();
                ChzzkUnity.instance.socket = null;
            }
        }
        ChzzkCTS?.Cancel();
        ChzzkCTS?.Dispose();
    }

    public CancellationTokenSource youtubeCTS;
    public void StartYoutube()
    {
        youtubeCTS?.Cancel();
        youtubeCTS?.Dispose();
        if (YoutubeUnity.instance != null)
        {
            youtubeCTS = CancellationTokenSource.CreateLinkedTokenSource(GameManager.instance.destroyCancellationToken);
            UniTask.RunOnThreadPool(() => YoutubeUnity.instance.Connect(youtubeCTS.Token), true).Forget();
        }
    }
    public void StopYoutube()
    {
        youtubeCTS?.Cancel();
        youtubeCTS?.Dispose();
    }

    public async void OnDestroy()
    {
        ChzzkCTS?.Cancel();
        youtubeCTS?.Cancel();
        await UniTask.Yield();
        ChzzkCTS?.Dispose();
        youtubeCTS?.Dispose();
    }
}
