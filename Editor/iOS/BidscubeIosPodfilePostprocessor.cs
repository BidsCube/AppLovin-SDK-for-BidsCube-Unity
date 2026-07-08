#if UNITY_EDITOR
using System;
using System.IO;
using BidscubeSDK.Mediation;
using UnityEditor;
using UnityEditor.Callbacks;

namespace BidscubeSDK.Editor.iOS
{
    /// <summary>
    /// On iOS export, ensures <c>BidscubeSDKAppLovin</c> (contains <c>ALBidscubeMediationAdapter</c>) is present in the exported Podfile.
    /// Does not add <c>AppLovinSDK</c> when the official AppLovin MAX Unity SDK already declares it.
    /// </summary>
    public static class BidscubeIosPodfilePostprocessor
    {
        public const string Marker = "# __BIDSCUBE_UNITY_IOS_MAX_PODS__";

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

                if (ContainsBidscubeSdkAppLovinPod(text))
                    return;

                if (ContainsStandaloneBidscubeSdkPod(text))
                {
                    UnityEngine.Debug.LogWarning(
                        "[Bidscube AppLovin] Podfile contains standalone bidscubeSdk/BidscubeSDK. " +
                        "AppLovin MAX mediation requires pod 'BidscubeSDKAppLovin' because it contains ALBidscubeMediationAdapter.");
                    return;
                }

                var version = AdapterPackageInfo.IosBidscubeAppLovinPodVersion;
                var append = string.Join("\n", new[]
                {
                    "",
                    Marker,
                    "# Bidscube AppLovin MAX adapter (see Documentation~/INSTALL.md)",
                    $"pod 'BidscubeSDKAppLovin', '{version}'",
                    ""
                });
                File.AppendAllText(podfile, append);
                UnityEngine.Debug.Log(
                    "[Bidscube AppLovin] Appended BidscubeSDKAppLovin pod to Podfile: " + podfile);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[Bidscube AppLovin] BidscubeIosPodfilePostprocessor: " + e.Message);
            }
        }

        private static bool ContainsBidscubeSdkAppLovinPod(string podfileText)
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
            }

            return false;
        }

        private static bool ContainsStandaloneBidscubeSdkPod(string podfileText)
        {
            foreach (var line in podfileText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = line.TrimStart();
                if (t.StartsWith("#", StringComparison.Ordinal))
                    continue;
                if (t.IndexOf("pod", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                if (t.IndexOf("BidscubeSDKAppLovin", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                if (t.IndexOf("bidscubeSdk", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (t.IndexOf("'BidscubeSDK'", StringComparison.Ordinal) >= 0
                    || t.IndexOf("\"BidscubeSDK\"", StringComparison.Ordinal) >= 0)
                    return true;
            }

            return false;
        }
    }
}
#endif
