using System;
using BidscubeSDK;
using UnityEditor;
using UnityEngine;

namespace BidscubeSDK.Editor
{
    sealed class BidscubeAndroidBuildFeaturesWindow : EditorWindow
    {
        const string MenuPath = "Tools/Bidscube SDK/Android Build Features";

        [MenuItem(MenuPath)]
        static void Open()
        {
            GetWindow<BidscubeAndroidBuildFeaturesWindow>(true, "Bidscube Android Build Features");
        }

        BidscubeFeatureSet _featureSet;
        bool _enableVideo;

        void OnEnable()
        {
            _featureSet = BidscubeFeatureSetStore.Load();
            _enableVideo = _featureSet == BidscubeFeatureSet.FullWithVideo;
        }

        void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Android native SDK capacity", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Enabled: FullWithVideo — video ads, IMA/Media3 Gradle lines, full core AAR (or Maven @aar).\n" +
                "Disabled: LiteNoVideo — lite core AAR only; no IMA/Media3 lines in Gradle.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            _enableVideo = EditorGUILayout.ToggleLeft("Enable video ads and video player (FullWithVideo)", _enableVideo);
            if (EditorGUI.EndChangeCheck())
            {
                _featureSet = _enableVideo ? BidscubeFeatureSet.FullWithVideo : BidscubeFeatureSet.LiteNoVideo;
                BidscubeFeatureSetStore.Save(_featureSet);
                BidscubeDefineApplicator.Apply(_featureSet);
                Debug.Log(_enableVideo
                    ? "[Bidscube] FullWithVideo: BIDSCUBE_ENABLE_VIDEO set on Player scripting define symbols."
                    : "[Bidscube] LiteNoVideo: BIDSCUBE_ENABLE_VIDEO removed — re-export Android.");
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Player scripting define (Android)", EditorStyles.boldLabel);
            var androidDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            var hasSym = androidDefines.IndexOf(VideoBuildDefines.EnableVideoSymbol, StringComparison.Ordinal) >= 0;
            EditorGUILayout.LabelField(VideoBuildDefines.EnableVideoSymbol,
                hasSym ? "Present — video code path compiles for Android player" : "Absent — lite / no video path");

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "com.bidscube.sdk must guard video APIs with #if BIDSCUBE_ENABLE_VIDEO for IL stripping to match Gradle.\n" +
                "Samples in this package hide video UI when the define is off.",
                MessageType.None);
        }
    }
}
