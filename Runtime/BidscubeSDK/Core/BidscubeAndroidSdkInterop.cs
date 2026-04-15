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

        /// <summary>Outcome of the last <see cref="SyncInitializeFromUnityConfig"/> run (Android player only).</summary>
        internal enum AndroidJavaInitPublisherState
        {
            Pending,
            SkippedNullActivity,
            SkippedNoActiveConfig,
            ReusedNativeAlreadyInitialized,
            NativeFreshInitializeSuccess,
            NativeFreshInitializeIncomplete,
            NativeJarMissingUnityCreativesOk,
            ExceptionDuringSync,
        }

        internal static AndroidJavaInitPublisherState LastPublisherState { get; private set; } = AndroidJavaInitPublisherState.Pending;

        internal static string FormatPublisherChecklistLine()
        {
            const string filter = "logcat filter: [BidscubeSDK] Init";
            switch (LastPublisherState)
            {
                case AndroidJavaInitPublisherState.SkippedNullActivity:
                    return $"AndroidJava=SKIPPED (Unity currentActivity null — too early?). Retry Initialize next frame. {filter}";
                case AndroidJavaInitPublisherState.SkippedNoActiveConfig:
                    return $"AndroidJava=SKIPPED (internal). {filter}";
                case AndroidJavaInitPublisherState.ReusedNativeAlreadyInitialized:
                    return $"AndroidJava=OK (native was already initialized; setActivity applied). {filter}";
                case AndroidJavaInitPublisherState.NativeFreshInitializeSuccess:
                    return $"AndroidJava=OK (native BidscubeSDK.initialize completed; isInitialized()==true). AppLovin MAX: call C# BidscubeSDK.Initialize before MaxSdk.InitializeSdk. {filter}";
                case AndroidJavaInitPublisherState.NativeFreshInitializeIncomplete:
                    return "AndroidJava=INCOMPLETE (initialize returned but isInitialized()==false). Remove duplicate com.bidscube:bidscube-sdk lines in Gradle (postprocessor + manual). " + filter;
                case AndroidJavaInitPublisherState.NativeJarMissingUnityCreativesOk:
                    return $"AndroidJava=JAR_MISSING (ClassNotFoundException). Unity C# ads still work; add native AAR/Maven for MAX parity. {filter}";
                case AndroidJavaInitPublisherState.ExceptionDuringSync:
                    return $"AndroidJava=FAILED (see Init (Android Java): FAILED line above). {filter}";
                default:
                    return $"AndroidJava=NOT_RUN. {filter}";
            }
        }

        internal static void SyncInitializeFromUnityConfig()
        {
            LastPublisherState = AndroidJavaInitPublisherState.Pending;
            try
            {
                using (var unityPlayer = new AndroidJavaClass(UnityPlayerClass))
                {
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (activity == null)
                    {
                        LastPublisherState = AndroidJavaInitPublisherState.SkippedNullActivity;
                        Logger.Warning("Init (Android Java): skipped — Unity currentActivity is null (too early?). Retry Initialize after Activity is ready.");
                        return;
                    }

                    var app = activity.Call<AndroidJavaObject>("getApplicationContext");
                    using (var sdk = new AndroidJavaClass(SdkClass))
                    {
                        if (sdk.CallStatic<bool>("isInitialized"))
                        {
                            sdk.CallStatic("setActivity", activity);
                            LastPublisherState = AndroidJavaInitPublisherState.ReusedNativeAlreadyInitialized;
                            Logger.Info("Init (Android Java): native SDK was already initialized; setActivity(current) applied. MAX adapter can use this instance.");
                            return;
                        }
                    }

                    var cfg = BidscubeSDK.ActiveConfiguration;
                    if (cfg == null)
                    {
                        LastPublisherState = AndroidJavaInitPublisherState.SkippedNoActiveConfig;
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
                            LastPublisherState = AndroidJavaInitPublisherState.NativeFreshInitializeSuccess;
                            Logger.Info(
                                "Init (Android Java): SUCCESS — native com.bidscube.sdk.BidscubeSDK.isInitialized()==true. " +
                                "Other log lines (BidscubeSDK / BidscubeSDKImpl) may appear from the native SDK; this line confirms Unity-driven init completed.");
                        }
                        else
                        {
                            LastPublisherState = AndroidJavaInitPublisherState.NativeFreshInitializeIncomplete;
                            Logger.InfoError(
                                "Init (Android Java): native initialize() returned but isInitialized()==false — often duplicate com.bidscube:bidscube-sdk on the classpath (e.g. manual implementation line + postprocessor). Keep a single core SDK source (Maven " +
                                Constants.NativeAndroidBidscubeSdkVersion + " per Gradle inject).");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var msg = e.Message ?? string.Empty;
                if (e is AndroidJavaException && msg.Contains("ClassNotFoundException", StringComparison.Ordinal) &&
                    msg.Contains("com.bidscube.sdk", StringComparison.Ordinal))
                {
                    LastPublisherState = AndroidJavaInitPublisherState.NativeJarMissingUnityCreativesOk;
                    Logger.Warning(
                        "Init (Android Java): com.bidscube.sdk.BidscubeSDK not in the APK (ClassNotFoundException). " +
                        "Unity C# creatives (e.g. ShowVideoAd) still work; embed the native Android Bidscube SDK AAR for MAX / Java parity — see Documentation~/INTEGRATION.md.");
                }
                else
                {
                    LastPublisherState = AndroidJavaInitPublisherState.ExceptionDuringSync;
                    Logger.InfoError($"Init (Android Java): FAILED — {e.GetType().Name}: {e.Message}");
                }
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
                            "[BidscubeAndroidSdkInterop] SDKConfig.Builder has no adRequestAuthority/baseURL setter; C# authority not applied on this native SDK version. " +
                            "Upgrade Unity UPM com.bidscube.sdk and ensure Gradle resolves com.bidscube:bidscube-sdk:" + Constants.NativeAndroidBidscubeSdkVersion +
                            " (remove legacy project(':bidscube-sdk-…') / old AAR). Mismatch can cause native init/callback errors.");
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
