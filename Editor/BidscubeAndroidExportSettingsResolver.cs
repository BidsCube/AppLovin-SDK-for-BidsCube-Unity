using BidscubeSDK.Android;
using UnityEditor;

namespace BidscubeSDK.Editor
{
    /// <summary>Resolves <see cref="BidscubeAndroidExportSettings"/> from assets, then EditorPrefs, then default <see cref="BidscubeAndroidFeatureSet.LiteNoVideo"/>.</summary>
    internal static class BidscubeAndroidExportSettingsResolver
    {
        internal static BidscubeAndroidExportSettings TryLoadFirstSettingsAsset()
        {
            var guids = AssetDatabase.FindAssets("t:BidscubeAndroidExportSettings");
            if (guids == null || guids.Length == 0)
                return null;
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path))
                return null;
            return AssetDatabase.LoadAssetAtPath<BidscubeAndroidExportSettings>(path);
        }

        internal static BidscubeAndroidFeatureSet GetEffectiveFeatureSet()
        {
            var asset = TryLoadFirstSettingsAsset();
            if (asset != null)
                return asset.featureSet;
            return BidscubeAndroidFeatureSetStore.Load();
        }

        internal static BidscubeAndroidCoreDependencyMode GetEffectiveCoreDependencyMode()
        {
            var asset = TryLoadFirstSettingsAsset();
            if (asset != null)
                return asset.coreDependencyMode;
            return BidscubeAndroidCoreDependencyMode.BundledUnityLibraryLibsAar;
        }

        internal static string GetEffectiveCustomGradleLines()
        {
            var asset = TryLoadFirstSettingsAsset();
            return asset != null ? asset.customCoreImplementationGradleLines ?? "" : "";
        }
    }
}
