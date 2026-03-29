using System;
using System.Text;

namespace BidscubeSDK
{
    /// <summary>
    /// Builds GET ad-request URLs (query parameters) for each ad type — parity with Android
    /// <c>ImageAdUrlBuilder</c>, <c>VideoAdUrlBuilder</c>, <c>NativeAdUrlBuilder</c>.
    /// </summary>
    /// <summary>Advanced: full ad-request URL construction (same rules as Android URL builders).</summary>
    public static class URLBuilder
    {
        /// <summary>
        /// Builds a full ad request URL (HTTPS base + query). SSP returns JSON UTF-8 body with <c>adm</c> and <c>position</c>.
        /// </summary>
        public static string BuildAdRequestURL(
            SDKConfig config,
            string placementId,
            AdType adType,
            AdPosition positionIgnored = AdPosition.Unknown,
            int timeoutMsIgnored = 0,
            bool debugIgnored = false,
            string ctaTextIgnored = null,
            double? nativeLogicalWidth = null,
            double? nativeLogicalHeight = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var authority = config.AdRequestAuthority;
            switch (adType)
            {
                case AdType.Image:
                    return BuildImageAdUrl(authority, placementId);
                case AdType.Video:
                    return BuildVideoAdUrl(authority, placementId);
                case AdType.Native:
                    return BuildNativeAdUrl(authority, placementId, nativeLogicalWidth, nativeLogicalHeight);
                default:
                    return BuildImageAdUrl(authority, placementId);
            }
        }

        private static string BuildImageAdUrl(string authority, string placementId)
        {
            var root = SspAdUriHelper.BuildHttpsSdkBaseUrl(authority).TrimEnd('/');
            var sb = new StringBuilder(root.Length + 256);
            sb.Append(root);
            sb.Append('?');
            var first = true;
            void Q(string k, string v)
            {
                if (!first)
                {
                    sb.Append('&');
                }

                first = false;
                sb.Append(k);
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(v ?? string.Empty));
            }

            Q("placementId", placementId);
            Q("c", "b");
            Q("m", "api");
            Q("res", "js");
            Q("app", "1");
            Q("bundle", DeviceInfo.BundleId);
            Q("name", DeviceInfo.AppName);
            Q("app_store_url", DeviceInfo.AppStoreURL);
            Q("language", DeviceInfo.Language);
            Q("deviceWidth", DeviceInfo.DeviceWidth.ToString());
            Q("deviceHeight", DeviceInfo.DeviceHeight.ToString());
            Q("ua", DeviceInfo.UserAgent);
            Q("ifa", DeviceInfo.AdvertisingIdentifier);
            Q("dnt", DeviceInfo.DoNotTrack.ToString());

            Logger.Info($"Built image ad URL: {sb}");
            return sb.ToString();
        }

        private static string BuildVideoAdUrl(string authority, string placementId)
        {
            var root = SspAdUriHelper.BuildHttpsSdkBaseUrl(authority).TrimEnd('/');
            var sb = new StringBuilder(root.Length + 256);
            sb.Append(root);
            sb.Append('?');
            var first = true;
            void Q(string k, string v)
            {
                if (!first)
                {
                    sb.Append('&');
                }

                first = false;
                sb.Append(k);
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(v ?? string.Empty));
            }

            Q("c", "v");
            Q("m", "xml");
            Q("id", placementId);
            Q("app", "1");
            Q("w", DeviceInfo.DeviceWidth.ToString());
            Q("h", DeviceInfo.DeviceHeight.ToString());
            Q("bundle", DeviceInfo.BundleId);
            Q("name", DeviceInfo.AppName);
            Q("app_version", DeviceInfo.AppVersion ?? string.Empty);
            Q("ifa", DeviceInfo.AdvertisingIdentifier);
            Q("dnt", DeviceInfo.DoNotTrack.ToString());
            Q("app_store_url", DeviceInfo.AppStoreURL);
            Q("ua", DeviceInfo.UserAgent);
            Q("language", DeviceInfo.Language);
            Q("deviceWidth", DeviceInfo.DeviceWidth.ToString());
            Q("deviceHeight", DeviceInfo.DeviceHeight.ToString());

            Logger.Info($"Built video ad URL: {sb}");
            return sb.ToString();
        }

        private static string BuildNativeAdUrl(string authority, string placementId, double? adWidth, double? adHeight)
        {
            var w = adWidth ?? 1080d;
            var h = adHeight ?? 800d;

            var root = SspAdUriHelper.BuildHttpsSdkBaseUrl(authority).TrimEnd('/');
            var sb = new StringBuilder(root.Length + 320);
            sb.Append(root);
            sb.Append('?');
            var first = true;
            void Q(string k, string v)
            {
                if (!first)
                {
                    sb.Append('&');
                }

                first = false;
                sb.Append(k);
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(v ?? string.Empty));
            }

            Q("c", "n");
            Q("m", "s");
            Q("id", placementId);
            Q("app", "1");
            Q("bundle", DeviceInfo.BundleId);
            Q("name", DeviceInfo.AppName);
            Q("app_version", DeviceInfo.AppVersion ?? string.Empty);
            Q("ifa", DeviceInfo.AdvertisingIdentifier ?? string.Empty);
            Q("dnt", DeviceInfo.DoNotTrack.ToString());
            Q("app_store_url", DeviceInfo.AppStoreURL);
            Q("ua", DeviceInfo.UserAgent);
            Q("gdpr", DeviceInfo.GDPR);
            Q("gdpr_consent", DeviceInfo.GDPRConsentForNativeQuery);
            Q("us_privacy", DeviceInfo.USPrivacyForNativeQuery);
            Q("ccpa", DeviceInfo.CcpaForNativeQuery);
            Q("coppa", DeviceInfo.CoppaBit);
            Q("language", DeviceInfo.Language);
            Q("deviceWidth", DeviceInfo.DeviceWidth.ToString());
            Q("deviceHeight", DeviceInfo.DeviceHeight.ToString());
            Q("w", w.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Q("h", h.ToString(System.Globalization.CultureInfo.InvariantCulture));

            Logger.Info($"Built native ad URL: {sb}");
            return sb.ToString();
        }
    }
}
