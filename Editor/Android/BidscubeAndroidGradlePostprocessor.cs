using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BidscubeSDK;
using BidscubeSDK.Editor;
using BidscubeSDK.Mediation;
using UnityEditor;
using UnityEditor.PackageManager;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace BidscubeSDK.Editor.Android
{
#if UNITY_ANDROID
    /// <summary>
    /// Copies the correct Bidscube core AAR into unityLibrary/libs and injects IMA/Media3 only for FullWithVideo.
    /// </summary>
    internal sealed class BidscubeAndroidGradlePostprocessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 50;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var featureSet = BidscubeFeatureSetStore.Load();
            var pkgRoot = ResolvePackageRoot();
            if (string.IsNullOrEmpty(pkgRoot))
            {
                Debug.LogWarning("[Bidscube] Could not resolve UPM package root; skipping Gradle/AAR integration.");
                return;
            }

            var plugins = Path.Combine(pkgRoot, "Runtime", "Plugins", "Android");
            var ver = AdapterPackageInfo.NativeAndroidBidscubeSdkVersion;
            var liteSrc = Path.Combine(plugins, $"bidscube-sdk-lite-{ver}.aar");
            var fullSrc = Path.Combine(plugins, $"bidscube-sdk-{ver}.aar");
            var libsDir = Path.Combine(path, "unityLibrary", "libs");
            Directory.CreateDirectory(libsDir);

            var liteDst = Path.Combine(libsDir, $"bidscube-sdk-lite-{ver}.aar");
            var fullDst = Path.Combine(libsDir, $"bidscube-sdk-{ver}.aar");

            if (featureSet == BidscubeFeatureSet.LiteNoVideo)
            {
                if (!File.Exists(liteSrc))
                {
                    Debug.LogError($"[Bidscube] LiteNoVideo: missing bundled lite AAR at {liteSrc}");
                    return;
                }

                File.Copy(liteSrc, liteDst, true);
                TryDelete(fullDst);
                PatchUnityLibraryGradle(path, featureSet, useMavenFullCore: false, fullCoreLocalPath: null);
            }
            else
            {
                TryDelete(liteDst);
                if (File.Exists(fullSrc))
                {
                    File.Copy(fullSrc, fullDst, true);
                    PatchUnityLibraryGradle(path, featureSet, useMavenFullCore: false, fullCoreLocalPath: fullDst);
                }
                else
                {
                    TryDelete(fullDst);
                    Debug.LogWarning(
                        $"[Bidscube] FullWithVideo: bidscube-sdk-{ver}.aar not found under Plugins/Android; " +
                        $"using Maven coordinate com.bidscube:bidscube-sdk:{ver}@aar. Add the full AAR for offline/air-gapped builds.");
                    PatchUnityLibraryGradle(path, featureSet, useMavenFullCore: true, fullCoreLocalPath: null);
                }
            }
        }

        static void TryDelete(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Bidscube] Could not delete {file}: {e.Message}");
            }
        }

        static void PatchUnityLibraryGradle(string gradleProjectRoot, BidscubeFeatureSet featureSet, bool useMavenFullCore,
            string fullCoreLocalPath)
        {
            var gradlePath = Path.Combine(gradleProjectRoot, "unityLibrary", "build.gradle");
            if (!File.Exists(gradlePath))
            {
                Debug.LogWarning($"[Bidscube] unityLibrary/build.gradle not found at {gradlePath}");
                return;
            }

            var content = File.ReadAllText(gradlePath);
            var ver = AdapterPackageInfo.NativeAndroidBidscubeSdkVersion;
            var sb = new StringBuilder();
            sb.AppendLine("// __BIDSCUBE_ANDROID_MANAGED_START__");
            if (featureSet == BidscubeFeatureSet.LiteNoVideo)
            {
                sb.AppendLine($"    implementation files('libs/bidscube-sdk-lite-{ver}.aar')");
            }
            else if (!useMavenFullCore && !string.IsNullOrEmpty(fullCoreLocalPath))
            {
                sb.AppendLine($"    implementation files('libs/bidscube-sdk-{ver}.aar')");
            }
            else
            {
                sb.AppendLine($"    implementation 'com.bidscube:bidscube-sdk:{ver}@aar'");
            }

            if (featureSet == BidscubeFeatureSet.FullWithVideo)
            {
                sb.AppendLine("    implementation 'androidx.media3:media3-common:1.4.1'");
                sb.AppendLine("    implementation 'androidx.media3:media3-ui:1.4.1'");
                sb.AppendLine("    implementation 'com.google.ads.interactivemedia.v3:interactivemedia:3.33.0'");
            }

            sb.AppendLine("// __BIDSCUBE_ANDROID_MANAGED_END__");

            var inner = sb.ToString();
            const string start = "// __BIDSCUBE_ANDROID_MANAGED_START__";
            const string end = "// __BIDSCUBE_ANDROID_MANAGED_END__";
            if (content.Contains(start))
            {
                var pattern = new Regex(Regex.Escape(start) + "[\\s\\S]*?" + Regex.Escape(end),
                    RegexOptions.Multiline);
                content = pattern.Replace(content, inner, 1);
            }
            else
                content = InjectAfterDependenciesOpen(content, inner);

            File.WriteAllText(gradlePath, content);
        }

        static string InjectAfterDependenciesOpen(string gradle, string block)
        {
            var idx = gradle.IndexOf("dependencies", StringComparison.Ordinal);
            if (idx < 0)
                return gradle + "\n" + block + "\n";
            var brace = gradle.IndexOf('{', idx);
            if (brace < 0)
                return gradle + "\n" + block + "\n";
            var insertAt = brace + 1;
            return gradle.Insert(insertAt, "\n" + block);
        }

        static string ResolvePackageRoot()
        {
            try
            {
                var asm = typeof(AdapterPackageInfo).Assembly;
                var info = PackageInfo.FindForAssembly(asm);
                if (info != null && !string.IsNullOrEmpty(info.resolvedPath))
                    return info.resolvedPath;
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
#endif
}
