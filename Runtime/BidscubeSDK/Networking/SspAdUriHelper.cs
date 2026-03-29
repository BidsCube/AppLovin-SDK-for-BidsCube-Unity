using System;
using System.Text.RegularExpressions;

namespace BidscubeSDK
{
    /// <summary>
    /// Builds <c>https://&lt;authority&gt;/sdk</c> for Bidscube ad requests.
    /// Parity with Android <c>com.bidscube.sdk.network.SspAdUriHelper</c>.
    /// (host:port must not rely on Uri encoding that breaks TCP port).
    /// </summary>
    public static class SspAdUriHelper
    {
        private static readonly Regex PortDigits = new Regex(@"^\d{1,5}$", RegexOptions.Compiled);

        /// <summary>
        /// Returns the HTTPS base URL for the SDK ad endpoint (path <c>/sdk</c>, no trailing slash before query).
        /// </summary>
        public static string BuildHttpsSdkBaseUrl(string normalizedAuthority)
        {
            var uri = BuildHttpsSdkUri(normalizedAuthority);
            return uri.AbsoluteUri;
        }

        /// <summary>
        /// Same as <see cref="BuildHttpsSdkBaseUrl"/> but avoids allocating <see cref="Uri"/> when only a string is needed
        /// (uses <see cref="UriBuilder"/> internally).
        /// </summary>
        public static Uri BuildHttpsSdkUri(string normalizedAuthority)
        {
            var a = string.IsNullOrWhiteSpace(normalizedAuthority)
                ? Constants.DefaultAdRequestAuthority
                : normalizedAuthority.Trim();
            if (a.Length == 0)
            {
                a = Constants.DefaultAdRequestAuthority;
            }

            var b = new UriBuilder
            {
                Scheme = Uri.UriSchemeHttps,
                Path = "/sdk",
            };

            ApplyAuthority(b, a);
            return b.Uri;
        }

        internal static void ApplyAuthority(UriBuilder builder, string a)
        {
            if (string.IsNullOrWhiteSpace(a))
            {
                a = Constants.DefaultAdRequestAuthority;
            }

            a = a.Trim();
            if (a.Length == 0)
            {
                a = Constants.DefaultAdRequestAuthority;
            }

            // IPv6 [addr]:port
            if (a.StartsWith("[", StringComparison.Ordinal))
            {
                var close = a.IndexOf(']', StringComparison.Ordinal);
                if (close > 1 && a.Length > close + 1 && a[close + 1] == ':')
                {
                    var inside = a.Substring(1, close - 1);
                    var portStr = a.Substring(close + 2);
                    if (PortDigits.IsMatch(portStr))
                    {
                        var p = int.Parse(portStr);
                        if (p >= 0 && p <= 65535)
                        {
                            builder.Host = inside;
                            builder.Port = p;
                            return;
                        }
                    }
                }

                if (a.EndsWith("]", StringComparison.Ordinal) && close > 1)
                {
                    builder.Host = a.Substring(1, a.Length - 2);
                    builder.Port = -1;
                    return;
                }
            }

            var lastColon = a.LastIndexOf(':');
            if (lastColon > 0)
            {
                var hostPart = a.Substring(0, lastColon);
                var portPart = a.Substring(lastColon + 1);
                if (PortDigits.IsMatch(portPart)
                    && hostPart.Length > 0
                    && hostPart.IndexOf(':') < 0
                    && hostPart.IndexOf(']') < 0)
                {
                    var p = int.Parse(portPart);
                    if (p >= 0 && p <= 65535)
                    {
                        builder.Host = hostPart;
                        builder.Port = p;
                        return;
                    }
                }
            }

            builder.Host = a;
            builder.Port = -1;
        }
    }
}
