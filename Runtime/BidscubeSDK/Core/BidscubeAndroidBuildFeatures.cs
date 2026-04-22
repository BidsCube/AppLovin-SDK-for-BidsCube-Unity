namespace BidscubeSDK
{
    /// <summary>
    /// Reflects <c>BIDSCUBE_ANDROID_LITE_NO_VIDEO</c> on Android player builds (set by the Editor preprocess that reads <c>BidscubeAndroidGradlePostprocessor.FeatureSet</c>).
    /// </summary>
    public static class BidscubeAndroidBuildFeatures
    {
        /// <summary>
        /// <c>true</c> when this assembly was built for Android with <c>BIDSCUBE_ANDROID_LITE_NO_VIDEO</c> (default <c>LiteNoVideo</c> feature set).
        /// iOS / Standalone / Editor-with-non-Android-target builds are <c>false</c>.
        /// </summary>
#if BIDSCUBE_ANDROID_LITE_NO_VIDEO
        public const bool IsLiteNoVideoAndroid = true;
#else
        public const bool IsLiteNoVideoAndroid = false;
#endif
    }
}
