#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace BidscubeSDK.Editor.iOS
{
    /// <summary>
    /// When the exported Xcode project uses CocoaPods, ensures AppLovin MAX 13.x and the
    /// <c>BidscubeSDKAppLovin</c> pod (BidCube runtime + <c>ALBidscubeMediationAdapter</c>) are present
    /// when missing — parity with the Bidscube iOS MAX repo (github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS).
    /// Skips lines that duplicate existing <c>pod</c> declarations.
    /// </summary>
    public static class BidscubeIosPodfilePostprocessor
    {
        /// <summary>Unified block appended on new exports (keep in sync with UPM <c>package.json</c> / docs).</summary>
        public const string Marker = "# __BIDSCUBE_UNITY_IOS_MAX_PODS__";

        public const string BidscubeAppLovinPodVersion = "1.0.4";

        [PostProcessBuild(49)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
                return;

            try
            {
                var podfile = Path.Combine(pathToBuiltProject, "Podfile");
                if (!File.Exists(podfile))
                    return;

                var text = File.ReadAllText(podfile);
                if (text.Contains(Marker, StringComparison.Ordinal))
                    return;

                var needAppLovin = !ContainsAppLovinSdkPod(text);
                var needBidscubeBundle = !ContainsBidscubeIosMediationPod(text);

                if (!needAppLovin && !needBidscubeBundle)
                    return;

                var lines = new System.Collections.Generic.List<string>
                {
                    "",
                    Marker,
                    "# Bidscube Unity package: AppLovin MAX on iOS (see Documentation~/APPLOVIN_MAX.md)",
                };

                if (needAppLovin)
                    lines.Add("pod 'AppLovinSDK', '>= 13.0.0', '< 14.0'");

                if (needBidscubeBundle)
                    lines.Add($"pod 'BidscubeSDKAppLovin', '{BidscubeAppLovinPodVersion}'");

                var append = string.Join("\n", lines) + "\n";
                File.AppendAllText(podfile, append);
                UnityEngine.Debug.Log("[BidscubeSDK] Appended iOS MAX CocoaPods block to Podfile: " + podfile);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[BidscubeSDK] BidscubeIosPodfilePostprocessor: " + e.Message);
            }
        }

        /// <summary>
        /// True if the Podfile already pulls BidCube for MAX (<c>BidscubeSDKAppLovin</c>) or a standalone <c>BidscubeSDK</c> runtime pod (do not add the bundle pod on top).
        /// </summary>
        private static bool ContainsBidscubeIosMediationPod(string podfileText)
        {
            foreach (var line in podfileText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = line.TrimStart();
                if (t.StartsWith("#", StringComparison.Ordinal))
                    continue;
                if (t.IndexOf("pod", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                if (t.IndexOf("BidscubeSDKAppLovin", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (t.IndexOf("BidscubeSDK", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static bool ContainsAppLovinSdkPod(string podfileText)
        {
            foreach (var line in podfileText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = line.TrimStart();
                if (t.StartsWith("#", StringComparison.Ordinal))
                    continue;
                if (t.IndexOf("AppLovinSDK", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (t.IndexOf("applovin", StringComparison.OrdinalIgnoreCase) >= 0
                    && t.IndexOf("pod", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
#endif
