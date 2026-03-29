#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace BidscubeSDK.Editor
{
    /// <summary>
    /// Manual parity checks for SSP authority normalization and HTTPS base URL (Android reference).
    /// </summary>
    internal static class SspUrlParityMenu
    {
        [MenuItem("Bidscube/Validate SSP URL parity (console)")]
        private static void Run()
        {
            var failed = 0;
            failed += Check("default null", AdRequestAuthorityNormalizer.Normalize(null), Constants.DefaultAdRequestAuthority);
            failed += Check("empty", AdRequestAuthorityNormalizer.Normalize(string.Empty), Constants.DefaultAdRequestAuthority);
            failed += Check(
                "https strip",
                AdRequestAuthorityNormalizer.Normalize("https://example.com/sdk"),
                "example.com");
            failed += Check("percent colon", AdRequestAuthorityNormalizer.Normalize("example.com%3A443"), "example.com:443");
            failed += Check("host port", AdRequestAuthorityNormalizer.Normalize("127.0.0.1:8787"), "127.0.0.1:8787");

            var u1 = SspAdUriHelper.BuildHttpsSdkBaseUrl("127.0.0.1:8787");
            if (!string.Equals(u1, "https://127.0.0.1:8787/sdk", StringComparison.Ordinal))
            {
                Debug.LogError($"Expected https://127.0.0.1:8787/sdk, got {u1}");
                failed++;
            }

            if (u1.Contains("%3A"))
            {
                Debug.LogError("Port must not be percent-encoded in authority.");
                failed++;
            }

            var cfg = new SDKConfig.Builder().AdRequestAuthority("ssp-bcc-ads.com").Build();
            if (!cfg.BaseURL.StartsWith("https://ssp-bcc-ads.com/sdk", StringComparison.Ordinal))
            {
                Debug.LogError($"SDKConfig.BaseURL unexpected: {cfg.BaseURL}");
                failed++;
            }

            if (failed == 0)
            {
                Debug.Log("[Bidscube] SSP URL parity checks passed.");
            }
            else
            {
                Debug.LogError($"[Bidscube] SSP URL parity checks failed ({failed}).");
            }
        }

        private static int Check(string label, string actual, string expected)
        {
            if (string.Equals(actual, expected, StringComparison.Ordinal))
            {
                return 0;
            }

            Debug.LogError($"[{label}] expected {expected}, got {actual}");
            return 1;
        }
    }
}
#endif
