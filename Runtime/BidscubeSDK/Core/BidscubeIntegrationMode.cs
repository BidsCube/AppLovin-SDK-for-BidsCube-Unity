namespace BidscubeSDK
{
    /// <summary>
    /// Direct Unity/C# SDK vs AppLovin MAX mediation (native adapter drives load/show).
    /// </summary>
    public enum BidscubeIntegrationMode
    {
        /// <summary>Ads are requested and displayed from Unity/C# APIs.</summary>
        DirectSdk = 0,

        /// <summary>
        /// Ads load and show only through AppLovin MAX. Call <see cref="BidscubeSDK.BidscubeSDK.Initialize(SDKConfig)"/>
        /// early so the native Bidscube stack matches the MAX custom adapter instance.
        /// </summary>
        AppLovinMaxMediation = 1,
    }

    /// <summary>Wire-string helpers for JSON / PlayerPrefs (parity with Flutter).</summary>
    public static class BidscubeIntegrationModeWire
    {
        /// <summary>Serialize for storage or logs (Flutter: direct / appLovinMax).</summary>
        public static string ToWireString(this BidscubeIntegrationMode mode)
        {
            return mode == BidscubeIntegrationMode.DirectSdk ? "direct" : "appLovinMax";
        }

        /// <summary>
        /// Deserialize stored values. Legacy <c>levelPlay</c> maps to <see cref="BidscubeIntegrationMode.AppLovinMaxMediation"/>.
        /// </summary>
        public static BidscubeIntegrationMode FromWire(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return BidscubeIntegrationMode.DirectSdk;

            var v = value.Trim();
            if (string.Equals(v, "appLovinMax", System.StringComparison.OrdinalIgnoreCase))
                return BidscubeIntegrationMode.AppLovinMaxMediation;
            if (string.Equals(v, "levelPlay", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "level_play", System.StringComparison.OrdinalIgnoreCase))
                return BidscubeIntegrationMode.AppLovinMaxMediation;
            if (string.Equals(v, "directSdk", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "direct", System.StringComparison.OrdinalIgnoreCase))
                return BidscubeIntegrationMode.DirectSdk;

            return BidscubeIntegrationMode.DirectSdk;
        }

        public static bool IsMediationMode(this BidscubeIntegrationMode mode) =>
            mode == BidscubeIntegrationMode.AppLovinMaxMediation;
    }
}
