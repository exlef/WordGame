using System.Collections;
using System.Linq;
using DTT.MinigameBase.Advertisements;
#if GOOGLE_MOBILE_ADS
using DTT.MinigameBase.Advertisements.AdMob;
using GoogleMobileAds.Api;
#endif
#if UNITY_ADS && UNITY_UNITYADS_API
using DTT.MinigameBase.Advertisements.UnityAds;
#endif
using UnityEngine;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Gates hint usage behind rewarded ad completion.
    /// </summary>
    public class RewardedHintService : MonoBehaviour
    {
        /// <summary>
        /// Supported rewarded hint types.
        /// </summary>
        private enum HintType
        {
            Letter,
            Word,
            Descriptive
        }

        /// <summary>
        /// Game manager that owns the hint methods.
        /// </summary>
        [SerializeField]
        [Tooltip("Game manager that owns the hint methods.")]
        private WordConnectManager _wordConnectManager;

        /// <summary>
        /// Rewarded ad component used before granting hints.
        /// </summary>
        [SerializeField]
        [Tooltip("Rewarded ad component used before granting hints.")]
        private BaseAd _rewardedAd;

        /// <summary>
        /// Modern LevelPlay rewarded ad component used before granting hints.
        /// </summary>
        [SerializeField]
        [Tooltip("Modern LevelPlay rewarded ad component used before granting hints.")]
        private LevelPlayRewardedHintAd _levelPlayRewardedAd;

        /// <summary>
        /// Time to wait before unlocking the service if an ad SDK never reports back.
        /// </summary>
        [SerializeField]
        [Min(1f)]
        [Tooltip("Time to wait before unlocking the service if an ad SDK never reports back.")]
        private float _adRequestTimeoutSeconds = 90f;

        /// <summary>
        /// Grace period after LevelPlay closes before treating the ad as unrewarded.
        /// </summary>
        [SerializeField]
        [Min(0.1f)]
        [Tooltip("Grace period after LevelPlay closes before treating the ad as unrewarded.")]
        private float _levelPlayCloseRewardGraceSeconds = 2f;

        /// <summary>
        /// Allows hint testing in the editor when no rewarded ad provider is attached.
        /// </summary>
        [SerializeField]
        [Tooltip("Allows hint testing in the editor when no rewarded ad provider is attached.")]
        private bool _allowEditorFallbackWithoutAd;

        /// <summary>
        /// The hint waiting for a rewarded ad.
        /// </summary>
        private HintType? _pendingHint;

        /// <summary>
        /// Whether the current pending ad granted a reward.
        /// </summary>
        private bool _rewardGranted;

        /// <summary>
        /// Timeout coroutine for the current ad request.
        /// </summary>
        private Coroutine _adTimeoutCoroutine;

        /// <summary>
        /// Grace coroutine for LevelPlay close-before-reward event ordering.
        /// </summary>
        private Coroutine _levelPlayCloseGraceCoroutine;

        /// <summary>
        /// Ad instance currently subscribed to.
        /// </summary>
        private BaseAd _registeredAd;

        /// <summary>
        /// LevelPlay ad instance currently subscribed to.
        /// </summary>
        private LevelPlayRewardedHintAd _registeredLevelPlayAd;

        /// <summary>
        /// Retrieves fallback references.
        /// </summary>
        private void Awake() => ResolveReferences();

        /// <summary>
        /// Adds ad event listeners.
        /// </summary>
        private void OnEnable()
        {
            ResolveReferences();
            AddRewardListeners();
        }

        /// <summary>
        /// Removes ad event listeners.
        /// </summary>
        private void OnDisable()
        {
            RemoveRewardListeners();
            RemoveLevelPlayRewardListeners();
            StopAdTimeout();
            StopLevelPlayCloseGrace();
            ClearPendingHint();
        }

        /// <summary>
        /// Requests a rewarded ad for a letter hint.
        /// </summary>
        public void RequestLetterHint() => RequestHint(HintType.Letter);

        /// <summary>
        /// Requests a rewarded ad for a word hint.
        /// </summary>
        public void RequestWordHint() => RequestHint(HintType.Word);

        /// <summary>
        /// Requests a rewarded ad for a descriptive hint.
        /// </summary>
        public void RequestDescriptiveHint() => RequestHint(HintType.Descriptive);

        /// <summary>
        /// Starts the rewarded ad flow for a hint.
        /// </summary>
        /// <param name="hintType">The hint type to grant after reward completion.</param>
        private void RequestHint(HintType hintType)
        {
            ResolveReferences();

            if (_pendingHint.HasValue || !CanUseHint(hintType))
                return;

            if (HasLevelPlayRewardedProvider())
            {
                AddLevelPlayRewardListeners();
                BeginPendingHint(hintType);
                _levelPlayRewardedAd.ShowRewardedAd();
                return;
            }

            if (!HasLegacyRewardedProvider())
            {
#if UNITY_EDITOR
                if (_allowEditorFallbackWithoutAd)
                {
                    ExecuteHint(hintType);
                    return;
                }
#endif

                Debug.LogWarning("Rewarded hint requested, but no supported rewarded ad component is attached. Configure LevelPlayRewardedHintAd, or enable editor fallback for local testing.");
                return;
            }

            AddRewardListeners();
            BeginPendingHint(hintType);
            _rewardedAd.LoadAd(true);
        }

        /// <summary>
        /// Retrieves missing references from the current game object.
        /// </summary>
        private void ResolveReferences()
        {
            if (_wordConnectManager == null)
                _wordConnectManager = GetComponent<WordConnectManager>();

            if (_rewardedAd == null)
                _rewardedAd = GetComponent<BaseAd>();

            if (_levelPlayRewardedAd == null)
                _levelPlayRewardedAd = GetComponent<LevelPlayRewardedHintAd>();
        }

        /// <summary>
        /// Checks whether the selected hint can currently be used.
        /// </summary>
        /// <param name="hintType">The hint type to check.</param>
        /// <returns>Whether the hint can be used.</returns>
        private bool CanUseHint(HintType hintType)
        {
            WordConnectState state = _wordConnectManager != null ? _wordConnectManager.WordConnectState : null;
            if (state == null)
                return false;

            switch (hintType)
            {
                case HintType.Letter:
                    return state.HintOptions.Count > 0;
                case HintType.Word:
                    return state.HintOptions.Count > 0;
                case HintType.Descriptive:
                    return state.CurrentRevealedDescriptiveHint == null &&
                           state.HintOptions.Any(hint => !hint.DescriptionRevealed);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks whether the attached ad is a rewarded provider.
        /// </summary>
        /// <returns>Whether a supported rewarded provider is attached.</returns>
        private bool HasLevelPlayRewardedProvider() =>
            _levelPlayRewardedAd != null && _levelPlayRewardedAd.IsAvailable;

        /// <summary>
        /// Checks whether the attached legacy ad is a rewarded provider.
        /// </summary>
        /// <returns>Whether a supported legacy rewarded provider is attached.</returns>
        private bool HasLegacyRewardedProvider()
        {
            if (_rewardedAd == null)
                return false;

#if UNITY_ADS && UNITY_UNITYADS_API
            if (_rewardedAd is UnityRewardAd)
                return true;
#endif

#if GOOGLE_MOBILE_ADS
            if (_rewardedAd is IAdMobRewardedAd)
                return true;
#endif

            return false;
        }

        /// <summary>
        /// Begins waiting for a rewarded ad to complete.
        /// </summary>
        /// <param name="hintType">The hint waiting for reward completion.</param>
        private void BeginPendingHint(HintType hintType)
        {
            _pendingHint = hintType;
            _rewardGranted = false;
            StartAdTimeout();
        }

        /// <summary>
        /// Grants the pending hint after rewarded completion.
        /// </summary>
        private void GrantPendingHint()
        {
            if (!_pendingHint.HasValue)
                return;

            HintType hintType = _pendingHint.Value;
            _rewardGranted = true;
            StopLevelPlayCloseGrace();
            ClearPendingHint();
            ExecuteHint(hintType);
        }

        /// <summary>
        /// Executes the requested hint on the game manager.
        /// </summary>
        /// <param name="hintType">The hint type to execute.</param>
        private void ExecuteHint(HintType hintType)
        {
            if (_wordConnectManager == null)
                return;

            switch (hintType)
            {
                case HintType.Letter:
                    _wordConnectManager.RevealRewardedLetterHint();
                    break;
                case HintType.Word:
                    _wordConnectManager.RevealRewardedWordHint();
                    break;
                case HintType.Descriptive:
                    _wordConnectManager.RevealRewardedDescriptiveHint();
                    break;
            }
        }

        /// <summary>
        /// Adds reward provider event listeners.
        /// </summary>
        private void AddRewardListeners()
        {
            if (_rewardedAd == null)
                return;

            if (_registeredAd == _rewardedAd)
                return;

            RemoveRewardListeners();

#if UNITY_ADS && UNITY_UNITYADS_API
            if (_rewardedAd is UnityRewardAd unityRewardAd)
            {
                unityRewardAd.onRewardComplete.AddListener(GrantPendingHint);
                unityRewardAd.onRewardFailed.AddListener(ClearPendingHint);
            }
#endif

#if GOOGLE_MOBILE_ADS
            if (_rewardedAd is IAdMobRewardedAd adMobRewardedAd)
                adMobRewardedAd.OnRewarded.AddListener(OnAdMobRewarded);
#endif

            if (_rewardedAd is ICompletable completable)
                completable.Completed += OnAdCompleted;

            _registeredAd = _rewardedAd;
        }

        /// <summary>
        /// Adds LevelPlay rewarded ad event listeners.
        /// </summary>
        private void AddLevelPlayRewardListeners()
        {
            if (_levelPlayRewardedAd == null)
                return;

            if (_registeredLevelPlayAd == _levelPlayRewardedAd)
                return;

            RemoveLevelPlayRewardListeners();

            _levelPlayRewardedAd.RewardGranted += GrantPendingHint;
            _levelPlayRewardedAd.Closed += OnLevelPlayAdClosed;
            _levelPlayRewardedAd.Failed += ClearPendingHint;
            _registeredLevelPlayAd = _levelPlayRewardedAd;
        }

        /// <summary>
        /// Removes reward provider event listeners.
        /// </summary>
        private void RemoveRewardListeners()
        {
            if (_registeredAd == null)
                return;

#if UNITY_ADS && UNITY_UNITYADS_API
            if (_registeredAd is UnityRewardAd unityRewardAd)
            {
                unityRewardAd.onRewardComplete.RemoveListener(GrantPendingHint);
                unityRewardAd.onRewardFailed.RemoveListener(ClearPendingHint);
            }
#endif

#if GOOGLE_MOBILE_ADS
            if (_registeredAd is IAdMobRewardedAd adMobRewardedAd)
                adMobRewardedAd.OnRewarded.RemoveListener(OnAdMobRewarded);
#endif

            if (_registeredAd is ICompletable completable)
                completable.Completed -= OnAdCompleted;

            _registeredAd = null;
        }

        /// <summary>
        /// Removes LevelPlay rewarded ad event listeners.
        /// </summary>
        private void RemoveLevelPlayRewardListeners()
        {
            if (_registeredLevelPlayAd == null)
                return;

            _registeredLevelPlayAd.RewardGranted -= GrantPendingHint;
            _registeredLevelPlayAd.Closed -= OnLevelPlayAdClosed;
            _registeredLevelPlayAd.Failed -= ClearPendingHint;
            _registeredLevelPlayAd = null;
        }

#if GOOGLE_MOBILE_ADS
        /// <summary>
        /// Grants a pending hint after AdMob reports an earned reward.
        /// </summary>
        /// <param name="reward">The earned ad reward.</param>
        private void OnAdMobRewarded(Reward reward) => GrantPendingHint();
#endif

        /// <summary>
        /// Clears a pending hint if the ad closed without a reward.
        /// </summary>
        private void OnAdCompleted()
        {
            if (!_rewardGranted)
                ClearPendingHint();
        }

        /// <summary>
        /// Waits briefly before clearing because LevelPlay reward and close callbacks are asynchronous.
        /// </summary>
        private void OnLevelPlayAdClosed()
        {
            StopLevelPlayCloseGrace();
            _levelPlayCloseGraceCoroutine = StartCoroutine(ClearAfterLevelPlayCloseGrace());
        }

        /// <summary>
        /// Starts the request timeout.
        /// </summary>
        private void StartAdTimeout()
        {
            StopAdTimeout();
            _adTimeoutCoroutine = StartCoroutine(AdTimeout());
        }

        /// <summary>
        /// Stops the request timeout.
        /// </summary>
        private void StopAdTimeout()
        {
            if (_adTimeoutCoroutine == null)
                return;

            StopCoroutine(_adTimeoutCoroutine);
            _adTimeoutCoroutine = null;
        }

        /// <summary>
        /// Stops the LevelPlay close grace coroutine.
        /// </summary>
        private void StopLevelPlayCloseGrace()
        {
            if (_levelPlayCloseGraceCoroutine == null)
                return;

            StopCoroutine(_levelPlayCloseGraceCoroutine);
            _levelPlayCloseGraceCoroutine = null;
        }

        /// <summary>
        /// Unlocks the hint service if the ad SDK never reports back.
        /// </summary>
        /// <returns>Coroutine enumerator.</returns>
        private IEnumerator AdTimeout()
        {
            yield return new WaitForSecondsRealtime(_adRequestTimeoutSeconds);

            if (_pendingHint.HasValue)
                ClearPendingHint();
        }

        /// <summary>
        /// Clears a pending LevelPlay hint if a reward did not arrive after close.
        /// </summary>
        /// <returns>Coroutine enumerator.</returns>
        private IEnumerator ClearAfterLevelPlayCloseGrace()
        {
            yield return new WaitForSecondsRealtime(_levelPlayCloseRewardGraceSeconds);
            _levelPlayCloseGraceCoroutine = null;

            if (_pendingHint.HasValue)
                ClearPendingHint();
        }

        /// <summary>
        /// Clears any pending rewarded hint request.
        /// </summary>
        private void ClearPendingHint()
        {
            StopAdTimeout();
            StopLevelPlayCloseGrace();
            _pendingHint = null;
            _rewardGranted = false;
        }
    }
}
