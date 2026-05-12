using System;
using BidscubeSDK.Android;
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

        [MenuItem(MenuPath)]
        static void Open()
        {
            GetWindow<BidscubeAndroidBuildFeaturesWindow>(true, "Bidscube Android Build Features");
        }

        BidscubeAndroidFeatureSet _featureSet;

        void OnEnable()
        {
            var raw = EditorPrefs.GetInt(FeatureSetEditorPrefKey, (int)BidscubeAndroidFeatureSet.LiteNoVideo);
            _featureSet = Enum.IsDefined(typeof(BidscubeAndroidFeatureSet), raw)
                ? (BidscubeAndroidFeatureSet)raw
                : BidscubeAndroidFeatureSet.LiteNoVideo;
        }

        void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Android Bidscube core variant", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "LiteNoVideo — smallest build, no rewarded/video, no Media3/IMA, no core library desugaring.\n" +
                "WebViewVideoNoDesugar — Android WebView + HTML5 video, no Media3/IMA, no core library desugaring.\n" +
                "LegacyMediaVideoNoDesugar — VideoView/MediaPlayer video path, no Media3/IMA, no core library desugaring.\n" +
                "FullWithVideo — full IMA/Media3 video path, best VAST support, launcher desugaring allowed.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            _featureSet = (BidscubeAndroidFeatureSet)EditorGUILayout.EnumPopup("Feature Set", _featureSet);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(FeatureSetEditorPrefKey, (int)_featureSet);
                BidscubeDefineApplicator.ApplyFromStoredFeatureSet();
                Debug.Log($"[Bidscube AppLovin] EditorPrefs → {_featureSet}.");
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Android scripting define", EditorStyles.boldLabel);
            var androidDefines = BidscubeDefineApplicator.GetScriptingDefineSymbols(BuildTargetGroup.Android);
            var hasLite = androidDefines.IndexOf(LiteNoVideoAndroidDefine, StringComparison.Ordinal) >= 0;
            EditorGUILayout.LabelField(LiteNoVideoAndroidDefine,
                hasLite ? "Present — LiteNoVideo active for Android player" : "Absent — a video-enabled mode is active");

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Prefer a committed BidscubeAndroidExportSettings asset (Assets → Create → Bidscube → Android Export Settings) for CI parity.\n" +
                "LiteNoVideo / WebViewVideoNoDesugar / LegacyMediaVideoNoDesugar should build without coreLibraryDesugaringEnabled. FullWithVideo may require launcher desugaring.\n" +
                "com.bidscube.sdk should guard video APIs with #if BIDSCUBE_ANDROID_LITE_NO_VIDEO (fail) / #else (video).",
                MessageType.None);
        }
    }
}
