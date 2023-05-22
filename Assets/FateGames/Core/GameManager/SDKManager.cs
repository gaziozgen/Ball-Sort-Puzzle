using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using UnityEngine;
using GameAnalyticsSDK;
using Facebook.Unity;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using UnityEngine.Events;
using System.Threading.Tasks;
using FateGames.Core;
public class SDKManager : MonoBehaviour
{
    private static SDKManager instance = null;
    public static SDKManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<SDKManager>();
            return instance;
        }
    }
    [SerializeField] private SaveDataVariable saveData;
    [Header("AD SETTINGS")]
    [SerializeField] float defaultTimeIntervalBetweenInterstitial = 60;
    [SerializeField] public float defaultFirstInterstitialTime = 60;
    [SerializeField] public float defaultGraceTime = 180;
    private float lastInterstitialShowTime = float.MinValue;
    float timeIntervalBetweenInterstitial => Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("time_interval_between_interstitial").LongValue;
    float firstInterstitialTime => Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("first_interstitial_time").LongValue;
    float graceTime => Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("grace_time").LongValue;
    public bool canShowInterstitial => Time.time >= lastInterstitialShowTime + timeIntervalBetweenInterstitial && Time.time >= firstInterstitialTime && graceTime <= GameManager.Instance.totalPlaytime;
    [SerializeField] private UnityEvent onInitialized;
    private bool facebookInitialized = false;
    private bool interstitialInitialized = false;
    private bool bannerInitialized = false;
    private bool rewardedInitialized = false;
    private bool isFirebaseInitialized = false;
    private bool isRemoteConfigInitialized = false;
    private Action onRewardedAdSucceed;
    private Action onRewardedAdFailed;
    private bool rewardedAdSucceed = false;
    private static bool isInitialized = false;
#if UNITY_STANDALONE || UNITY_IOS
    private const string BannerAdUnitId = "fdda5b43d1149b6a";
    private const string InterstitialAdUnitId = "5e77c66e430b532d";
    private const string RewardedAdUnitId = "4dea8718e5ae7b39";
    
#endif



#if UNITY_ANDROID


    private const string BannerAdUnitId = "1a4bf753219a3631";
    private const string InterstitialAdUnitId = "ee144065a5d47b57";
    private const string RewardedAdUnitId = "3fe3ed116eec3fd3";

#endif






    private int interstitialRetryAttempt;
    private int rewardedRetryAttempt;

    private bool isBannerShowing = false;


    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;


    private void Awake()
    {
        if (isInitialized)
        {
            Destroy(this);
            return;
        }
        isInitialized = true;
        // Firebase SDK is initialized
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {

                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                Debug.Log("Enabling firebase data collection.");
            }
            else
            {
                Debug.LogError(
                    "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
            InitializeFirebase();
        });



        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            // AppLovin SDK is initialized
            TenjinConnect();

            //Gameanalytics is initialized
            GameAnalytics.Initialize();

            //FB SDK initialized
            initFB();

            //Start loading ads
            InitializeBannerAds();
            InitializeInterstitialAds();
            InitializeRewardedAds();


        };

        MaxSdk.SetSdkKey("09mBPe6fn7Tg_xo6p4-shNiAaXlBrtK4zAFXmPKNwdK3df-td8R7o5CgUWUpH3LQb2Mxxmp8AKngmcXgROmQJV");
        MaxSdk.SetUserId("USER_ID");
        MaxSdk.InitializeSdk();

        //MaxSdk.ShowMediationDebugger();

        IEnumerator waitForInitializations()
        {
            yield return new WaitUntil(() =>
            {
                return isFirebaseInitialized && isRemoteConfigInitialized && facebookInitialized/* && bannerInitialized */;
            });
            onInitialized.Invoke();
        }

        StartCoroutine(waitForInitializations());
    }
    void InitializeFirebase()
    {
        // [START set_defaults]
        System.Collections.Generic.Dictionary<string, object> defaults =
          new System.Collections.Generic.Dictionary<string, object>();

        // These are the values that are used if we haven't fetched data from the
        // server
        // yet, or if we ask for values that the server doesn't have:
        defaults.Add("first_interstitial_time", defaultFirstInterstitialTime);
        defaults.Add("time_interval_between_interstitial", defaultTimeIntervalBetweenInterstitial);
        defaults.Add("grace_time", defaultGraceTime);

        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
          .ContinueWithOnMainThread(task =>
          {
              // [END set_defaults]
              Debug.Log("RemoteConfig configured and ready!");

              isFirebaseInitialized = true;
              FetchDataAsync();
          });
    }

    public Task FetchDataAsync()
    {
        Debug.Log("Fetching data...");
        System.Threading.Tasks.Task fetchTask =
        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
            TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }
    //[END fetch_async]

    void FetchComplete(Task fetchTask)
    {
        if (fetchTask.IsCanceled)
        {
            Debug.Log("Fetch canceled.");
        }
        else if (fetchTask.IsFaulted)
        {
            Debug.Log("Fetch encountered an error.");
        }
        else if (fetchTask.IsCompleted)
        {
            Debug.Log("Fetch completed successfully!");
        }

        var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
        switch (info.LastFetchStatus)
        {
            case Firebase.RemoteConfig.LastFetchStatus.Success:
                Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                .ContinueWithOnMainThread(task =>
                {
                    Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).",
                               info.FetchTime));
                });

                break;
            case Firebase.RemoteConfig.LastFetchStatus.Failure:
                switch (info.LastFetchFailureReason)
                {
                    case Firebase.RemoteConfig.FetchFailureReason.Error:
                        Debug.Log("Fetch failed for unknown reason");
                        break;
                    case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                        Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                        break;
                }
                break;
            case Firebase.RemoteConfig.LastFetchStatus.Pending:
                Debug.Log("Latest Fetch call still pending.");
                break;
        }
        isRemoteConfigInitialized = true;
    }




    public void TenjinConnect()
    {
        BaseTenjin instance = Tenjin.getInstance("VSGFVVVMXTFLPXSB64TTEDC6O5Q2YQKJ");

        // Sends install/open event to Tenjin
        instance.Connect();
    }



    #region FB SDK Methods


    public void initFB()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }

    }




    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            facebookInitialized = true;
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }


    #endregion

    #region Interstitial Ad Methods

    private void InitializeInterstitialAds()
    {
        // Attach callbacks
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;


        // Load the first interstitial
        LoadInterstitial();
        //InitializeRewardedAds();
    }

    void LoadInterstitial()
    {
        Debug.Log("Loading...");
        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
    }

    public void ShowInterstitial()
    {
        Debug.Log("ShowInterstitial");
        if (MaxSdk.IsInterstitialReady(InterstitialAdUnitId) && canShowInterstitial)
        {
            Debug.Log("Showing");
            MaxSdk.ShowInterstitial(InterstitialAdUnitId);
            Firebase.Analytics.FirebaseAnalytics.LogEvent("INTWatched");
        }
        else
        {
            Debug.Log("Ad not ready");

        }
    }







    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'

        Debug.Log("Interstitial loaded");

        // Reset retry attempt
        interstitialRetryAttempt = 0;
        interstitialInitialized = true;
    }

    private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        interstitialRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));


        Debug.Log("Interstitial failed to load with error code: " + errorInfo.Code);

        Invoke("LoadInterstitial", (float)retryDelay);
        interstitialInitialized = true;
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. We recommend loading the next ad
        Debug.Log("Interstitial failed to display with error code: " + errorInfo.Code);
        LoadInterstitial();
    }

    private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {

        lastInterstitialShowTime = Time.time;
        // Interstitial ad is hidden. Pre-load the next ad
        Debug.Log("Interstitial dismissed");
        LoadInterstitial();
    }






    #endregion

    #region Rewarded Ad Methods

    private void InitializeRewardedAds()
    {
        // Attach callbacks
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;


        // Load the first RewardedAd
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        Debug.Log("Loading...");
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }

    public void ShowRewardedAd(Action onFailed, Action onSucceed)
    {
        if (MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
        {
            onRewardedAdSucceed = onSucceed;
            onRewardedAdFailed = onFailed;
            Debug.Log("Showing...");
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);

        }
        else
        {
            Debug.Log("Ad not ready...");

        }
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'

        Debug.Log("Rewarded ad loaded");

        // Reset retry attempt
        rewardedRetryAttempt = 0;
        rewardedInitialized = true;
    }

    private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        rewardedRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));


        Debug.Log("Rewarded ad failed to load with error code: " + errorInfo.Code);

        Invoke("LoadRewardedAd", (float)retryDelay);
        rewardedInitialized = true;
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. We recommend loading the next ad
        Debug.Log("Rewarded ad failed to display with error code: " + errorInfo.Code);
        LoadRewardedAd();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad displayed");
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad clicked");
    }

    private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        Debug.Log("Rewarded ad dismissed");
        lastInterstitialShowTime = Time.time;
        if (!rewardedAdSucceed && onRewardedAdFailed != null)
            onRewardedAdFailed.Invoke();
        else if (rewardedAdSucceed && onRewardedAdSucceed != null)
            onRewardedAdSucceed.Invoke();
        onRewardedAdSucceed = null;
        onRewardedAdFailed = null;
        rewardedAdSucceed = false;
        LoadRewardedAd();
    }


    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad was displayed and user should receive the reward
        Debug.Log("Rewarded ad received reward");
        rewardedAdSucceed = true;
    }


    #endregion

    #region Banner Ad Methods

    private void InitializeBannerAds()
    {
        Debug.Log("InitializeBannerAds");
        // Attach Callbacks
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;


        // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
        // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
        MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

        // Set background or background color for banners to be fully functional.
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.white);
    }
    /*
    public void ToggleBannerVisibility()
    {

        if (Convert.ToBoolean(PlayerPrefs.GetInt("hasPurchasedRemoveAds", 0)))
        {

            Debug.Log("ADS REMOVED");
        }
        else
        {
            if (!isBannerShowing)
            {
                MaxSdk.ShowBanner(BannerAdUnitId);

            }
            else
            {
                MaxSdk.HideBanner(BannerAdUnitId);

            }
            isBannerShowing = !isBannerShowing;
        }
    }
    */
    public void ShowBannerAd()
    {
        Debug.Log("ShowBannerAd");
        MaxSdk.ShowBanner(BannerAdUnitId);
    }

    public void HideBannerAd()
    {
        Debug.Log("HideBannerAd");
        MaxSdk.HideBanner(BannerAdUnitId);
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad is ready to be shown.
        // If you have already called MaxSdk.ShowBanner(BannerAdUnitId) it will automatically be shown on the next ad refresh.
        Debug.Log("Banner ad loaded");
        bannerInitialized = true;
        ShowBannerAd();
    }

    private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Banner ad failed to load. MAX will automatically try loading a new ad internally.
        Debug.Log("Banner ad failed to load with error code: " + errorInfo.Code);
        bannerInitialized = true;
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Banner ad clicked");
    }


    #endregion


}//CLASS KAPANIŞI
