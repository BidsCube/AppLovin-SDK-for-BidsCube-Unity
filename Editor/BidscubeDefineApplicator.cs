using System;
using System.Collections.Generic;
using BidscubeSDK.Android;
using UnityEditor;

namespace BidscubeSDK.Editor
{
    internal static class BidscubeDefineApplicator
    {
        const string LegacyEnableVideo = "BIDSCUBE_ENABLE_VIDEO";

        public static void ApplyFromEffectiveSettings()
        {
            Apply(BidscubeAndroidExportSettingsResolver.GetEffectiveFeatureSet());
        }

        public static void ApplyFromStoredFeatureSet()
        {
            Apply(BidscubeAndroidFeatureSetStore.Load());
        }

        public static void Apply(BidscubeAndroidFeatureSet featureSet)
        {
            var liteAndroid = featureSet == BidscubeAndroidFeatureSet.LiteNoVideo;
            ApplyLiteNoVideoDefineForAndroid(liteAndroid);
        }

        /// <summary>
        /// Android-only: adds <see cref="AndroidBuildDefines.LiteNoVideoSymbol"/> for LiteNoVideo; removes for FullWithVideo.
        /// Also strips legacy <c>BIDSCUBE_ENABLE_VIDEO</c> from all groups.
        /// </summary>
        static void ApplyLiteNoVideoDefineForAndroid(bool liteNoVideo)
        {
            var liteSym = AndroidBuildDefines.LiteNoVideoSymbol;
            try
            {
                ApplySymbolForGroup(BuildTargetGroup.Android, liteSym, liteNoVideo);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[Bidscube AppLovin] Could not set Android scripting defines: {e.Message}");
            }

            StripLegacyEnableVideoFromAllGroups();
        }

        static void StripLegacyEnableVideoFromAllGroups()
        {
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (group == BuildTargetGroup.Unknown)
                    continue;
                try
                {
                    RemoveSymbolFromGroup(group, LegacyEnableVideo);
                }
                catch
                {
                    // ignored
                }
            }
        }

        static void ApplySymbolForGroup(BuildTargetGroup group, string symbol, bool add)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var list = new List<string>();
            foreach (var p in defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (p != symbol)
                    list.Add(p);
            }

            if (add)
                list.Add(symbol);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", list));
        }

        static void RemoveSymbolFromGroup(BuildTargetGroup group, string symbol)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var list = new List<string>();
            foreach (var p in defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (p != symbol)
                    list.Add(p);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", list));
        }
    }
}
