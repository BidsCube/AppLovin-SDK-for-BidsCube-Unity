namespace BidscubeSDK.Mediation
{
    /// <summary>Version metadata for <c>com.bidscube.applovin.max</c>; must stay aligned with <c>package.json</c> and release tooling.</summary>
    public static class AdapterPackageInfo
    {
        public const string UpmVersion = "1.0.21";

        /// <summary>Native Bidscube Android SDK semver bundled / Maven pin (may trail Unity UPM <c>com.bidscube.sdk</c> patch).</summary>
        public const string NativeAndroidBidscubeSdkVersion = "1.2.5";

        public const string BundledMaxAdapterAarVersion = "1.2.5";

        /// <summary>iOS CocoaPods <c>BidscubeSDKAppLovin</c> pin (align with native releases).</summary>
        public const string IosBidscubeAppLovinPodVersion = "1.0.4";

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
