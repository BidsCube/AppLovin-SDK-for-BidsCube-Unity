namespace BidscubeSDK.Mediation
{
    /// <summary>Version metadata for <c>com.bidscube.applovin.max</c>; must stay aligned with <c>package.json</c> and release tooling.</summary>
    public static class AdapterPackageInfo
    {
        public const string UpmVersion = "1.0.25";

        /// <summary>Native Bidscube Android SDK semver bundled / Maven pin (must match MAX adapter build line).</summary>
        public const string NativeAndroidBidscubeSdkVersion = "1.2.11";

        public const string BundledMaxAdapterAarVersion = "1.2.11";

        /// <summary>iOS CocoaPods <c>BidscubeSDKAppLovin</c> pin (align with native releases).</summary>
        public const string IosBidscubeAppLovinPodVersion = "1.1.1";

        /// <summary>
        /// Native Android OpenRTB 2.6-style <em>response</em> parsing in the bundled MAX adapter / core SDK
        /// (<c>NativeAndroidBidscubeSdkVersion</c> + <c>BundledMaxAdapterAarVersion</c>).
        /// Must match native AppLovin-SDK-for-BidsCube-Android release notes before setting true.
        /// </summary>
        public const bool OpenRtb26AndroidResponseParsingSupported = false;

        /// <summary>
        /// Native iOS OpenRTB 2.6-style <em>response</em> parsing in <c>BidscubeSDKAppLovin</c>
        /// (<c>IosBidscubeAppLovinPodVersion</c>). Must match native AppLovin-SDK-for-BidsCube-iOS before setting true.
        /// </summary>
        public const bool OpenRtb26IosResponseParsingSupported = false;

        public static string NativeAndroidBundledCoreAarLiteFileName =>
            $"bidscube-sdk-lite-no-video-{NativeAndroidBidscubeSdkVersion}.aar";

        public static string NativeAndroidBundledCoreAarWebViewVideoFileName =>
            $"bidscube-sdk-webview-video-{NativeAndroidBidscubeSdkVersion}.aar";

        public static string NativeAndroidBundledCoreAarLegacyMediaVideoFileName =>
            $"bidscube-sdk-legacy-media-video-{NativeAndroidBidscubeSdkVersion}.aar";

        public static string NativeAndroidBundledCoreAarFullFileName =>
            $"bidscube-sdk-full-video-{NativeAndroidBidscubeSdkVersion}.aar";
    }
}
