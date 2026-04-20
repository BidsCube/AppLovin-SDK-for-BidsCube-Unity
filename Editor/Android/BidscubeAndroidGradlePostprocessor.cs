#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using BidscubeSDK;
using UnityEditor.Android;
using UnityEngine;

namespace BidscubeSDK.Editor.Android
{
    /// <summary>
    /// Injects Maven dependencies for the **core** Bidscube Android SDK (<c>com.bidscube:bidscube-sdk</c>, version <see cref="Constants.NativeAndroidBidscubeSdkVersion"/>)
    /// as an <b>AAR</b> artifact (<c>@aar</c>) so Gradle does not stop at a root <c>packaging=pom</c> shell on Maven Central.
    /// Plus transitives for the bundled <b>AppLovin MAX</b> adapter AAR (local AARs do not pull their own Maven graph).
    /// The UPM package ships only <c>applovin-bidscube-max-adapter-*.aar</c>; core classes resolve from Maven Central — do <b>not</b> add a second <c>implementation 'com.bidscube:bidscube-sdk:…'</c> in Custom Gradle.
    /// Core library desugaring is **not** injected by default — see <see cref="NoDesugarMode"/>; opt in via host Gradle or set <see cref="NoDesugarMode"/> to <c>false</c> for legacy plugin-managed desugaring (still skipped if the host Gradle already declares desugaring).
    /// Also raises <c>compileSdk</c> / <c>minSdk</c> when needed so <c>CheckAarMetadata</c> passes against Material / AndroidX.
    /// </summary>
    public sealed class BidscubeAndroidGradlePostprocessor : IPostGenerateGradleAndroidProject
    {
        public const string Marker = "// __BIDSCUBE_SDK_GRADLE_DEPS__";

        /// <summary>
        /// When <c>true</c> (default), this post-processor does <b>not</b> inject <c>coreLibraryDesugaring</c>, <c>desugar_jdk_libs</c>, or <c>coreLibraryDesugaringEnabled</c>.
        /// Add desugaring in your own Gradle (Custom Base Gradle / launcher / exported templates) if your graph requires it (e.g. <c>CheckAarMetadata</c>, Java 8+ APIs on older devices).
        /// Set to <c>false</c> to restore <b>plugin-managed</b> desugaring on fresh injects only when the exported Gradle does not already declare desugaring.
        /// </summary>
        public static bool NoDesugarMode = true;

        /// <summary>Minimum AppLovin MAX Android SDK line (13.0+); resolved to latest 13.x from Maven Central.</summary>
        public const string AppLovinSdkGradleCoordinate = "com.applovin:applovin-sdk:13.+";

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
                    var pluginManagesDesugar = ShouldPluginInjectCoreLibraryDesugaring(text);

                    var desugarLine = pluginManagesDesugar
                        ? "    coreLibraryDesugaring 'com.android.tools:desugar_jdk_libs:2.1.4'\n"
                        : string.Empty;

                    var depsBlock = $@"
    {Marker}
    implementation 'com.bidscube:bidscube-sdk:{Constants.NativeAndroidBidscubeSdkVersion}@aar'
    implementation '{AppLovinSdkGradleCoordinate}'
    implementation 'androidx.media3:media3-common:1.4.1'
    implementation 'androidx.media3:media3-ui:1.4.1'
    implementation 'com.google.android.ump:user-messaging-platform:2.2.0'
    implementation 'com.google.android.gms:play-services-ads-identifier:18.0.1'
    implementation 'com.google.ads.interactivemedia.v3:interactivemedia:3.33.0'
    implementation 'androidx.cardview:cardview:1.0.0'
    implementation 'com.google.android.material:material:1.12.0'
    implementation 'com.github.bumptech.glide:glide:4.15.1'
{desugarLine}";

                    var idx = text.IndexOf("dependencies {", StringComparison.Ordinal);
                    if (idx < 0)
                    {
                        Debug.LogWarning("[BidscubeSDK] unityLibrary/build.gradle has no dependencies { block.");
                    }
                    else
                    {
                        var insertAt = idx + "dependencies {".Length;
                        text = text.Insert(insertAt, depsBlock);
                        if (pluginManagesDesugar)
                            text = EnsureCoreLibraryDesugaring(text);
                        File.WriteAllText(unityLib, text);
                        Debug.Log("[BidscubeSDK] Injected Bidscube Android SDK Maven dependencies into " + unityLib);
                    }
                }

                EnsureMinCompileSdkInFile(unityLib, MinCompileSdkForBidscubeDeps);
                EnsureMinMinSdkInFile(unityLib, MinMinSdkForBidscube);

                NormalizeBidscubeCoreSdkCoordinateInFile(unityLib);
                EnsureMavenBidscubeCoreSdk(unityLib);
                ValidateBidscubeCoreSdkUsesAarSuffix(unityLib);
            }

            var launcher = FindLauncherBuildGradle(path);
            if (!string.IsNullOrEmpty(launcher))
            {
                EnsureMinCompileSdkInFile(launcher, MinCompileSdkForBidscubeDeps);
                EnsureMinMinSdkInFile(launcher, MinMinSdkForBidscube);
                if (!NoDesugarMode)
                    EnsureLauncherCoreLibraryDesugaring(launcher);
            }

            var gradleRoot = FindGradleProjectRoot(path, unityLib, launcher);
            if (!string.IsNullOrEmpty(gradleRoot))
                EnsureGradlePropertiesForAarMetadata(gradleRoot);

            if (NoDesugarMode)
            {
                Debug.LogWarning(
                    "[BidscubeSDK] Android Gradle export: NoDesugarMode=true (default). This plugin does not inject coreLibraryDesugaring, " +
                    "desugar_jdk_libs, or coreLibraryDesugaringEnabled. If your dependency graph needs Java 8+ desugaring (e.g. CheckAarMetadata), " +
                    "add it in your host Gradle (Custom Base Gradle / mainTemplate / launcher). Set BidscubeAndroidGradlePostprocessor.NoDesugarMode=false " +
                    "only if you want legacy plugin-managed desugaring on fresh exports when Gradle does not already declare it. " +
                    "Validate with a clean Gradle cache, ./gradlew --refresh-dependencies, assembleDebug/assembleRelease, and a lower-API device or emulator smoke test.");
            }
        }

        /// <summary>
        /// Plugin-managed desugaring only when the integrator opted out of <see cref="NoDesugarMode"/> and the exported <c>unityLibrary/build.gradle</c>
        /// does not already declare core library desugaring (host-owned Gradle must not be overridden).
        /// </summary>
        private static bool ShouldPluginInjectCoreLibraryDesugaring(string unityLibraryGradleText)
        {
            if (NoDesugarMode)
                return false;
            return !GradleAlreadyDeclaresCoreLibraryDesugaring(unityLibraryGradleText);
        }

        /// <summary>
        /// True when the Gradle script already sets up core library desugaring (host / template / prior export). Do not inject or flip flags.
        /// </summary>
        private static bool GradleAlreadyDeclaresCoreLibraryDesugaring(string gradleText)
        {
            if (string.IsNullOrEmpty(gradleText))
                return false;
            if (gradleText.Contains("coreLibraryDesugaringEnabled"))
                return true;
            if (gradleText.Contains("desugar_jdk_libs"))
                return true;
            return Regex.IsMatch(gradleText, @"coreLibraryDesugaring\s+['""]", RegexOptions.Multiline);
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

        /// <summary>
        /// When <see cref="NoDesugarMode"/> is <c>false</c>, mirrors plugin-managed desugaring onto <b>launcher</b> for AGP 8 <c>CheckAarMetadata</c> — only if the launcher Gradle does not already declare desugaring.
        /// </summary>
        private static void EnsureLauncherCoreLibraryDesugaring(string launcherGradlePath)
        {
            try
            {
                if (!File.Exists(launcherGradlePath))
                    return;

                var text = File.ReadAllText(launcherGradlePath);
                if (GradleAlreadyDeclaresCoreLibraryDesugaring(text))
                    return;

                var original = text;

                if (!text.Contains("coreLibraryDesugaringEnabled"))
                {
                    var withFlag = EnsureCoreLibraryDesugaring(text);
                    if (ReferenceEquals(withFlag, text) || !withFlag.Contains("coreLibraryDesugaringEnabled"))
                    {
                        const string androidNeedle = "android {";
                        var ai = text.IndexOf(androidNeedle, StringComparison.Ordinal);
                        if (ai >= 0)
                        {
                            var insert = ai + androidNeedle.Length;
                            text = text.Insert(
                                insert,
                                "\n    compileOptions {\n        coreLibraryDesugaringEnabled true\n    }");
                        }
                    }
                    else
                    {
                        text = withFlag;
                    }
                }

                const string desugarCoord = "coreLibraryDesugaring 'com.android.tools:desugar_jdk_libs:2.1.4'";
                if (!text.Contains("desugar_jdk_libs"))
                {
                    var depIdx = text.IndexOf("dependencies {", StringComparison.Ordinal);
                    if (depIdx >= 0)
                    {
                        var ins = depIdx + "dependencies {".Length;
                        text = text.Insert(ins, $"\n    {desugarCoord}\n");
                    }
                }

                if (text != original)
                {
                    File.WriteAllText(launcherGradlePath, text);
                    Debug.Log("[BidscubeSDK] Enabled core library desugaring on launcher module: " + launcherGradlePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] EnsureLauncherCoreLibraryDesugaring: {e.Message}");
            }
        }

        /// <summary>
        /// Rewrites any <c>implementation</c> line for <c>com.bidscube:bidscube-sdk:…</c> missing <c>@aar</c> to the <c>@aar</c> form (legacy exports after <see cref="Marker"/>).
        /// </summary>
        private static string NormalizeBidscubeCoreSdkImplementationToAar(string gradleText)
        {
            return Regex.Replace(
                gradleText,
                @"implementation\s+(['""])(com\.bidscube:bidscube-sdk:)([^'""]+)\1",
                m =>
                {
                    var q = m.Groups[1].Value;
                    var prefix = m.Groups[2].Value;
                    var verOrCoord = m.Groups[3].Value.Trim();
                    if (verOrCoord.EndsWith("@aar", StringComparison.Ordinal))
                        return m.Value;
                    return $"implementation {q}{prefix}{verOrCoord}@aar{q}";
                },
                RegexOptions.Multiline);
        }

        private static void NormalizeBidscubeCoreSdkCoordinateInFile(string gradlePath)
        {
            try
            {
                if (string.IsNullOrEmpty(gradlePath) || !File.Exists(gradlePath))
                    return;

                var text = File.ReadAllText(gradlePath);
                var next = NormalizeBidscubeCoreSdkImplementationToAar(text);
                if (next == text)
                    return;

                File.WriteAllText(gradlePath, next);
                Debug.Log("[BidscubeSDK] Normalized com.bidscube:bidscube-sdk Gradle line(s) to use @aar in " + gradlePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] NormalizeBidscubeCoreSdkCoordinateInFile: {e.Message}");
            }
        }

        private static void ValidateBidscubeCoreSdkUsesAarSuffix(string unityLibGradlePath)
        {
            try
            {
                if (string.IsNullOrEmpty(unityLibGradlePath) || !File.Exists(unityLibGradlePath))
                    return;

                var text = File.ReadAllText(unityLibGradlePath);
                if (!text.Contains(Marker))
                    return;

                var ver = Regex.Escape(Constants.NativeAndroidBidscubeSdkVersion);
                if (Regex.IsMatch(
                        text,
                        @"implementation\s+['""]com\.bidscube:bidscube-sdk:" + ver + @"@aar['""]",
                        RegexOptions.Multiline))
                    return;

                Debug.LogError(
                    "Bidscube core SDK dependency was injected without @aar; Android Java classes may be missing at runtime.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] ValidateBidscubeCoreSdkUsesAarSuffix: {e.Message}");
            }
        }

        /// <summary>
        /// Older Unity exports had our <see cref="Marker"/> block without a Maven line for the core SDK (core used to ship as a local AAR).
        /// Inserts <c>implementation 'com.bidscube:bidscube-sdk:…@aar'</c> immediately after the marker when missing.
        /// </summary>
        private static void EnsureMavenBidscubeCoreSdk(string unityLibGradlePath)
        {
            try
            {
                if (string.IsNullOrEmpty(unityLibGradlePath) || !File.Exists(unityLibGradlePath))
                    return;

                var text = File.ReadAllText(unityLibGradlePath);
                text = NormalizeBidscubeCoreSdkImplementationToAar(text);

                var ver = Regex.Escape(Constants.NativeAndroidBidscubeSdkVersion);
                if (Regex.IsMatch(
                        text,
                        @"implementation\s+['""]com\.bidscube:bidscube-sdk:" + ver + @"@aar['""]",
                        RegexOptions.Multiline))
                {
                    File.WriteAllText(unityLibGradlePath, text);
                    return;
                }

                var markerIdx = text.IndexOf(Marker, StringComparison.Ordinal);
                if (markerIdx < 0)
                {
                    File.WriteAllText(unityLibGradlePath, text);
                    return;
                }

                var line = "\n    implementation 'com.bidscube:bidscube-sdk:" + Constants.NativeAndroidBidscubeSdkVersion + "@aar'";
                text = text.Insert(markerIdx + Marker.Length, line);
                File.WriteAllText(unityLibGradlePath, text);
                Debug.Log("[BidscubeSDK] Injected Maven core SDK (@aar) after " + Marker + ".");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] EnsureMavenBidscubeCoreSdk: {e.Message}");
            }
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
