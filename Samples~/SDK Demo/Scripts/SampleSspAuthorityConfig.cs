using System;

namespace BidscubeSDK
{
    /// <summary>
    /// Shared sample logic: optional <see cref="SDKConfig.Builder.AdRequestAuthority"/> vs legacy <see cref="SDKConfig.Builder.BaseURL"/>.
    /// Editor: <c>BIDSCUBE_SSP_AUTHORITY</c> env overrides (parity with Android test hooks).
    /// </summary>
    public static class SampleSspAuthorityConfig
    {
        internal static void ApplyTo(SDKConfig.Builder builder, string baseUrlFallback, string optionalAuthority)
        {
#if UNITY_EDITOR
            var env = Environment.GetEnvironmentVariable("BIDSCUBE_SSP_AUTHORITY");
            if (!string.IsNullOrWhiteSpace(env))
            {
                builder.AdRequestAuthority(env);
                return;
            }
#endif
            if (!string.IsNullOrWhiteSpace(optionalAuthority))
            {
                builder.AdRequestAuthority(optionalAuthority);
            }
            else
            {
                builder.BaseURL(baseUrlFallback);
            }
        }
    }
}
