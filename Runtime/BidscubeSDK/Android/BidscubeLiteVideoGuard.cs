using System;
using UnityEngine;

namespace BidscubeSDK.Android
{
    /// <summary>
    /// Call from integrator code when optional video paths must fail fast in Lite builds.
    /// Core <c>com.bidscube.sdk</c> should use the same define guards for video APIs.
    /// </summary>
    public static class BidscubeLiteVideoGuard
    {
        public const string LiteModeMessage =
            "Bidscube video playback is disabled in LiteNoVideo build. Switch to FullWithVideo to use video/rewarded ads.";

#if BIDSCUBE_ANDROID_LITE_NO_VIDEO
        public static bool IsVideoBlocked => true;
#else
        public static bool IsVideoBlocked => false;
#endif

        /// <summary>Returns false in LiteNoVideo Android builds after logging; otherwise true.</summary>
        public static bool TryEnterVideoPath(Action<string> logError = null)
        {
#if BIDSCUBE_ANDROID_LITE_NO_VIDEO
            var msg = LiteModeMessage;
            if (logError != null)
                logError(msg);
            else
                Debug.LogError("[Bidscube] " + msg);
            return false;
#else
            return true;
#endif
        }
    }
}
