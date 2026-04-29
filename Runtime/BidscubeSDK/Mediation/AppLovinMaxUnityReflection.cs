using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BidscubeSDK.Mediation
{
    /// <summary>
    /// Invokes AppLovin MAX Unity plugin (<c>MaxSdk</c> in assembly <c>MaxSdk.Scripts</c>) via reflection so the Bidscube package
    /// compiles before MAX is imported. After you add the official MAX plugin, the same calls reach native MAX and your Bidscube adapter.
    /// </summary>
    public static class AppLovinMaxUnityReflection
    {
        private static Type _maxSdkType;
        private static Type _maxSdkBaseType;

        /// <summary>True when a loaded assembly exposes <c>MaxSdk.InitializeSdk</c>.</summary>
        public static bool IsMaxSdkAvailable => ResolveMaxSdk() != null;

        private static Type ResolveMaxSdk()
        {
            if (_maxSdkType != null)
                return _maxSdkType;

            // Faster path + helps some IL2CPP builds where only assembly-qualified lookup resolves.
            try
            {
                var qt = Type.GetType("MaxSdk, MaxSdk.Scripts", throwOnError: false);
                if (qt?.GetMethod("InitializeSdk", BindingFlags.Public | BindingFlags.Static) != null)
                {
                    _maxSdkType = qt;
                    _maxSdkBaseType = FindMaxSdkBase(qt);
                    return _maxSdkType;
                }
            }
            catch
            {
                // continue with scan
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t;
                try
                {
                    t = assembly.GetType("MaxSdk", false);
                }
                catch
                {
                    continue;
                }

                if (t == null)
                    continue;
                var init = t.GetMethod("InitializeSdk", BindingFlags.Public | BindingFlags.Static);
                if (init != null)
                {
                    _maxSdkType = t;
                    _maxSdkBaseType = FindMaxSdkBase(t);
                    return _maxSdkType;
                }
            }

            return null;
        }

        private static Type FindMaxSdkBase(Type maxSdk)
        {
            for (var t = maxSdk?.BaseType; t != null; t = t.BaseType)
            {
                if (t.Name == "MaxSdkBase")
                    return t;
            }

            return null;
        }

        private static bool InvokeStaticBestMatch(string methodName, object[] args)
        {
            var type = ResolveMaxSdk();
            if (type == null)
                return false;

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(m => m.Name == methodName)
                .ToArray();
            foreach (var m in methods)
            {
                var ps = m.GetParameters();
                if (ps.Length != args.Length)
                    continue;
                var ok = true;
                for (var i = 0; i < ps.Length; i++)
                {
                    if (args[i] == null)
                    {
                        if (ps[i].ParameterType.IsValueType && Nullable.GetUnderlyingType(ps[i].ParameterType) == null)
                            ok = false;
                        continue;
                    }

                    if (!ps[i].ParameterType.IsInstanceOfType(args[i]) && args[i].GetType() != ps[i].ParameterType)
                    {
                        if (!(ps[i].ParameterType.IsEnum && args[i].GetType().IsEnum &&
                              ps[i].ParameterType.Name == args[i].GetType().Name))
                            ok = false;
                    }
                }

                if (!ok)
                    continue;
                m.Invoke(null, args);
                return true;
            }

            return false;
        }

        /// <summary>Deprecated in MAX but still used by many samples; forwards SDK key before <see cref="TryInitializeSdk"/>.</summary>
        public static void TrySetSdkKey(string sdkKey)
        {
            if (string.IsNullOrWhiteSpace(sdkKey))
                return;
            var type = ResolveMaxSdk();
            if (type == null)
                return;
            var m = type.GetMethod("SetSdkKey", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (m != null)
                m.Invoke(null, new object[] { sdkKey });
        }

        /// <summary>Mirrors Android <c>AppLovinSdkInitializationConfiguration.testDeviceAdvertisingIds</c>; call before <see cref="TryInitializeSdk"/>.</summary>
        public static void TrySetTestDeviceAdvertisingIdentifiers(string[] advertisingIdentifiers)
        {
            if (advertisingIdentifiers == null || advertisingIdentifiers.Length == 0)
                return;
            var type = ResolveMaxSdk();
            if (type == null)
                return;
            var m = type.GetMethod("SetTestDeviceAdvertisingIdentifiers",
                BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string[]) }, null);
            if (m != null)
                m.Invoke(null, new object[] { advertisingIdentifiers });
        }

        public static void TrySetVerboseLogging(bool enabled)
        {
            var type = ResolveMaxSdk();
            if (type == null)
                return;
            var m = type.GetMethod("SetVerboseLogging", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(bool) }, null);
            if (m != null)
                m.Invoke(null, new object[] { enabled });
        }

        public static bool TryInitializeSdk()
        {
            var type = ResolveMaxSdk();
            if (type == null)
                return false;
            var m0 = type.GetMethod("InitializeSdk", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
            if (m0 != null)
            {
                m0.Invoke(null, null);
                return true;
            }

            var m1 = type.GetMethod("InitializeSdk", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string[]) }, null);
            if (m1 != null)
            {
                m1.Invoke(null, new object[] { null });
                return true;
            }

            return false;
        }

        public static bool TryIsInitialized()
        {
            var type = ResolveMaxSdk();
            if (type == null)
                return false;
            var m = type.GetMethod("IsInitialized", BindingFlags.Public | BindingFlags.Static);
            if (m == null)
                return false;
            return m.Invoke(null, null) is true;
        }

        public static void TryLoadInterstitial(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
                return;
            InvokeStaticBestMatch("LoadInterstitial", new object[] { adUnitId });
        }

        public static bool TryIsInterstitialReady(string adUnitId)
        {
            var type = ResolveMaxSdk();
            if (type == null || string.IsNullOrEmpty(adUnitId))
                return false;
            var m = type.GetMethod("IsInterstitialReady", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (m == null)
                return false;
            return m.Invoke(null, new object[] { adUnitId }) is true;
        }

        public static void TryShowInterstitial(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
                return;
            var type = ResolveMaxSdk();
            if (type == null)
                return;
            var m = type.GetMethod("ShowInterstitial", BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(string), typeof(string), typeof(string) }, null);
            if (m != null)
                m.Invoke(null, new object[] { adUnitId, null, null });
        }

        public static void TryLoadRewardedAd(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
                return;
            InvokeStaticBestMatch("LoadRewardedAd", new object[] { adUnitId });
        }

        public static bool TryIsRewardedAdReady(string adUnitId)
        {
            var type = ResolveMaxSdk();
            if (type == null || string.IsNullOrEmpty(adUnitId))
                return false;
            var m = type.GetMethod("IsRewardedAdReady", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (m == null)
                return false;
            return m.Invoke(null, new object[] { adUnitId }) is true;
        }

        public static void TryShowRewardedAd(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
                return;
            var type = ResolveMaxSdk();
            if (type == null)
                return;
            var m = type.GetMethod("ShowRewardedAd", BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(string), typeof(string), typeof(string) }, null);
            if (m != null)
                m.Invoke(null, new object[] { adUnitId, null, null });
        }

        public static bool TryCreateBannerBottomCenter(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
                return false;
            var type = ResolveMaxSdk();
            if (type == null || _maxSdkBaseType == null)
                return false;

            var bannerPos = _maxSdkBaseType.GetNestedType("BannerPosition", BindingFlags.Public);
            if (bannerPos == null || !bannerPos.IsEnum)
                return false;
            object bottom;
            try
            {
                bottom = Enum.Parse(bannerPos, "BottomCenter");
            }
            catch
            {
                return false;
            }

            var mi = type.GetMethod("CreateBanner", BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(string), bannerPos }, null);
            if (mi == null)
                return false;
            mi.Invoke(null, new[] { adUnitId, bottom });
            return true;
        }

        public static void TryShowBanner(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
                return;
            InvokeStaticBestMatch("ShowBanner", new object[] { adUnitId });
        }

        public static void TryHideBanner(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
                return;
            InvokeStaticBestMatch("HideBanner", new object[] { adUnitId });
        }

        public static void TryDestroyBanner(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
                return;
            InvokeStaticBestMatch("DestroyBanner", new object[] { adUnitId });
        }

        public static void TryShowMediationDebugger()
        {
            InvokeStaticBestMatch("ShowMediationDebugger", Array.Empty<object>());
        }
    }
}
