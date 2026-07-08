using System;
using System.Reflection;
using UnityEngine;

namespace BidscubeSDK.Mediation
{
    /// <summary>
    /// MAX-first rewarded helper. Optional direct SDK fallback is disabled by default so load/show stay on MAX mediation.
    /// Enable <see cref="EnableDirectSdkFallback"/> only for explicit QA/debug when product approves bypassing MAX priority.
    /// </summary>
    public static class AppLovinMaxRewardedBridge
    {
        private static MethodInfo _showRewardedVideoAdMethod;

        /// <summary>When true, <see cref="ShowRewarded"/> uses Bidscube SDK if MAX rewarded cannot be shown. Default is false (MAX-only).</summary>
        public static bool EnableDirectSdkFallback { get; set; } = false;

        /// <summary>Loads MAX rewarded when MAX is available; no-op otherwise.</summary>
        public static void LoadRewarded(string maxRewardedAdUnitId)
        {
            if (string.IsNullOrWhiteSpace(maxRewardedAdUnitId))
                return;
            if (!AppLovinMaxUnityReflection.IsMaxSdkAvailable)
                return;

            AppLovinMaxUnityReflection.TryLoadRewardedAd(maxRewardedAdUnitId);
        }

        /// <summary>
        /// Returns true when MAX rewarded can be shown for the given ad unit (plugin present, initialized, ad ready).
        /// </summary>
        public static bool IsMaxRewardedReady(string maxRewardedAdUnitId)
        {
            if (string.IsNullOrWhiteSpace(maxRewardedAdUnitId))
                return false;
            if (!AppLovinMaxUnityReflection.IsMaxSdkAvailable)
                return false;
            if (!AppLovinMaxUnityReflection.TryIsInitialized())
                return false;

            return AppLovinMaxUnityReflection.TryIsRewardedAdReady(maxRewardedAdUnitId);
        }

        /// <summary>
        /// Shows rewarded via MAX when ready; otherwise uses Bidscube SDK direct rewarded APIs when
        /// <see cref="EnableDirectSdkFallback"/> is true.
        /// </summary>
        /// <param name="maxRewardedAdUnitId">AppLovin MAX rewarded ad unit id.</param>
        /// <param name="bidscubePlacementId">Bidscube placement id for direct SDK fallback.</param>
        /// <param name="callback">Optional direct SDK callback (used only on fallback path).</param>
        /// <returns><c>true</c> if MAX show was attempted; <c>false</c> if direct SDK fallback ran or nothing ran.</returns>
        public static bool ShowRewarded(string maxRewardedAdUnitId, string bidscubePlacementId, IAdCallback callback = null)
        {
            if (IsMaxRewardedReady(maxRewardedAdUnitId))
            {
                AppLovinMaxUnityReflection.TryShowRewardedAd(maxRewardedAdUnitId);
                return true;
            }

            if (!EnableDirectSdkFallback)
            {
                Debug.LogWarning("[Bidscube AppLovin] MAX rewarded not ready and direct SDK fallback is disabled.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(bidscubePlacementId))
            {
                Debug.LogWarning("[Bidscube AppLovin] MAX rewarded not ready and bidscubePlacementId is empty — cannot fallback.");
                return false;
            }

            if (!BidscubeSDK.IsInitialized())
            {
                Debug.LogWarning("[Bidscube AppLovin] MAX rewarded not ready and Bidscube SDK is not initialized.");
                return false;
            }

            Debug.Log("[Bidscube AppLovin] MAX rewarded unavailable — falling back to Bidscube SDK direct rewarded.");
            ShowDirectRewardedVideo(bidscubePlacementId, callback);
            return false;
        }

        private static void ShowDirectRewardedVideo(string placementId, IAdCallback callback)
        {
            var rewardedMethod = ResolveShowRewardedVideoAdMethod();
            if (rewardedMethod != null)
            {
                rewardedMethod.Invoke(null, new object[] { placementId, callback });
                return;
            }

            BidscubeSDK.ShowVideoAd(placementId, callback);
        }

        private static MethodInfo ResolveShowRewardedVideoAdMethod()
        {
            if (_showRewardedVideoAdMethod != null)
                return _showRewardedVideoAdMethod;

            var sdkType = typeof(BidscubeSDK);
            _showRewardedVideoAdMethod = sdkType.GetMethod(
                "ShowRewardedVideoAd",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(IAdCallback) },
                null);

            return _showRewardedVideoAdMethod;
        }
    }
}
