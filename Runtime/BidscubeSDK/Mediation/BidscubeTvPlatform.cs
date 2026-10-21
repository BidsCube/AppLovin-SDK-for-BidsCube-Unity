using System;
using UnityEngine;

namespace BidscubeSDK.Mediation
{
    /// <summary>
    /// Android TV / Fire TV and Apple TV detection. Feature flags match
    /// <c>TelevisionDetector</c> in the bundled Android SDK (same rules as direct BidsCube SDK).
    /// </summary>
    public static class BidscubeTvPlatform
    {
        public static bool IsAppleTv
        {
            get
            {
#if UNITY_TVOS
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsAndroidTv
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (!_androidChecked)
                {
                    _androidChecked = true;
                    _androidTv = QueryAndroidTelevision();
                }
                return _androidTv;
#else
                return false;
#endif
            }
        }

        public static bool IsTelevision => IsAppleTv || IsAndroidTv;

        /// <summary>Siri Remote Menu and Android Back.</summary>
        public static bool WasMenuPressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return false;
#else
            if (Input.GetKeyDown(KeyCode.Escape))
                return true;
            if (Input.GetKeyDown(KeyCode.JoystickButton0))
                return true;
            try
            {
                return Input.GetButtonDown("Cancel");
            }
            catch (Exception)
            {
                return false;
            }
#endif
        }

        /// <summary>Same decision as native <c>TelevisionDetector.detect</c>.</summary>
        public static bool Detect(
            bool hasLeanback,
            bool hasTelevisionFeature,
            bool hasFireTvFeature,
            bool uiModeTelevision,
            string manufacturer,
            string model)
        {
            if (hasLeanback || hasTelevisionFeature || hasFireTvFeature || uiModeTelevision)
                return true;
            if (string.IsNullOrWhiteSpace(manufacturer) || string.IsNullOrWhiteSpace(model))
                return false;
            return manufacturer.Trim().Equals("amazon", StringComparison.OrdinalIgnoreCase)
                && model.Trim().StartsWith("AFT", StringComparison.OrdinalIgnoreCase);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static bool _androidChecked;
        private static bool _androidTv;

        private static bool QueryAndroidTelevision()
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (activity == null)
                        return false;

                    bool leanback = false;
                    bool television = false;
                    bool fireTv = false;
                    bool uiTelevision = false;
                    using (var pm = activity.Call<AndroidJavaObject>("getPackageManager"))
                    {
                        if (pm != null)
                        {
                            leanback = pm.Call<bool>("hasSystemFeature", "android.software.leanback");
                            television = pm.Call<bool>("hasSystemFeature", "android.hardware.type.television");
                            fireTv = pm.Call<bool>("hasSystemFeature", "amazon.hardware.fire_tv");
                        }
                    }

                    using (var ui = activity.Call<AndroidJavaObject>("getSystemService", "uimode"))
                    {
                        uiTelevision = ui != null && ui.Call<int>("getCurrentModeType") == 4;
                    }

                    string manufacturer = null;
                    string model = null;
                    using (var build = new AndroidJavaClass("android.os.Build"))
                    {
                        manufacturer = build.GetStatic<string>("MANUFACTURER");
                        model = build.GetStatic<string>("MODEL");
                    }

                    return Detect(leanback, television, fireTv, uiTelevision, manufacturer, model);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Bidscube AppLovin] TV detect failed: " + e.Message);
                return false;
            }
        }
#endif
    }
}
