#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Android;
using UnityEngine;

namespace BidscubeSDK.Editor.Android
{
    /// <summary>
    /// Injects Maven dependencies required by the bundled <c>bidscube-sdk</c> AAR (they are not pulled transitively from a local AAR).
    /// Mirrors the native Bidscube Android SDK Gradle dependency block + core library desugaring.
    /// Also raises <c>compileSdk</c> / <c>minSdk</c> when needed so <c>CheckAarMetadata</c> passes against Material / AndroidX.
    /// </summary>
    public sealed class BidscubeAndroidGradlePostprocessor : IPostGenerateGradleAndroidProject
    {
        public const string Marker = "// __BIDSCUBE_SDK_GRADLE_DEPS__";

        /// <summary>Bundled AndroidX / Material / IMA AAR metadata often requires API 34+; Unity 6 may use 36.</summary>
        private const int MinCompileSdkForBidscubeDeps = 34;

        /// <summary>
        /// Some transitive deps (e.g. newer Play / AndroidX) advertise <c>minSdk</c> &gt; 24 in AAR metadata;
        /// Unity-exported <c>minSdk</c> must be &gt;= that value or <c>CheckAarMetadata</c> fails.
        /// </summary>
        private const int MinMinSdkForBidscube = 26;

        private const string GradlePropsMarker = "# __BIDSCUBE_GRADLE_PROPERTIES__";

        public int callbackOrder => 50;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var unityLib = FindUnityLibraryBuildGradle(path);
            if (string.IsNullOrEmpty(unityLib))
            {
                Debug.LogWarning("[BidscubeSDK] Could not find unityLibrary/build.gradle to inject Bidscube dependencies.");
            }
            else
            {
                var text = File.ReadAllText(unityLib);
                if (!text.Contains(Marker))
                {
                    const string depsBlock = @"
    " + Marker + @"
    implementation 'androidx.media3:media3-common:1.4.1'
    implementation 'androidx.media3:media3-ui:1.4.1'
    implementation 'com.google.android.ump:user-messaging-platform:2.2.0'
    implementation 'com.google.android.gms:play-services-ads-identifier:18.0.1'
    implementation 'com.google.ads.interactivemedia.v3:interactivemedia:3.33.0'
    implementation 'androidx.cardview:cardview:1.0.0'
    implementation 'com.google.android.material:material:1.12.0'
    implementation 'com.github.bumptech.glide:glide:4.15.1'
    coreLibraryDesugaring 'com.android.tools:desugar_jdk_libs:2.1.4'
";

                    var idx = text.IndexOf("dependencies {", StringComparison.Ordinal);
                    if (idx < 0)
                    {
                        Debug.LogWarning("[BidscubeSDK] unityLibrary/build.gradle has no dependencies { block.");
                    }
                    else
                    {
                        var insertAt = idx + "dependencies {".Length;
                        text = text.Insert(insertAt, depsBlock);
                        text = EnsureCoreLibraryDesugaring(text);
                        File.WriteAllText(unityLib, text);
                        Debug.Log("[BidscubeSDK] Injected Bidscube Android SDK Maven dependencies into " + unityLib);
                    }
                }

                EnsureMinCompileSdkInFile(unityLib, MinCompileSdkForBidscubeDeps);
                EnsureMinMinSdkInFile(unityLib, MinMinSdkForBidscube);
                TryUpgradeDesugarLibs(unityLib);
            }

            var launcher = FindLauncherBuildGradle(path);
            if (!string.IsNullOrEmpty(launcher))
            {
                EnsureMinCompileSdkInFile(launcher, MinCompileSdkForBidscubeDeps);
                EnsureMinMinSdkInFile(launcher, MinMinSdkForBidscube);
            }

            var gradleRoot = FindGradleProjectRoot(path, unityLib, launcher);
            if (!string.IsNullOrEmpty(gradleRoot))
                EnsureGradlePropertiesForAarMetadata(gradleRoot);
        }

        /// <summary>
        /// When <c>compileSdk</c> is newer than versions some AARs declare in metadata, AGP 8+ can still fail
        /// <c>CheckAarMetadata</c>. This property tells the build to allow those API levels (comma-separated).
        /// </summary>
        private static void EnsureGradlePropertiesForAarMetadata(string gradleProjectRoot)
        {
            try
            {
                var propsPath = Path.Combine(gradleProjectRoot, "gradle.properties");
                if (!File.Exists(propsPath))
                    return;

                var text = File.ReadAllText(propsPath);
                if (text.Contains(GradlePropsMarker))
                    return;

                // API levels: Unity 6 often uses compileSdk 35–36 while some Maven AARs only declare support up to 34 in metadata.
                var append = $@"
{GradlePropsMarker}
android.suppressUnsupportedCompileSdk=34,35,36
";
                File.AppendAllText(propsPath, append);
                Debug.Log("[BidscubeSDK] Appended android.suppressUnsupportedCompileSdk to " + propsPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] EnsureGradlePropertiesForAarMetadata: {e.Message}");
            }
        }

        private static string FindGradleProjectRoot(string path, string unityLibGradle, string launcherGradle)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var rootProps = Path.Combine(path, "gradle.properties");
                if (File.Exists(rootProps))
                    return path;
            }

            try
            {
                if (!string.IsNullOrEmpty(unityLibGradle))
                {
                    var dir = Directory.GetParent(Path.GetDirectoryName(unityLibGradle) ?? "")?.FullName;
                    if (!string.IsNullOrEmpty(dir) && File.Exists(Path.Combine(dir, "gradle.properties")))
                        return dir;
                }
            }
            catch { /* ignore */ }

            try
            {
                if (!string.IsNullOrEmpty(launcherGradle))
                {
                    var dir = Directory.GetParent(Path.GetDirectoryName(launcherGradle) ?? "")?.FullName;
                    if (!string.IsNullOrEmpty(dir) && File.Exists(Path.Combine(dir, "gradle.properties")))
                        return dir;
                }
            }
            catch { /* ignore */ }

            return null;
        }

        private static void TryUpgradeDesugarLibs(string gradlePath)
        {
            try
            {
                if (!File.Exists(gradlePath))
                    return;
                var text = File.ReadAllText(gradlePath);
                if (!text.Contains("desugar_jdk_libs:2.0.4"))
                    return;
                text = text.Replace("desugar_jdk_libs:2.0.4", "desugar_jdk_libs:2.1.4");
                File.WriteAllText(gradlePath, text);
                Debug.Log("[BidscubeSDK] Upgraded coreLibraryDesugaring to desugar_jdk_libs:2.1.4");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] TryUpgradeDesugarLibs: {e.Message}");
            }
        }

        private static void EnsureMinCompileSdkInFile(string gradlePath, int minCompileSdk)
        {
            try
            {
                if (!File.Exists(gradlePath))
                    return;

                var text = File.ReadAllText(gradlePath);
                var original = text;

                text = Regex.Replace(
                    text,
                    @"^(\s*)compileSdkVersion\s+(\d+)\s*$",
                    m =>
                    {
                        var v = int.Parse(m.Groups[2].Value);
                        var n = Math.Max(v, minCompileSdk);
                        return $"{m.Groups[1].Value}compileSdkVersion {n}";
                    },
                    RegexOptions.Multiline);

                text = Regex.Replace(
                    text,
                    @"^(\s*)compileSdk\s+(\d+)\s*$",
                    m =>
                    {
                        var v = int.Parse(m.Groups[2].Value);
                        var n = Math.Max(v, minCompileSdk);
                        return $"{m.Groups[1].Value}compileSdk {n}";
                    },
                    RegexOptions.Multiline);

                if (text != original)
                {
                    File.WriteAllText(gradlePath, text);
                    Debug.Log($"[BidscubeSDK] Ensured compileSdk >= {minCompileSdk} in {gradlePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] EnsureMinCompileSdk failed for {gradlePath}: {e.Message}");
            }
        }

        private static void EnsureMinMinSdkInFile(string gradlePath, int minMinSdk)
        {
            try
            {
                if (!File.Exists(gradlePath))
                    return;

                var text = File.ReadAllText(gradlePath);
                var original = text;

                text = Regex.Replace(
                    text,
                    @"^(\s*)minSdkVersion\s+(\d+)\s*$",
                    m =>
                    {
                        var v = int.Parse(m.Groups[2].Value);
                        var n = Math.Max(v, minMinSdk);
                        return $"{m.Groups[1].Value}minSdkVersion {n}";
                    },
                    RegexOptions.Multiline);

                text = Regex.Replace(
                    text,
                    @"^(\s*)minSdk\s+(\d+)\s*$",
                    m =>
                    {
                        var v = int.Parse(m.Groups[2].Value);
                        var n = Math.Max(v, minMinSdk);
                        return $"{m.Groups[1].Value}minSdk {n}";
                    },
                    RegexOptions.Multiline);

                if (text != original)
                {
                    File.WriteAllText(gradlePath, text);
                    Debug.Log($"[BidscubeSDK] Ensured minSdk >= {minMinSdk} in {gradlePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] EnsureMinMinSdk failed for {gradlePath}: {e.Message}");
            }
        }

        private static string EnsureCoreLibraryDesugaring(string gradle)
        {
            if (gradle.Contains("coreLibraryDesugaringEnabled"))
                return gradle;

            const string needle = "compileOptions {";
            var i = gradle.IndexOf(needle, StringComparison.Ordinal);
            if (i < 0)
                return gradle;

            var insert = i + needle.Length;
            return gradle.Insert(insert, "\n        coreLibraryDesugaringEnabled true");
        }

        private static string FindUnityLibraryBuildGradle(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var nested = Path.Combine(path, "unityLibrary", "build.gradle");
            if (File.Exists(nested))
                return nested;

            var parent = Directory.GetParent(path)?.FullName;
            if (!string.IsNullOrEmpty(parent))
            {
                var up = Path.Combine(parent, "unityLibrary", "build.gradle");
                if (File.Exists(up))
                    return up;
            }

            var direct = Path.Combine(path, "build.gradle");
            if (File.Exists(direct))
            {
                try
                {
                    var head = File.ReadAllText(direct);
                    if (head.Contains("com.android.library") && head.Contains("dependencies"))
                        return direct;
                }
                catch { /* ignore */ }
            }

            return null;
        }

        private static string FindLauncherBuildGradle(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var nested = Path.Combine(path, "launcher", "build.gradle");
            if (File.Exists(nested))
                return nested;

            var parent = Directory.GetParent(path)?.FullName;
            if (!string.IsNullOrEmpty(parent))
            {
                var up = Path.Combine(parent, "launcher", "build.gradle");
                if (File.Exists(up))
                    return up;
            }

            return null;
        }
    }
}
#endif
