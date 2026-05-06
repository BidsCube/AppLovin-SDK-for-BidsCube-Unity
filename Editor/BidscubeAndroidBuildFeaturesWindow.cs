using System;
using UnityEditor;
using UnityEngine;

namespace BidscubeSDK.Editor
{
    sealed class BidscubeAndroidBuildFeaturesWindow : EditorWindow
    {
        const string MenuPath = "Tools/Bidscube SDK/Android Build Features";

        /// <summary>Must match <c>BidscubeAndroidFeatureSetStore</c> pref key string in com.bidscube.sdk (keep in sync).</summary>
        const string FeatureSetEditorPrefKey = "Bidscube.Android.BidscubeAndroidFeatureSet";

        /// <summary>Must match <c>BIDSCUBE_ANDROID_LITE_NO_VIDEO</c> in core <c>AndroidBuildDefines</c>.</summary>
        const string LiteNoVideoAndroidDefine = "BIDSCUBE_ANDROID_LITE_NO_VIDEO";

        /// <summary>Ordinal of <c>BidscubeAndroidFeatureSet.FullWithVideo</c> in core (do not duplicate the enum here).</summary>
        const int FullWithVideoOrdinal = 1;

        [MenuItem(MenuPath)]
        static void Open()
        {
            GetWindow<BidscubeAndroidBuildFeaturesWindow>(true, "Bidscube Android Build Features");
        }

        bool _fullWithVideo;

        void OnEnable()
        {
            var raw = EditorPrefs.GetInt(FeatureSetEditorPrefKey, 0);
            _fullWithVideo = raw == FullWithVideoOrdinal;
        }

        void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Android Bidscube core variant", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Default: LiteNoVideo — bundled bidscube-sdk-lite-no-video AAR, no Media3/IMA, no core library desugaring in Gradle, BIDSCUBE_ANDROID_LITE_NO_VIDEO on Android.\n" +
                "FullWithVideo — requires bidscube-sdk-full-video AAR under Plugins/Android (or Maven sdk-full-video); adds Media3 + Google IMA + desugar_jdk_libs in the launcher.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            _fullWithVideo = EditorGUILayout.ToggleLeft("FullWithVideo (video player + IMA/Media3)", _fullWithVideo);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(FeatureSetEditorPrefKey, _fullWithVideo ? FullWithVideoOrdinal : 0);
                BidscubeDefineApplicator.ApplyFromStoredFeatureSet();
                Debug.Log(_fullWithVideo
                    ? "[Bidscube AppLovin] EditorPrefs → FullWithVideo (Android define BIDSCUBE_ANDROID_LITE_NO_VIDEO removed)."
                    : "[Bidscube AppLovin] EditorPrefs → LiteNoVideo (Android define BIDSCUBE_ANDROID_LITE_NO_VIDEO set).");
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Android scripting define", EditorStyles.boldLabel);
            var androidDefines = BidscubeDefineApplicator.GetScriptingDefineSymbols(BuildTargetGroup.Android);
            var hasLite = androidDefines.IndexOf(LiteNoVideoAndroidDefine, StringComparison.Ordinal) >= 0;
            EditorGUILayout.LabelField(LiteNoVideoAndroidDefine,
                hasLite ? "Present — LiteNoVideo active for Android player" : "Absent — FullWithVideo for Android player");

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Prefer a committed BidscubeAndroidExportSettings asset (Assets → Create → Bidscube → Android Export Settings) for CI parity.\n" +
                "LiteNoVideo + sdk-lite-no-video should build without coreLibraryDesugaringEnabled. FullWithVideo may require desugaring for AAR metadata.\n" +
                "com.bidscube.sdk should guard video APIs with #if BIDSCUBE_ANDROID_LITE_NO_VIDEO (fail) / #else (video).",
                MessageType.None);
        }
    }
}
