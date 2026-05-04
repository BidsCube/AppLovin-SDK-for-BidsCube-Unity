using UnityEngine;

namespace BidscubeSDK.Android
{
    /// <summary>
    /// Optional project asset to pin Android export behaviour (commit for CI / teams parity).
    /// </summary>
    [CreateAssetMenu(fileName = "BidscubeAndroidExportSettings", menuName = "Bidscube/Android Export Settings", order = 10)]
    public sealed class BidscubeAndroidExportSettings : ScriptableObject
    {
        [Tooltip("Default: LiteNoVideo — no Media3/IMA. FullWithVideo — video player + IMA/Media3 Gradle lines.")]
        public BidscubeAndroidFeatureSet featureSet = BidscubeAndroidFeatureSet.LiteNoVideo;

        public BidscubeAndroidCoreDependencyMode coreDependencyMode = BidscubeAndroidCoreDependencyMode.BundledUnityLibraryLibsAar;

        [TextArea(2, 8)]
        [Tooltip("Used when coreDependencyMode == CustomGradleLines (newline-separated implementation lines).")]
        public string customCoreImplementationGradleLines = "";

        public bool forceCompileSdk;
        public int forceCompileSdkValue = 34;

        public bool forceMinSdk;
        public int forceMinSdkValue = 26;

        public bool enableDesugaring = true;
    }
}
