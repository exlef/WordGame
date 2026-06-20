using System;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Modern LevelPlay rewarded ad provider for hint rewards.
    /// </summary>
    public sealed class LevelPlayRewardedHintAd : MonoBehaviour
    {
        /// <summary>
        /// Invoked when LevelPlay grants the reward.
        /// </summary>
        public event Action RewardGranted;

        /// <summary>
        /// Invoked when the ad closes.
        /// </summary>
        public event Action Closed;

        /// <summary>
        /// Invoked when initialization, loading, or display fails.
        /// </summary>
        public event Action Failed;

        /// <summary>
        /// Android app key from the LevelPlay dashboard.
        /// </summary>
        [Header("LevelPlay App Keys")]
        [SerializeField]
        [Tooltip("Android app key from the LevelPlay dashboard.")]
        private string _androidAppKey;

        /// <summary>
        /// iOS app key from the LevelPlay dashboard.
        /// </summary>
        [SerializeField]
        [Tooltip("iOS app key from the LevelPlay dashboard.")]
        private string _iosAppKey;

        /// <summary>
        /// Android rewarded ad unit id from the LevelPlay dashboard.
        /// </summary>
        [Header("Rewarded Ad Units")]
        [SerializeField]
        [Tooltip("Android rewarded ad unit id from the LevelPlay dashboard.")]
        private string _androidRewardedAdUnitId;

        /// <summary>
        /// iOS rewarded ad unit id from the LevelPlay dashboard.
        /// </summary>
        [SerializeField]
        [Tooltip("iOS rewarded ad unit id from the LevelPlay dashboard.")]
        private string _iosRewardedAdUnitId;

        /// <summary>
        /// Optional placement name configured in LevelPlay.
        /// </summary>
        [Header("Options")]
        [SerializeField]
        [Tooltip("Optional placement name configured in LevelPlay.")]
        private string _placementName;

        /// <summary>
        /// Whether to preload a rewarded ad after initialization.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to preload a rewarded ad after initialization.")]
        private bool _preloadOnStart = true;

        /// <summary>
        /// Whether to ask LevelPlay to pause the game during ads.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to ask LevelPlay to pause the game during ads.")]
        private bool _pauseGameDuringAds = true;

        /// <summary>
        /// Whether to run LevelPlay integration validation before initialization.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to run LevelPlay integration validation before initialization.")]
        private bool _validateIntegration;

        /// <summary>
        /// Shared SDK initialized state for this process.
        /// </summary>
        private static bool _sdkInitialized;

        /// <summary>
        /// Shared SDK initializing state for this process.
        /// </summary>
        private static bool _sdkInitializing;

        /// <summary>
        /// Whether the global LevelPlay callbacks have been subscribed.
        /// </summary>
        private static bool _callbacksRegistered;

        /// <summary>
        /// Global successful initialization event.
        /// </summary>
        private static event Action SdkInitialized;

        /// <summary>
        /// Global failed initialization event.
        /// </summary>
        private static event Action SdkInitializationFailed;

        /// <summary>
        /// Runtime rewarded ad instance.
        /// </summary>
        private LevelPlayRewardedAd _rewardedAd;

        /// <summary>
        /// Whether rewarded ad callbacks are registered.
        /// </summary>
        private bool _rewardedAdCallbacksRegistered;

        /// <summary>
        /// Whether a rewarded ad load is in flight.
        /// </summary>
        private bool _adLoading;

        /// <summary>
        /// Whether the current request should show as soon as the ad is loaded.
        /// </summary>
        private bool _showWhenLoaded;

        /// <summary>
        /// Whether the current displayed ad granted the reward.
        /// </summary>
        private bool _rewardGranted;

        /// <summary>
        /// Whether this provider has enough configuration to request rewarded ads.
        /// </summary>
        public bool IsAvailable => !string.IsNullOrEmpty(GetAppKey()) && !string.IsNullOrEmpty(GetRewardedAdUnitId());

        /// <summary>
        /// Registers global callbacks and optionally preloads.
        /// </summary>
        private void OnEnable()
        {
            RegisterSdkCallbacks();
            SdkInitialized += OnSdkInitialized;
            SdkInitializationFailed += OnSdkInitializationFailed;

            if (_preloadOnStart && IsAvailable)
                LoadRewardedAd(false);
        }

        /// <summary>
        /// Releases callbacks.
        /// </summary>
        private void OnDisable()
        {
            SdkInitialized -= OnSdkInitialized;
            SdkInitializationFailed -= OnSdkInitializationFailed;
            UnregisterRewardedAdCallbacks();
        }

        /// <summary>
        /// Disposes the ad instance.
        /// </summary>
        private void OnDestroy()
        {
            UnregisterRewardedAdCallbacks();
            _rewardedAd?.DestroyAd();
            _rewardedAd = null;
        }

        /// <summary>
        /// Requests a rewarded ad and shows it when ready.
        /// </summary>
        public void ShowRewardedAd()
        {
            if (!IsAvailable)
            {
                Debug.LogWarning("LevelPlay rewarded hint ad is not configured. Fill app key and rewarded ad unit id on LevelPlayRewardedHintAd.");
                Failed?.Invoke();
                return;
            }

            LoadRewardedAd(true);
        }

        /// <summary>
        /// Starts SDK initialization or loads/shows when already initialized.
        /// </summary>
        /// <param name="showWhenLoaded">Whether to show as soon as loading completes.</param>
        private void LoadRewardedAd(bool showWhenLoaded)
        {
            _showWhenLoaded |= showWhenLoaded;

            if (!EnsureSdkInitialized())
                return;

            EnsureRewardedAd();
            ShowIfReadyOrLoad();
        }

        /// <summary>
        /// Creates the rewarded ad object.
        /// </summary>
        private void EnsureRewardedAd()
        {
            if (_rewardedAd == null)
                _rewardedAd = new LevelPlayRewardedAd(GetRewardedAdUnitId());

            if (_rewardedAdCallbacksRegistered)
                return;

            _rewardedAd.OnAdLoaded += OnRewardedAdLoaded;
            _rewardedAd.OnAdLoadFailed += OnRewardedAdLoadFailed;
            _rewardedAd.OnAdDisplayFailed += OnRewardedAdDisplayFailed;
            _rewardedAd.OnAdRewarded += OnRewardedAdRewarded;
            _rewardedAd.OnAdClosed += OnRewardedAdClosed;
            _rewardedAdCallbacksRegistered = true;
        }

        /// <summary>
        /// Removes rewarded ad callbacks.
        /// </summary>
        private void UnregisterRewardedAdCallbacks()
        {
            if (_rewardedAd == null)
                return;

            _rewardedAd.OnAdLoaded -= OnRewardedAdLoaded;
            _rewardedAd.OnAdLoadFailed -= OnRewardedAdLoadFailed;
            _rewardedAd.OnAdDisplayFailed -= OnRewardedAdDisplayFailed;
            _rewardedAd.OnAdRewarded -= OnRewardedAdRewarded;
            _rewardedAd.OnAdClosed -= OnRewardedAdClosed;
            _rewardedAdCallbacksRegistered = false;
        }

        /// <summary>
        /// Initializes the LevelPlay SDK if needed.
        /// </summary>
        /// <returns>Whether the SDK is already initialized.</returns>
        private bool EnsureSdkInitialized()
        {
            RegisterSdkCallbacks();

            if (_sdkInitialized)
                return true;

            if (_sdkInitializing)
                return false;

            if (_validateIntegration)
                LevelPlay.ValidateIntegration();

            LevelPlay.SetPauseGame(_pauseGameDuringAds);
            _sdkInitializing = true;
            LevelPlay.Init(GetAppKey());
            return false;
        }

        /// <summary>
        /// Shows a ready ad or loads a new one.
        /// </summary>
        private void ShowIfReadyOrLoad()
        {
            if (_rewardedAd.IsAdReady())
            {
                _showWhenLoaded = false;
                _rewardGranted = false;
                _rewardedAd.ShowAd(string.IsNullOrEmpty(_placementName) ? null : _placementName);
                return;
            }

            if (_adLoading)
                return;

            _adLoading = true;
            _rewardedAd.LoadAd();
        }

        /// <summary>
        /// Handles SDK initialization success.
        /// </summary>
        private void OnSdkInitialized()
        {
            if (!IsAvailable)
                return;

            if (_rewardedAd == null)
                EnsureRewardedAd();

            ShowIfReadyOrLoad();
        }

        /// <summary>
        /// Handles SDK initialization failure.
        /// </summary>
        private void OnSdkInitializationFailed()
        {
            _showWhenLoaded = false;
            Failed?.Invoke();
        }

        /// <summary>
        /// Handles rewarded load success.
        /// </summary>
        /// <param name="adInfo">LevelPlay ad info.</param>
        private void OnRewardedAdLoaded(LevelPlayAdInfo adInfo)
        {
            _adLoading = false;

            if (_showWhenLoaded)
                ShowIfReadyOrLoad();
        }

        /// <summary>
        /// Handles rewarded load failure.
        /// </summary>
        /// <param name="error">LevelPlay ad error.</param>
        private void OnRewardedAdLoadFailed(LevelPlayAdError error)
        {
            _adLoading = false;
            _showWhenLoaded = false;
            Debug.LogWarning($"LevelPlay rewarded hint ad failed to load: {error}");
            Failed?.Invoke();
        }

        /// <summary>
        /// Handles rewarded display failure.
        /// </summary>
        /// <param name="adInfo">LevelPlay ad info.</param>
        /// <param name="error">LevelPlay ad error.</param>
        private void OnRewardedAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            _showWhenLoaded = false;
            Debug.LogWarning($"LevelPlay rewarded hint ad failed to display: {error}");
            Failed?.Invoke();
            LoadRewardedAd(false);
        }

        /// <summary>
        /// Handles rewarded completion.
        /// </summary>
        /// <param name="adInfo">LevelPlay ad info.</param>
        /// <param name="reward">LevelPlay reward.</param>
        private void OnRewardedAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
        {
            _rewardGranted = true;
            RewardGranted?.Invoke();
        }

        /// <summary>
        /// Handles rewarded ad close.
        /// </summary>
        /// <param name="adInfo">LevelPlay ad info.</param>
        private void OnRewardedAdClosed(LevelPlayAdInfo adInfo)
        {
            _rewardGranted = false;
            Closed?.Invoke();

            if (_preloadOnStart)
                LoadRewardedAd(false);
        }

        /// <summary>
        /// Registers shared LevelPlay SDK callbacks.
        /// </summary>
        private static void RegisterSdkCallbacks()
        {
            if (_callbacksRegistered)
                return;

            LevelPlay.OnInitSuccess += OnLevelPlayInitSuccess;
            LevelPlay.OnInitFailed += OnLevelPlayInitFailed;
            _callbacksRegistered = true;
        }

        /// <summary>
        /// Handles global LevelPlay initialization success.
        /// </summary>
        /// <param name="configuration">LevelPlay configuration.</param>
        private static void OnLevelPlayInitSuccess(LevelPlayConfiguration configuration)
        {
            _sdkInitialized = true;
            _sdkInitializing = false;
            SdkInitialized?.Invoke();
        }

        /// <summary>
        /// Handles global LevelPlay initialization failure.
        /// </summary>
        /// <param name="error">LevelPlay init error.</param>
        private static void OnLevelPlayInitFailed(LevelPlayInitError error)
        {
            _sdkInitializing = false;
            Debug.LogWarning($"LevelPlay initialization failed: {error}");
            SdkInitializationFailed?.Invoke();
        }

        /// <summary>
        /// Gets the platform app key.
        /// </summary>
        /// <returns>Platform app key.</returns>
        private string GetAppKey()
        {
#if UNITY_IOS
            return _iosAppKey;
#elif UNITY_ANDROID
            return _androidAppKey;
#else
            return string.IsNullOrEmpty(_androidAppKey) ? _iosAppKey : _androidAppKey;
#endif
        }

        /// <summary>
        /// Gets the platform rewarded ad unit id.
        /// </summary>
        /// <returns>Platform rewarded ad unit id.</returns>
        private string GetRewardedAdUnitId()
        {
#if UNITY_IOS
            return _iosRewardedAdUnitId;
#elif UNITY_ANDROID
            return _androidRewardedAdUnitId;
#else
            return string.IsNullOrEmpty(_androidRewardedAdUnitId) ? _iosRewardedAdUnitId : _androidRewardedAdUnitId;
#endif
        }
    }
}
