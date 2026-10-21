using System;
using System.IO;
using BidscubeSDK.Mediation;
using UnityEditor;
using UnityEditor.Android;

namespace BidscubeSDK.Editor.Android
{
    internal sealed class BidscubeAndroidGradlePostprocessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 50;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            BidscubeAndroidGradleProjectPatcher.OnPostGenerateGradleAndroidProject(
                path,
                logPrefix: "[Bidscube AppLovin]",
                packageAssembly: typeof(AdapterPackageInfo).Assembly,
                appendAppLovinSdkDependency: true,
                new BidscubeAndroidBundledCoreAarNames(
                    AdapterPackageInfo.NativeAndroidBidscubeSdkVersion,
                    AdapterPackageInfo.NativeAndroidBundledCoreAarLiteFileName,
                    AdapterPackageInfo.NativeAndroidBundledCoreAarWebViewVideoFileName,
                    AdapterPackageInfo.NativeAndroidBundledCoreAarLegacyMediaVideoFileName,
                    AdapterPackageInfo.NativeAndroidBundledCoreAarFullFileName));
            EnsureMaterialComponents(path);
        }

        /// <summary>
        /// The bundled core AAR does not publish its Maven dependency on Material Components.
        /// Older AARs reference <c>?attr/colorOnSurface</c> and fail resource linking without this line.
        /// </summary>
        static void EnsureMaterialComponents(string pathFromUnity)
        {
            const string coordinate = "com.google.android.material:material:1.12.0";
            var gradle = ResolveUnityLibraryGradle(pathFromUnity);
            if (string.IsNullOrEmpty(gradle) || !File.Exists(gradle))
                return;

            try
            {
                var content = File.ReadAllText(gradle);
                if (content.IndexOf("com.google.android.material:material", StringComparison.Ordinal) >= 0)
                    return;

                const string needle = "dependencies {";
                var idx = content.IndexOf(needle, StringComparison.Ordinal);
                if (idx < 0)
                {
                    UnityEngine.Debug.LogWarning("[Bidscube AppLovin] Could not add Material Components: dependencies block not found in " + gradle);
                    return;
                }

                var insertAt = idx + needle.Length;
                var line = "\n    implementation '" + coordinate + "'";
                content = content.Insert(insertAt, line);
                File.WriteAllText(gradle, content);
                UnityEngine.Debug.Log("[Bidscube AppLovin] Added " + coordinate + " so colorOnSurface links in the BidsCube AAR.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[Bidscube AppLovin] Could not add Material Components: " + e.Message);
            }
        }

        static string ResolveUnityLibraryGradle(string pathFromUnity)
        {
            if (string.IsNullOrEmpty(pathFromUnity))
                return null;

            var direct = Path.Combine(pathFromUnity, "build.gradle");
            if (File.Exists(direct) && pathFromUnity.Replace('\\', '/').EndsWith("unityLibrary", StringComparison.OrdinalIgnoreCase))
                return direct;

            var nested = Path.Combine(pathFromUnity, "unityLibrary", "build.gradle");
            if (File.Exists(nested))
                return nested;

            if (File.Exists(direct))
                return direct;

            return null;
        }
    }
}
