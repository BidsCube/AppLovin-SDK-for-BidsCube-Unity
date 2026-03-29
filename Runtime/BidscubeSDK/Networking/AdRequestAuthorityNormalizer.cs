using System;

namespace BidscubeSDK
{
    /// <summary>
    /// Normalizes user input for the Bidscube SSP ad-request authority (host[:port]).
    /// Parity with <c>SDKConfig.Builder.normalizeAdRequestAuthority</c> on Android.
    /// </summary>
    public static class AdRequestAuthorityNormalizer
    {
        /// <summary>
        /// Trim, percent-decode (up to 3 passes), strip scheme/path/query; empty → default authority.
        /// </summary>
        public static string Normalize(string input)
        {
            if (input == null)
            {
                return Constants.DefaultAdRequestAuthority;
            }

            var s = input.Trim();
            if (s.Length == 0)
            {
                return Constants.DefaultAdRequestAuthority;
            }

            for (var i = 0; i < 3; i++)
            {
                try
                {
                    var dec = Uri.UnescapeDataString(s);
                    if (string.Equals(dec, s, StringComparison.Ordinal))
                    {
                        break;
                    }

                    s = dec.Trim();
                }
                catch (UriFormatException)
                {
                    break;
                }
            }

            if (s.Length >= 8 && s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(8);
            }
            else if (s.Length >= 7 && s.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(7);
            }

            var slash = s.IndexOf('/');
            if (slash > 0)
            {
                s = s.Substring(0, slash);
            }

            var q = s.IndexOf('?');
            if (q > 0)
            {
                s = s.Substring(0, q);
            }

            s = s.Trim();
            return s.Length == 0 ? Constants.DefaultAdRequestAuthority : s;
        }
    }
}
