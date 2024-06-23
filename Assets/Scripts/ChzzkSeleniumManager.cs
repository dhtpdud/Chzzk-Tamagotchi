using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OSY;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class ChzzkSeleniumManager : MonoBehaviour
{
    /*public Dictionary<string, List<ChatInfo>> chatInfos = new Dictionary<string, List<ChatInfo>>();
    public bool isHeadless;

    //string 캐싱
    string baseUrl = "https://chzzk.naver.com/live/";
    [SerializeField]
    string liveTokent = "4d3c16597264971b1f0277b19e8b3053/chat";
    string href = "href";
    string pageOption = "?page=";
    string nextLine = "\n";
    string emptyString = "";
    string stringKey = "key";
    string stringTextContent = "textContent";

    protected ChromeOptions _options = null;
    ChromeDriverService _driverService;
    ChromeDriver _driver;
    // Start is called before the first frame update
    void Start()
    {
        //드라이버 초기화
        _driverService = ChromeDriverService.CreateDefaultService();
        _driverService.HideCommandPromptWindow = true;
        _options = new ChromeOptions();
        if (isHeadless)
        {
            _options.AddArgument("--disable-gpu");
            _options.AddArgument("--headless");
        }
        _options.AddArgument("--disable-infobars");
        _options.AddArgument("--disable-extensions");
        //_options.AddArgument("--blink-settings=imagesEnabled=false");
        _driver = new ChromeDriver(_driverService, _options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        _driver.Navigate().GoToUrl(baseUrl + liveTokent);
        UniTask.RunOnThreadPool(async () =>
        {
            while (!destroyCancellationToken.IsCancellationRequested)
            {
                var chatElements = _driver.FindElements(By.ClassName("live_chatting_list_item__0SGhw"));

                *//*var chatTextCompnents = _driver.FindElements(By.ClassName("live_chatting_message_text__DyleH"));
                var viewerNameCompnents = _driver.FindElements(By.ClassName("live_chatting_username_container__m1-i5 live_chatting_username_is_message__jvTvP"));*//*

                string CNchatTextCompnent = "live_chatting_message_text__DyleH";
                string CNviewerNameCompnent = "live_chatting_username_nickname__dDbbj";

                var chatElement = chatElements.Last();
                var chatTextCompnent = chatElement.FindElement(By.ClassName(CNchatTextCompnent));
                var viewerNameCompnent = chatElement.FindElement(By.ClassName(CNviewerNameCompnent));

                string viewerName = viewerNameCompnent.GetAttribute(stringTextContent);
                ViewerInfo viewerInfo = new ViewerInfo { name = viewerName, lastChatIndex = 0 };

                //메세지 카운트 업데이트
                *//*_driver.ExecuteScript("arguments[0].click();", chatComponent.FindElement(By.TagName("BUTTON")));
                var viewerInfoComponent = chatComponent.FindElement(By.ClassName("live_chatting_popup_profile_container__+YBl1 live_chatting_popup_profile_is_popup_chat__-fFc6"));
                viewerInfo.messeageCount = uint.Parse(viewerInfoComponent.FindElement(By.ClassName("live_chatting_popup_profile_count__oNaRD")).GetAttribute(stringTextContent));*//*

                if (chatInfos.TryAdd(viewerInfo.name, new List<ChatInfo>()))
                {
                    await UniTask.SwitchToMainThread();
                    //peepo소환
                    var spawnedPeepo = Instantiate(GameManager.Instance.peepo, GameManager.Instance.subscene);
                    spawnedPeepo.GetComponent<Rigidbody>().AddForce(Vector2.left * 10, ForceMode.Impulse);
                    spawnedPeepo.transform.localScale = Vector2.one * Utils.GetRandom(1, 3);
                    await UniTask.SwitchToThreadPool();
                }

                *//*if (chatInfos.ContainsKey(viewerInfo.name))
                {
                    var temp = chatElement.GetAttribute("__reactFiber$6hizzxrrxes");
                    UnityEngine.Debug.Log(temp);
                    var chatJson = JObject.Parse(temp);
                    ChatInfo chatInfo = new ChatInfo { id = (string)chatJson[stringKey], text = chatTextCompnent.GetAttribute(stringTextContent) };
                    if (!chatInfos[viewerInfo.name].Contains(chatInfo))
                    {
                        chatInfos[viewerInfo.name].Add(chatInfo);
                        //말풍선
                    }
                }*//*

                await UniTask.Delay(TimeSpan.FromSeconds(2));
            }
            _driver.Navigate().Refresh();
        }, true, destroyCancellationToken).Forget();

    }
    private void OnDestroy()
    {
        _driver?.Close();
        _driver?.Quit();
        foreach (Process P in Process.GetProcessesByName("chromedriver"))
            P.Kill();
        _driverService?.Dispose();
    }*/
}
