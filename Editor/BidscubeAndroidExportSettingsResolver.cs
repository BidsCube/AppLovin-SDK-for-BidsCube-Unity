using BidscubeSDK.Android;
using UnityEditor;

namespace BidscubeSDK.Editor
{
    /// <summary>
    /// Resolution order: (1) first <see cref="BidscubeAndroidExportSettings"/> asset in the project;
    /// (2) <see cref="BidscubeAndroidFeatureSetStore"/> (EditorPrefs; default <see cref="BidscubeAndroidFeatureSet.LiteNoVideo"/>);
    /// same value is used for the Gradle postprocessor (<c>BidscubeAndroidGradlePostprocessor</c>), <see cref="BidscubeDefineApplicator"/>, and Player builds.
    /// </summary>
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

        /// <summary>
        /// When no <see cref="BidscubeAndroidExportSettings"/> asset exists, returns <c>true</c> (do not alter host Gradle desugaring).
        /// When an asset exists, returns <see cref="BidscubeAndroidExportSettings.enableDesugaring"/>.
        /// </summary>
        internal static bool GetEffectiveEnableDesugaring()
        {
            var asset = TryLoadFirstSettingsAsset();
            if (asset == null)
                return true;
            return asset.enableDesugaring;
        }
    }
}
