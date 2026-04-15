#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;

namespace BidscubeSDK
{
    /// <summary>
    /// Mirrors native Android <c>com.bidscube.sdk.BidscubeSDK</c> so AppLovin MAX mediation shares the same Java instance as Unity after <see cref="BidscubeSDK.Initialize(SDKConfig)"/>.
    /// </summary>
    internal static class BidscubeAndroidSdkInterop
    {
        private const string SdkClass = "com.bidscube.sdk.BidscubeSDK";
        private const string BuilderClass = "com.bidscube.sdk.config.SDKConfig$Builder";
        private const string UnityPlayerClass = "com.unity3d.player.UnityPlayer";

        internal static void SyncInitializeFromUnityConfig()
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass(UnityPlayerClass))
                {
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (activity == null)
                    {
                        Logger.Warning("Init (Android Java): skipped — Unity currentActivity is null (too early?). Retry Initialize after Activity is ready.");
                        return;
                    }

                    var app = activity.Call<AndroidJavaObject>("getApplicationContext");
                    using (var sdk = new AndroidJavaClass(SdkClass))
                    {
                        if (sdk.CallStatic<bool>("isInitialized"))
                        {
                            sdk.CallStatic("setActivity", activity);
                            Logger.Info("Init (Android Java): native SDK was already initialized; setActivity(current) applied. MAX adapter can use this instance.");
                            return;
                        }
                    }

                    var cfg = BidscubeSDK.ActiveConfiguration;
                    if (cfg == null)
                    {
                        Logger.Warning("Init (Android Java): skipped — no ActiveConfiguration (internal).");
                        return;
                    }

                    Logger.Info("Init (Android Java): invoking com.bidscube.sdk.BidscubeSDK.initialize(Application, SDKConfig) …");

                    using (var builder = new AndroidJavaObject(BuilderClass, app))
                    {
                        builder.Call<AndroidJavaObject>("enableLogging", cfg.EnableLogging);
                        builder.Call<AndroidJavaObject>("enableDebugMode", cfg.EnableDebugMode);
                        builder.Call<AndroidJavaObject>("defaultAdTimeout", cfg.DefaultAdTimeoutMs);
                        builder.Call<AndroidJavaObject>("defaultAdPosition", MapAdPositionToJavaEnumName(cfg.DefaultAdPosition));
                        TryApplyOptionalBuilderMethods(builder, cfg);

                        using (var javaConfig = builder.Call<AndroidJavaObject>("build"))
                        {
                            using (var sdk = new AndroidJavaClass(SdkClass))
                            {
                                sdk.CallStatic("initialize", app, javaConfig);
                            }
                        }
                    }

                    using (var sdk = new AndroidJavaClass(SdkClass))
                    {
                        sdk.CallStatic("setActivity", activity);
                        if (sdk.CallStatic<bool>("isInitialized"))
                        {
                            Logger.Info(
                                "Init (Android Java): SUCCESS — native com.bidscube.sdk.BidscubeSDK.isInitialized()==true. " +
                                "Other log lines (BidscubeSDK / BidscubeSDKImpl) may appear from the native SDK; this line confirms Unity-driven init completed.");
                        }
                        else
                        {
                            Logger.InfoError(
                                "Init (Android Java): native initialize() returned but isInitialized()==false — often duplicate com.bidscube:bidscube-sdk on the classpath (e.g. manual implementation line + postprocessor). Keep a single core SDK source (Maven " +
                                Constants.NativeAndroidBidscubeSdkVersion + " per Gradle inject).");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.InfoError($"Init (Android Java): FAILED — {e.GetType().Name}: {e.Message}");
            }
        }

        /// <summary>Parity with Flutter / Android: forward <c>adRequestAuthority</c>, legacy base URL aliases, and test flags.</summary>
        private static void TryApplyOptionalBuilderMethods(AndroidJavaObject builder, SDKConfig cfg)
        {
            var auth = cfg.AdRequestAuthority?.Trim();
            if (!string.IsNullOrEmpty(auth))
            {
                if (!TryInvokeBuilderReturnsBuilder(builder, new[] { "adRequestAuthority", "setAdRequestAuthority" }, auth))
                {
                    var baseUrl = cfg.BaseURL?.Trim();
                    if (!string.IsNullOrEmpty(baseUrl) &&
                        !TryInvokeBuilderReturnsBuilder(builder, new[] { "baseURL", "setBaseUrl", "setBaseURL" }, baseUrl))
                    {
                        Logger.Warning(
                            "[BidscubeAndroidSdkInterop] SDKConfig.Builder has no adRequestAuthority/baseURL setter; C# authority not applied on this native SDK version.");
                    }
                }
            }

            if (cfg.EnableTestMode &&
                !TryInvokeBuilderBoolReturnsBuilder(builder, new[] { "enableTestMode", "setEnableTestMode", "setTestMode" }, true))
            {
                Logger.Warning(
                    "[BidscubeAndroidSdkInterop] SDKConfig.Builder has no enableTestMode setter; EnableTestMode ignored on this native SDK version.");
            }
        }

        private static bool TryInvokeBuilderReturnsBuilder(AndroidJavaObject builder, string[] methodNames, string arg)
        {
            foreach (var name in methodNames)
            {
                try
                {
                    builder.Call(name, arg);
                    return true;
                }
                catch (Exception)
                {
                    // try next alias
                }
            }

            return false;
        }

        private static bool TryInvokeBuilderBoolReturnsBuilder(AndroidJavaObject builder, string[] methodNames, bool arg)
        {
            foreach (var name in methodNames)
            {
                try
                {
                    builder.Call(name, arg);
                    return true;
                }
                catch (Exception)
                {
                }
            }

            return false;
        }

        private static string MapAdPositionToJavaEnumName(AdPosition p)
        {
            switch (p)
            {
                case AdPosition.AboveTheFold: return "ABOVE_THE_FOLD";
                case AdPosition.DependOnScreenSize: return "MAYBE_DEPENDING_ON_SCREEN_SIZE";
                case AdPosition.BelowTheFold: return "BELOW_THE_FOLD";
                case AdPosition.Header: return "HEADER";
                case AdPosition.Footer: return "FOOTER";
                case AdPosition.Sidebar: return "SIDEBAR";
                case AdPosition.FullScreen: return "FULL_SCREEN";
                default: return "UNKNOWN";
            }
        }
    }
}
#endif
