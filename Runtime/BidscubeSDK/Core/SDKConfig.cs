using System;
using UnityEngine;
using UnityEngine.UI;

namespace BidscubeSDK
{
    /// <summary>
    /// SDK configuration class
    /// </summary>
    [Serializable]
    public class SDKConfig
    {
        public bool EnableLogging { get; private set; }
        public bool EnableDebugMode { get; private set; }
        public bool EnableTestMode { get; private set; }
        public int DefaultAdTimeoutMs { get; private set; }
        public AdPosition DefaultAdPosition { get; private set; }

        /// <summary>
        /// Normalized SSP host (and optional port), e.g. <c>ssp-bcc-ads.com</c> or <c>127.0.0.1:8787</c>.
        /// Parity with Android <c>SDKConfig.getAdRequestAuthority()</c>.
        /// </summary>
        public string AdRequestAuthority { get; private set; }

        /// <summary>
        /// HTTPS base URL for ad requests: <c>https://&lt;AdRequestAuthority&gt;/sdk</c>.
        /// Do not pass a full URL with query here — use <see cref="Builder.AdRequestAuthority"/> or <see cref="Builder.BaseURL"/>; the SDK appends path <c>/sdk</c> and query parameters.
        /// </summary>
        public string BaseURL => SspAdUriHelper.BuildHttpsSdkBaseUrl(AdRequestAuthority);

        public AdSizeSettings AdSizeSettings { get; private set; }
        public BidscubeIntegrationMode IntegrationMode { get; private set; }

        /// <summary>
        /// Optional factory for <strong>Direct SDK</strong> linear VAST / direct-URL surface playback (Unity <c>VideoAdView</c>). Prefer <see cref="Builder.VideoPlaybackFactory"/>; <see cref="VideoAdView.VideoPlaybackFactory"/> is a fallback. See package <c>Documentation~/VIDEO_PLAYBACK.md</c>.
        /// </summary>
        public Func<GameObject, RawImage, IVideoSurfacePlayback> VideoPlaybackFactory { get; private set; }

        private SDKConfig(bool enableLogging, bool enableDebugMode, bool enableTestMode, int defaultAdTimeoutMs,
                         AdPosition defaultAdPosition, string adRequestAuthority, AdSizeSettings adSizeSettings,
                         BidscubeIntegrationMode integrationMode,
                         Func<GameObject, RawImage, IVideoSurfacePlayback> videoPlaybackFactory)
        {
            EnableLogging = enableLogging;
            EnableDebugMode = enableDebugMode;
            EnableTestMode = enableTestMode;
            DefaultAdTimeoutMs = defaultAdTimeoutMs;
            DefaultAdPosition = defaultAdPosition;
            AdRequestAuthority = adRequestAuthority;
            AdSizeSettings = adSizeSettings;
            IntegrationMode = integrationMode;
            VideoPlaybackFactory = videoPlaybackFactory;
        }

        /// <summary>
        /// Builder class for SDK configuration
        /// </summary>
        public class Builder
        {
            private bool _enableLogging = true;
            private bool _enableDebugMode = false;
            private bool _enableTestMode = false;
            private int _defaultAdTimeoutMs = 30000;
            private AdPosition _defaultAdPosition = AdPosition.Unknown;
            private string _authorityUserInput;
            private bool _authoritySet;
            private AdSizeSettings _adSizeSettings = null;
            private BidscubeIntegrationMode _integrationMode = BidscubeIntegrationMode.DirectSdk;
            private Func<GameObject, RawImage, IVideoSurfacePlayback> _videoPlaybackFactory;

            public Builder() { }

            /// <summary>
            /// Enable logging
            /// </summary>
            public Builder EnableLogging(bool value)
            {
                _enableLogging = value;
                return this;
            }

            /// <summary>
            /// Enable debug mode
            /// </summary>
            public Builder EnableDebugMode(bool value)
            {
                _enableDebugMode = value;
                return this;
            }

            /// <summary>
            /// Forwarded to native Android <c>SDKConfig.Builder</c> when supported (parity with Flutter <c>enableTestMode</c>).
            /// </summary>
            public Builder EnableTestMode(bool value)
            {
                _enableTestMode = value;
                return this;
            }

            /// <summary>
            /// Direct SDK vs AppLovin MAX mediation (default: direct).
            /// </summary>
            public Builder IntegrationMode(BidscubeIntegrationMode mode)
            {
                _integrationMode = mode;
                return this;
            }

            /// <summary>
            /// Restore <see cref="IntegrationMode"/> from JSON/PlayerPrefs wire values (<c>direct</c>, <c>appLovinMax</c>, legacy <c>levelPlay</c>).
            /// </summary>
            public Builder IntegrationModeFromWire(string wire)
            {
                _integrationMode = BidscubeIntegrationModeWire.FromWire(wire);
                return this;
            }

            /// <summary>
            /// Set default ad timeout
            /// </summary>
            public Builder DefaultAdTimeout(int millis)
            {
                _defaultAdTimeoutMs = millis;
                return this;
            }

            /// <summary>
            /// Set default ad position
            /// </summary>
            public Builder DefaultAdPosition(AdPosition position)
            {
                _defaultAdPosition = position;
                return this;
            }

            /// <summary>
            /// HTTPS host (and optional port) for Bidscube SSP ad requests.
            /// Accepts <c>host</c>, <c>host:port</c>, IPv6 <c>[addr]:port</c>, or a prefix such as <c>https://edge.example.com/sdk</c> (scheme and path are stripped).
            /// Do not pass a full URL with query — the SDK appends <c>/sdk</c> and query parameters.
            /// </summary>
            public Builder AdRequestAuthority(string authorityOrUrlPrefix)
            {
                _authorityUserInput = authorityOrUrlPrefix;
                _authoritySet = true;
                return this;
            }

            /// <summary>
            /// Same normalization as <see cref="AdRequestAuthority"/> — platform-style alias for publishers migrating from older samples.
            /// </summary>
            public Builder BaseURL(string urlOrAuthorityPrefix)
            {
                _authorityUserInput = urlOrAuthorityPrefix;
                _authoritySet = true;
                return this;
            }

            /// <summary>
            /// Set AdSizeSettings asset to provide default ad sizes
            /// </summary>
            public Builder AdSizeSettings(AdSizeSettings settings)
            {
                _adSizeSettings = settings;
                return this;
            }

            /// <summary>
            /// Custom linear video backend for Direct SDK <c>VideoAdView</c> (e.g. AVPro). With <c>BIDSCUBE_DISABLE_UNITY_VIDEO</c>, this or <see cref="VideoAdView.VideoPlaybackFactory"/> is required for that path. Not used for AppLovin MAX native video — see <c>Documentation~/VIDEO_PLAYBACK.md</c>.
            /// </summary>
            public Builder VideoPlaybackFactory(Func<GameObject, RawImage, IVideoSurfacePlayback> factory)
            {
                _videoPlaybackFactory = factory;
                return this;
            }

            /// <summary>
            /// Build SDK configuration
            /// </summary>
            public SDKConfig Build()
            {
                var authority = !_authoritySet
                    ? Constants.DefaultAdRequestAuthority
                    : AdRequestAuthorityNormalizer.Normalize(_authorityUserInput);

                return new SDKConfig(
                    _enableLogging,
                    _enableDebugMode,
                    _enableTestMode,
                    _defaultAdTimeoutMs,
                    _defaultAdPosition,
                    authority,
                    _adSizeSettings,
                    _integrationMode,
                    _videoPlaybackFactory
                );
            }
        }

        /// <summary>
        /// Get detected app ID
        /// </summary>
        public static string DetectedAppId => Application.identifier;

        /// <summary>
        /// Get detected app name
        /// </summary>
        public static string DetectedAppName => Application.productName;

        /// <summary>
        /// Get detected app version
        /// </summary>
        public static string DetectedAppVersion => Application.version;

        /// <summary>
        /// Get detected language
        /// </summary>
        public static string DetectedLanguage => Application.systemLanguage.ToString();

        /// <summary>
        /// Get detected user agent
        /// </summary>
        public static string DetectedUserAgent =>
            $"BidscubeSDK-Unity/{Constants.SdkVersion} (Unity {Application.unityVersion}; {SystemInfo.operatingSystem})";
    }
}
