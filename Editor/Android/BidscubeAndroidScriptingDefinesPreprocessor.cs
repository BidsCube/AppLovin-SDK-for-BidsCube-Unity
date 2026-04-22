#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using BidscubeSDK;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BidscubeSDK.Editor.Android
{
    /// <summary>
    /// Syncs <c>BIDSCUBE_ANDROID_LITE_NO_VIDEO</c> on the Android scripting define list from the same effective source as Gradle export
    /// (<see cref="BidscubeAndroidExportSettings"/> asset or <see cref="BidscubeAndroidGradlePostprocessor.FeatureSet"/> fallback).
    /// </summary>
    internal sealed class BidscubeAndroidScriptingDefinesPreprocessor : IPreprocessBuildWithReport
    {
        public const string LiteNoVideoScriptingDefine = "BIDSCUBE_ANDROID_LITE_NO_VIDEO";

        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            var group = BuildTargetGroup.Android;
            var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var set = new HashSet<string>(
                current.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()),
                StringComparer.Ordinal);

            var wantLite = BidscubeAndroidExportSettingsResolver.GetEffectiveFeatureSet() == BidscubeAndroidFeatureSet.LiteNoVideo;
            var hadLite = set.Contains(LiteNoVideoScriptingDefine);

            if (wantLite)
                set.Add(LiteNoVideoScriptingDefine);
            else
                set.Remove(LiteNoVideoScriptingDefine);

            var next = string.Join(";", set.OrderBy(s => s, StringComparer.Ordinal));
            if (next != current)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, next);

            if (wantLite)
                Debug.Log("[BidscubeSDK] Android preprocess: " + LiteNoVideoScriptingDefine + " enabled (LiteNoVideo).");
            else if (hadLite)
                Debug.Log("[BidscubeSDK] Android preprocess: " + LiteNoVideoScriptingDefine + " removed (FullWithVideo).");
        }
    }
}
#endif
