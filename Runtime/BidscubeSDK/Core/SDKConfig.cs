using System;
using UnityEngine;

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
        public string BaseURL { get; private set; }
        public AdSizeSettings AdSizeSettings { get; private set; }
        public BidscubeIntegrationMode IntegrationMode { get; private set; }

        private SDKConfig(bool enableLogging, bool enableDebugMode, bool enableTestMode, int defaultAdTimeoutMs,
                         AdPosition defaultAdPosition, string baseURL, AdSizeSettings adSizeSettings,
                         BidscubeIntegrationMode integrationMode)
        {
            EnableLogging = enableLogging;
            EnableDebugMode = enableDebugMode;
            EnableTestMode = enableTestMode;
            DefaultAdTimeoutMs = defaultAdTimeoutMs;
            DefaultAdPosition = defaultAdPosition;
            BaseURL = baseURL;
            AdSizeSettings = adSizeSettings;
            IntegrationMode = integrationMode;
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
            private string _baseURL = Constants.BaseURL;
            private AdSizeSettings _adSizeSettings = null;
            private BidscubeIntegrationMode _integrationMode = BidscubeIntegrationMode.DirectSdk;

            public Builder() { }

            /// <summary>
            /// Enable logging
            /// </summary>
            /// <param name="value">Enable logging flag</param>
            /// <returns>Builder instance</returns>
            public Builder EnableLogging(bool value)
            {
                _enableLogging = value;
                return this;
            }

            /// <summary>
            /// Enable debug mode
            /// </summary>
            /// <param name="value">Enable debug mode flag</param>
            /// <returns>Builder instance</returns>
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
            /// <param name="millis">Timeout in milliseconds</param>
            /// <returns>Builder instance</returns>
            public Builder DefaultAdTimeout(int millis)
            {
                _defaultAdTimeoutMs = millis;
                return this;
            }

            /// <summary>
            /// Set default ad position
            /// </summary>
            /// <param name="position">Default ad position</param>
            /// <returns>Builder instance</returns>
            public Builder DefaultAdPosition(AdPosition position)
            {
                _defaultAdPosition = position;
                return this;
            }

            /// <summary>
            /// Set base URL
            /// </summary>
            /// <param name="url">Base URL</param>
            /// <returns>Builder instance</returns>
            public Builder BaseURL(string url)
            {
                _baseURL = url;
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
            /// Build SDK configuration
            /// </summary>
            /// <returns>SDK configuration</returns>
            public SDKConfig Build()
            {
                return new SDKConfig(
                    _enableLogging,
                    _enableDebugMode,
                    _enableTestMode,
                    _defaultAdTimeoutMs,
                    _defaultAdPosition,
                    _baseURL,
                    _adSizeSettings,
                    _integrationMode
                );
            }
        }

        /// <summary>
        /// Get detected app ID
        /// </summary>
        public static string DetectedAppId
        {
            get
            {
                return Application.identifier;
            }
        }

        /// <summary>
        /// Get detected app name
        /// </summary>
        public static string DetectedAppName
        {
            get
            {
                return Application.productName;
            }
        }

        /// <summary>
        /// Get detected app version
        /// </summary>
        public static string DetectedAppVersion
        {
            get
            {
                return Application.version;
            }
        }

        /// <summary>
        /// Get detected language
        /// </summary>
        public static string DetectedLanguage
        {
            get
            {
                return Application.systemLanguage.ToString();
            }
        }

        /// <summary>
        /// Get detected user agent
        /// </summary>
        public static string DetectedUserAgent
        {
            get
            {
                return $"BidscubeSDK-Unity/1.0 (Unity {Application.unityVersion}; {SystemInfo.operatingSystem})";
            }
        }
    }
}
