#if UNITY_EDITOR
using BidscubeSDK;
using UnityEditor;
using UnityEngine;

namespace BidscubeSDK.Editor.Android
{
    /// <summary>
    /// Resolves the Android export feature set: <see cref="BidscubeAndroidExportSettings"/> asset (if any) wins over
    /// <see cref="BidscubeAndroidGradlePostprocessor.FeatureSet"/>.
    /// </summary>
    public static class BidscubeAndroidExportSettingsResolver
    {
        /// <summary>First <see cref="BidscubeAndroidExportSettings"/> in the project, or <see cref="BidscubeAndroidGradlePostprocessor.FeatureSet"/>.</summary>
        public static BidscubeAndroidFeatureSet GetEffectiveFeatureSet()
        {
            return TryLoadFirst(out var s, out _) ? s.featureSet : BidscubeAndroidGradlePostprocessor.FeatureSet;
        }

        /// <summary>For export logs: where the effective value came from.</summary>
        public static string DescribeEffectiveFeatureSetSource()
        {
            return TryLoadFirst(out _, out var path)
                ? "BidscubeAndroidExportSettings at " + path
                : "BidscubeAndroidGradlePostprocessor.FeatureSet (no BidscubeAndroidExportSettings asset in project)";
        }

        private static bool TryLoadFirst(out BidscubeAndroidExportSettings settings, out string assetPath)
        {
            settings = null;
            assetPath = null;
            var guids = AssetDatabase.FindAssets("t:BidscubeAndroidExportSettings");
            if (guids.Length > 1)
            {
                Debug.LogWarning(
                    "[BidscubeSDK] Multiple BidscubeAndroidExportSettings assets found; using the first. Remove duplicates for predictable Android exports.");
            }

            foreach (var guid in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                var s = AssetDatabase.LoadAssetAtPath<BidscubeAndroidExportSettings>(p);
                if (s != null)
                {
                    settings = s;
                    assetPath = p;
                    return true;
                }
            }

            return false;
        }
    }
}
#endif
