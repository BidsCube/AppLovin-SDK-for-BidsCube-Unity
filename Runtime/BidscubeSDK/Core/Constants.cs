namespace BidscubeSDK
{
    /// <summary>
    /// SDK constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Default timeout in milliseconds
        /// </summary>
        public const int DefaultTimeoutMs = 30000;

        /// <summary>
        /// Default ad position
        /// </summary>
        public const AdPosition DefaultAdPosition = AdPosition.Unknown;

        /// <summary>
        /// Default SSP host for ad requests (authority only — no scheme/path/query).
        /// Parity with Android <c>DeviceInfo.DEFAULT_AD_REQUEST_AUTHORITY</c>.
        /// </summary>
        public const string DefaultAdRequestAuthority = "ssp-bcc-ads.com";

        /// <summary>
        /// Default HTTPS ad endpoint (<c>https://&lt;DefaultAdRequestAuthority&gt;/sdk</c>) for samples and tests.
        /// </summary>
        public const string BaseURL = "https://ssp-bcc-ads.com/sdk";

        /// <summary>
        /// User agent prefix
        /// </summary>
        public const string UserAgentPrefix = "BidscubeSDK";

        /// <summary>
        /// Unity UPM package version (user-agent, diagnostics). Must match <c>package.json</c> <c>version</c>.
        /// </summary>
        public const string SdkVersion = "1.0.6";

        /// <summary>
        /// Native Android core SDK semver (parity with published <c>com.bidscube:bidscube-sdk</c> when using default Gradle injection). On export, <c>BidscubeAndroidGradlePostprocessor</c> injects this version as <c>@aar</c> unless <c>CoreDependencyMode</c> uses custom lines / integrator-owned core.
        /// </summary>
        public const string NativeAndroidBidscubeSdkVersion = "1.2.3";

        /// <summary>
        /// Error codes
        /// </summary>
        public static class ErrorCodes
        {
            public const int InvalidURL = 1001;
            public const int InvalidResponse = 1002;
            public const int NetworkError = 1003;
            public const int TimeoutError = 1004;
            public const int Timeout = 1004; // Alias for TimeoutError
            public const int UnknownError = 1005;
        }

        /// <summary>
        /// Error messages
        /// </summary>
        public static class ErrorMessages
        {
            public const string FailedToBuildURL = "Failed to build request URL";
            public const string InvalidResponse = "Invalid response from server";
            public const string NetworkError = "Network error occurred";
            public const string TimeoutError = "Request timeout";
            public const string UnknownError = "Unknown error occurred";
        }
    }

    /// <summary>
    /// Public error codes alias for documentation compatibility
    /// </summary>
    public static class ErrorCodes
    {
        public const int InvalidURL = Constants.ErrorCodes.InvalidURL;
        public const int InvalidResponse = Constants.ErrorCodes.InvalidResponse;
        public const int NetworkError = Constants.ErrorCodes.NetworkError;
        public const int TimeoutError = Constants.ErrorCodes.TimeoutError;
        public const int UnknownError = Constants.ErrorCodes.UnknownError;
    }

    /// <summary>
    /// Public error messages alias for documentation compatibility
    /// </summary>
    public static class ErrorMessages
    {
        public const string FailedToBuildURL = Constants.ErrorMessages.FailedToBuildURL;
        public const string InvalidResponse = Constants.ErrorMessages.InvalidResponse;
        public const string NetworkError = Constants.ErrorMessages.NetworkError;
        public const string TimeoutError = Constants.ErrorMessages.TimeoutError;
        public const string UnknownError = Constants.ErrorMessages.UnknownError;
    }
}
