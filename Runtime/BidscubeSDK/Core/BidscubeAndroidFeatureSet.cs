namespace BidscubeSDK
{
    /// <summary>
    /// Android Gradle export profile for the bundled <c>com.bidscube.sdk</c> core AAR and optional IMA/Media3 Maven lines.
    /// Configure via a <see cref="BidscubeAndroidExportSettings"/> asset (recommended for teams / GitHub) or
    /// <c>BidscubeAndroidGradlePostprocessor.FeatureSet</c> as a code-only fallback.
    /// </summary>
    public enum BidscubeAndroidFeatureSet
    {
        /// <summary>Bundled <c>bidscube-sdk-lite-*.aar</c>; no Media3 / IMA Maven lines. Banner / native / image.</summary>
        LiteNoVideo = 0,

        /// <summary>Bundled <c>bidscube-sdk-*.aar</c> (with IMA); Media3 + interactivemedia.</summary>
        FullWithVideo = 1
    }
}
