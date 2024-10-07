using Cysharp.Threading.Tasks;
using OSY;
using System;
using System.Threading;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class StreamingEventManager : MonoBehaviour
{
    public CancellationTokenSource LiveCTS;
    // Start is called before the first frame update
    void Start()
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
            }
        }, true, destroyCancellationToken).Forget();

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
    }
    public void StartLive()
    {
        UniTask.RunOnThreadPool(async () =>
        {
            if (ChzzkUnity.instance != null)
            {
                if (ChzzkUnity.instance.socket != null && ChzzkUnity.instance.socket.IsAlive)
                {
                    LiveCTS?.Cancel();
                    ChzzkUnity.instance.socket.Close();
                    ChzzkUnity.instance.socket = null;
                }
                LiveCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
                await ChzzkUnity.instance.Connect();
                UniTask.RunOnThreadPool(async () =>
                {
                    await UniTask.SwitchToMainThread();
                    while (!LiveCTS.IsCancellationRequested)
                    {
                        ChzzkUnity.instance.liveStatus = await ChzzkUnity.instance.GetLiveStatus(ChzzkUnity.instance.inputChannelID.text);
                        GameManager.instance.channelViewerCount.text = $"시청자 수: {ChzzkUnity.instance.liveStatus.content.concurrentUserCount}";
                        GameManager.instance.peepoCountText.text = $"채팅 참여자 수: {GameManager.instance.viewerInfos.Count}";
                        await UniTask.Delay(TimeSpan.FromSeconds(2), false, PlayerLoopTiming.FixedUpdate, LiveCTS.Token, true);
                    }
                }, true, LiveCTS.Token).Forget();
            }
        }, true, destroyCancellationToken).Forget();
    }
    public void StopLive()
    {
        if (ChzzkUnity.instance != null)
        {
            if (ChzzkUnity.instance.socket != null && ChzzkUnity.instance.socket.IsAlive)
            {
                ChzzkUnity.instance.socket.Close();
                ChzzkUnity.instance.socket = null;
            }
        }
    }
    private void OnDestroy()
    {
        LiveCTS?.Cancel();
    }
}
