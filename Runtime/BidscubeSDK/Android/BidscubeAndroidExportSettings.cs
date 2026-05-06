using UnityEngine;

namespace BidscubeSDK.Android
{
    /// <summary>
    /// Optional project asset to pin Android export behaviour (commit for CI / teams parity).
    /// </summary>
    [CreateAssetMenu(fileName = "BidscubeAndroidExportSettings", menuName = "Bidscube/Android Export Settings", order = 10)]
    public sealed class BidscubeAndroidExportSettings : ScriptableObject
    {
        [Tooltip("Default for new assets (1.0.17+): LiteNoVideo — bundled lite AAR, no Media3/IMA. FullWithVideo — requires bidscube-sdk-*.aar or Maven mode + Media3/IMA.")]
        public BidscubeAndroidFeatureSet featureSet = BidscubeAndroidFeatureSet.LiteNoVideo;

        public BidscubeAndroidCoreDependencyMode coreDependencyMode = BidscubeAndroidCoreDependencyMode.BundledUnityLibraryLibsAar;

        [TextArea(2, 8)]
        [Tooltip("Used when coreDependencyMode == CustomGradleLines (newline-separated implementation lines).")]
        public string customCoreImplementationGradleLines = "";

        public bool forceCompileSdk;
        public int forceCompileSdkValue = 34;

        public bool forceMinSdk;
        public int forceMinSdkValue = 26;

        [Tooltip(
            "When unchecked, the Gradle post-processor removes coreLibraryDesugaring lines and sets coreLibraryDesugaringEnabled false in the generated launcher and unityLibrary build.gradle. " +
            "Bundled bidscube-sdk-lite / bidscube-sdk AARs typically declare desugaring required (AGP checkReleaseAarMetadata) — leave enabled unless you use a core that does not require it.")]
        public bool enableDesugaring = true;
    }
}
