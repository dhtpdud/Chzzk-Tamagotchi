using Cysharp.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OSY;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using static UnityEditor.Progress;
using Debug = UnityEngine.Debug;


// 아프리카 도우미 사용시 구독자 기간은 어떻게 조회 할것인지?
public class AHSeleniumUnity : MonoBehaviour
{
    public static AHSeleniumUnity instance;
    public bool isHeadless;

    //string 캐싱
    string baseUrl = "http://afreehp.kr/";
    public TMP_InputField InputChatUrl;

    protected ChromeOptions _options = null;
    ChromeDriverService _driverService;
    ChromeDriver _driver;

    public Action<string, string> OnChat = (authorName, str) => { };
    public Action<string, string, int> OnDonation = (authorName, str, donationAmount) => { };
    public Action<string, string, int> onSubscription = (authorName, str, month) => { };
    public void Awake()
    {
        instance = this;
    }
    /*public void LoginAH()
    {
        UniTask.RunOnThreadPool(async () =>
        {
            System.Environment.SetEnvironmentVariable("SE_MANAGER_PATH", Environment.CurrentDirectory + "\\Selenium\\selenium-manager.exe");
            //드라이버 초기화
            _driverService = ChromeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;
            _options = new ChromeOptions();
            _options.AddArgument("--ignore-certificate-errors");
            _options.AddArgument("--disable-infobars");
            _options.AddArgument("--disable-extensions");
            _options.AddArgument($"--app={InputChatUrl.text}");
            _options.AddArgument("disable-blink-features=AutomationControlled");
            _options.AddArgument($"user-data-dir={Environment.CurrentDirectory}\\Selenium\\UserData");
            //_options.PageLoadStrategy = PageLoadStrategy.Eager;

            //_options.AddArgument("--blink-settings=imagesEnabled=false");
            _driver = new ChromeDriver(_driverService, _options);
            //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            _driver.Navigate().GoToUrl(baseUrl);
            IWebElement loginButton = null;
            while (loginButton != null)
            {
                try
                {
                    loginButton = _driver.FindElement(By.XPath("//*[@id=\"main_header\"]/div/ul/li[5]/a[1]"));
                }
                catch (NoSuchElementException)
                {
                    _driver.Navigate().Refresh();
                }
            }

            if (loginButton != null)
            {
                loginButton.Click();
                string elementToKeepXPath = "//*[@id=\"popup_login\"]";

                ((IJavaScriptExecutor)_driver).ExecuteScript(@"
    var allElements = document.getElementsByTagName('*');
    for (var i = 0; i < allElements.length; i++) {
        allElements[i].style.pointerEvents = 'none';
        allElements[i].style.opacity = '0.5';
    }
    var enabledElements = document.evaluate('/html | /html/body', document, null, XPathResult.UNORDERED_NODE_SNAPSHOT_TYPE, null);
    for (var i = 0; i < enabledElements.snapshotLength; i++) {
        var element = enabledElements.snapshotItem(i);
        element.style.pointerEvents = 'auto';
        element.style.opacity = '1';
    }
    var enabledElements = document.evaluate('//*[@id=""popup_login""]', document, null, XPathResult.UNORDERED_NODE_SNAPSHOT_TYPE, null);
    for (var i = 0; i < enabledElements.snapshotLength; i++) {
        var element = enabledElements.snapshotItem(i);
        element.style.pointerEvents = 'auto';
        element.style.opacity = '1';
        var children = element.getElementsByTagName('*');
        for (var j = 0; j < children.length; j++) {
            children[j].style.pointerEvents = 'auto';
            children[j].style.opacity = '1';
        }
    }
");
                //IWebElement element = waiter.Until(ExpectedConditions.ElementIsVisible(By.Id("//*[@id=\"main_header\"]/div/ul/li[6]/a")));
                while (_driver.Url != "http://afreehp.kr/dashboard")
                    await UniTask.Yield();
                Debug.Log("로그인 성공 감지됨");
            }
            else
            {
                Debug.Log("이미 로그인되어 있음");
            }
            var cookies = _driver.Manage().Cookies.AllCookies;
            _driver.Quit();

            if (isHeadless)
            {
                _options.AddArgument("--headless");
                _options.AddArgument("--window-size=1920,1080");
            }
            _driver = new ChromeDriver(_driverService, _options);
            //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            foreach (var cookie in cookies)
                _driver.Manage().Cookies.AddCookie(cookie);
        }, true, destroyCancellationToken).Forget();
    }*/
    public void UpdateChatTask()
    {
        UniTask.RunOnThreadPool(async () =>
        {
            System.Environment.SetEnvironmentVariable("SE_MANAGER_PATH", Environment.CurrentDirectory + "\\Selenium\\selenium-manager.exe");
            //드라이버 초기화
            _driverService = ChromeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;
            _options = new ChromeOptions();
            _options.AddArgument("--ignore-certificate-errors");
            _options.AddArgument("--disable-infobars");
            _options.AddArgument("--disable-extensions");
            _options.AddArgument($"--app={InputChatUrl.text}");
            _options.AddArgument("disable-blink-features=AutomationControlled");
            _options.AddArgument($"user-data-dir={Environment.CurrentDirectory}\\Selenium\\UserData");
            //_options.PageLoadStrategy = PageLoadStrategy.Eager;
            if (isHeadless)
            {
                _options.AddArgument("--headless");
                //_options.AddArgument("--disable-gpu");
                _options.AddArgument("--window-size=1920,1080");
                _options.AddArgument("--disable-images");
                //_options.AddArgument("--blink-settings=imagesEnabled=false");
            }
            _driver = new ChromeDriver(_driverService, _options);

            IWebElement chatListElement = null;
            while (chatListElement == null)
            {
                try
                {
                    chatListElement = _driver.FindElement(By.XPath("//*[@id=\"page\"]/div/div[2]/div[6]/div[10]/div[1]/ul"));
                }
                catch (NoSuchElementException)
                {
                    _driver.Navigate().Refresh();
                }
            }

            string stringNaver = "naver";
            string stringYotube = "youtube";
            string stringAfreeca = "afreeca";
            string stringSoop = "soop";
            string stringTwitch = "twitch";
            string stringKakao = "kakao";

            string stringChat = "chat";
            string stringDonation = "donation";

            //치지직
            string stringChzzk = "chzzk";

            //유튜브
            string stringSponsor = "sponsor";
            string stringSuperchat = "superchat";

            //숲(아프리카)
            string stringStarballoon = "starballoon";
            string stringFollow = "follow";
            string stringSticker = "sticker";
            string stringChoco = "choco";
            string stringAdballoon = "adballoon";
            string stringPung = "pung";

            //트위치
            string stringCheer = "cheer";
            string stringSubscription = "subscription";

            //카카오
            string stringCookie = "cookie";

            string stringNim = ")님이 ";
            string stringLI = "LI";
            string stringClassName = "className";
            string stringDataId = "data-id";
            string stringOuterText = "outerText";
            string stringDataName = "data-name";

            string intChecker = @"\D";

            string userID;
            string[] classes;
            string platformlName;
            string contentInfo;
            string donationType;
            string outerText;
            string[] chatContent;
            string name;

            //string preChatClassName = string.Empty;
            int startIndex = 0;
            while (!destroyCancellationToken.IsCancellationRequested)
            {
                try
                {
                    var chatElementList = chatListElement.FindElements(By.TagName(stringLI));
                    int chatElementListCount = chatElementList.Count;

                    if (chatElementListCount <= 0)
                        continue;

                    /*int startIndex = 0;
                    if (preChatClassName != string.Empty)
                    {
                        for (startIndex = 0; startIndex < chatElementListCount; startIndex++)
                        {
                            string preSibling = chatElementList[startIndex].GetAttribute("className");
                            if (preChatClassName == preSibling)
                            {
                                startIndex++;
                                break;
                            }
                        }
                    }*/
                    for (int i = startIndex; i < chatElementListCount; i++)
                    {
                        startIndex++;
                        /*if (i == (chatElementListCount - 1))
                        {
                            preChatClassName = chatElementList[i].GetAttribute("className");
                            //Debug.Log($"{i} /{preChatClassName}");
                        }*/

                        userID = chatElementList[i].GetAttribute(stringDataId);

                        classes = chatElementList[i].GetAttribute(stringClassName).Split(Utils.stringSpace);
                        platformlName = classes[0];
                        contentInfo = classes[1];
                        donationType = classes[3];

                        outerText = chatElementList[i].GetAttribute(stringOuterText);
                        chatContent = outerText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                        name = chatElementList[i].GetAttribute(stringDataName);

                        if (contentInfo == stringChat)
                        {
                            OnChat($"{platformlName}{userID}!:{name}", chatContent[1]);
                        }
                        else if (contentInfo == stringDonation)
                        {
                            if (platformlName == stringNaver)
                            {
                                if (donationType == stringChzzk)
                                {
                                    OnDonation($"{platformlName}{userID}!:{name}", chatContent[1], int.Parse(Regex.Replace(chatContent[2], intChecker, string.Empty)));
                                }
                            }
                            else if (platformlName == stringYotube)
                            {
                                if (donationType == stringSponsor)
                                {
                                    onSubscription($"{platformlName}{userID}!:{name}", chatContent[2], 0);
                                }
                                else if (donationType == stringSuperchat)
                                {
                                    OnDonation($"{platformlName}{userID}!:{name}", chatContent[2], int.Parse(Regex.Replace(chatContent[1], intChecker, string.Empty)));
                                }
                            }
                            else if (platformlName == stringAfreeca || platformlName == stringSoop)
                            {
                                if (donationType == stringStarballoon)
                                {
                                    OnDonation($"{platformlName}{userID}!:{name}", chatContent[2], int.Parse(Regex.Replace(chatContent[0], intChecker, string.Empty)) * 10);
                                }
                                else if (donationType == stringFollow)
                                {
                                    onSubscription($"{platformlName}{userID}!:{name}", outerText, int.Parse(Regex.Replace(chatContent[1], intChecker, string.Empty)));
                                }
                                else if (donationType == stringSticker)
                                {
                                    OnDonation($"{platformlName}{userID}!:{name}", chatContent[2], int.Parse(Regex.Replace(chatContent[0], intChecker, string.Empty)) * 10);
                                }
                                else if (donationType == stringChoco)
                                {
                                    OnDonation($"{platformlName}{userID}!:{name}", chatContent[2], int.Parse(Regex.Replace(chatContent[0], intChecker, string.Empty)) * 10);
                                }
                                else if (donationType == stringAdballoon)
                                {
                                    OnDonation($"{platformlName}{userID}!:{name}", chatContent[2], int.Parse(Regex.Replace(chatContent[2], intChecker, string.Empty)) * 10);
                                }
                                else if (donationType == stringPung)
                                {
                                    OnDonation($"{platformlName}{userID}!:{name}", chatContent[1], int.Parse(Regex.Replace(chatContent[1], intChecker, string.Empty)) * 10);
                                }
                            }
                            else if (platformlName == stringTwitch)
                            {
                                if (donationType == stringCheer)
                                {
                                    OnDonation($"{platformlName}{userID}!:{name}", chatContent[2], int.Parse(Regex.Replace(chatContent[1], intChecker, string.Empty)));
                                }
                                else if (donationType == stringSubscription)
                                {
                                    onSubscription($"{platformlName}{userID}!:{name}", chatContent[1], int.Parse(Regex.Replace(chatContent[0].Split(stringNim)[1], intChecker, string.Empty)));
                                }
                            }
                            else if (platformlName == stringKakao)
                            {
                                if (donationType == stringCookie)
                                {
                                    OnDonation($"{platformlName}{userID}!:{name}", chatContent[2], int.Parse(Regex.Replace(chatContent[0], intChecker, string.Empty)) * 10);
                                }
                            }
                        }
                        await Utils.YieldCaches.UniTaskYield;
                    }
                }
                catch(WebDriverTimeoutException)
                {
                    continue;
                }
                catch(StaleElementReferenceException)
                {
                    chatListElement = _driver.FindElement(By.XPath("//*[@id=\"page\"]/div/div[2]/div[6]/div[10]/div[1]/ul"));
                    startIndex = 0;
                    continue;
                }
                finally
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
                }
            }
        }, true, destroyCancellationToken).Forget();
    }
    public void OnDestroy()
    {
        _driver?.Close();
        _driver?.Quit();
        foreach (Process P in Process.GetProcessesByName("chromedriver"))
            P.Kill();
        _driverService?.Dispose();
    }
}
