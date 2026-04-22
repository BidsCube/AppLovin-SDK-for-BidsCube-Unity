#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BidscubeSDK;
using UnityEditor.Android;
using UnityEngine;

namespace BidscubeSDK.Editor.Android
{
    /// <summary>
    /// How the **core** Bidscube Android SDK (<c>com.bidscube.sdk.*</c>) is added to <c>unityLibrary/build.gradle</c>.
    /// Default <see cref="BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar"/> keeps backward-compatible <c>implementation 'com.bidscube:bidscube-sdk:…@aar'</c> (any reachable Gradle repo — Central, mirror, <c>mavenLocal()</c>, private host).
    /// </summary>
    public enum BidscubeAndroidCoreDependencyMode
    {
        /// <summary>Inject <c>implementation 'com.bidscube:bidscube-sdk:{Constants.NativeAndroidBidscubeSdkVersion}@aar'</c> after <see cref="BidscubeAndroidGradlePostprocessor.Marker"/>.</summary>
        MavenBidscubeSdkAar = 0,

        /// <summary>Inject <see cref="BidscubeAndroidGradlePostprocessor.CustomCoreImplementationGradleLines"/> after the marker (local <c>files(...)</c>, <c>flatDir</c>, <c>project(...)</c>, etc.).</summary>
        CustomGradleLines = 1,

        /// <summary>Do not inject core; integrator must declare exactly one core SDK on the classpath (duplicate classes if two).</summary>
        SkipInjectionIntegratorOwnsCore = 2,
    }

    /// <summary>
    /// Injects Gradle dependencies for the **core** Bidscube Android SDK and transitives for the bundled <b>AppLovin MAX</b> adapter AAR.
    /// Core resolution is controlled by <see cref="CoreDependencyMode"/> (default: Maven coordinate with <c>@aar</c>).
    /// Plus transitives for the bundled <b>AppLovin MAX</b> adapter AAR (local AARs do not pull their own Maven graph).
    /// The UPM package ships <c>applovin-bidscube-max-adapter-*.aar</c> (Android-enabled) and a <b>reference</b> <c>bidscube-sdk-*.aar</c> with Android import disabled — copy/use per docs; do <b>not</b> add a second core <c>implementation</c> in Custom Gradle.
    /// When <see cref="NoDesugarMode"/> is <c>false</c> (default), appends <b>launcher</b> Gradle lines for <c>coreLibraryDesugaringEnabled</c> and <c>com.android.tools:desugar_jdk_libs</c> so <c>CheckAarMetadata</c> passes for <c>com.bidscube:bidscube-sdk</c> (AAR metadata requires desugaring on <c>:launcher</c>). Set <see cref="NoDesugarMode"/> to <c>true</c> to skip that injection and own desugaring in Custom Launcher / Base Gradle.
    /// Also raises <c>compileSdk</c> / <c>minSdk</c> when needed so <c>CheckAarMetadata</c> passes against Material / AndroidX.
    /// </summary>
    public sealed class BidscubeAndroidGradlePostprocessor : IPostGenerateGradleAndroidProject
    {
        public const string Marker = "// __BIDSCUBE_SDK_GRADLE_DEPS__";

        /// <summary>
        /// When <c>false</c> (default), the post-processor injects <b>launcher</b> <c>coreLibraryDesugaring</c> / <c>coreLibraryDesugaringEnabled</c> (idempotent) so <c>com.bidscube:bidscube-sdk</c> AAR metadata checks pass.
        /// When <c>true</c>, skips that injection and logs an Editor <b>warning</b> — use if you already declare desugaring in Custom Launcher / Base Gradle and want to avoid duplicate lines.
        /// </summary>
        public static bool NoDesugarMode = false;

        /// <summary>Pinned <c>desugar_jdk_libs</c> for <see cref="EnsureLauncherCoreLibraryDesugaring"/> (AGP 8.x).</summary>
        public const string DesugarJdkLibsVersion = "2.1.4";

        /// <summary>How <c>unityLibrary</c> obtains the core Bidscube Android SDK. Default: Maven-style coordinate with <c>@aar</c> (repo is host-defined — Central, mirror, private, <c>mavenLocal()</c>).</summary>
        public static BidscubeAndroidCoreDependencyMode CoreDependencyMode = BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar;

        /// <summary>
        /// When <see cref="CoreDependencyMode"/> is <see cref="BidscubeAndroidCoreDependencyMode.CustomGradleLines"/>, Gradle line(s) inserted after <see cref="Marker"/>.
        /// Example: <c>"    implementation files('libs/bidscube-sdk-1.2.3.aar')\n"</c>. Leading/trailing whitespace trimmed; final newline optional.
        /// </summary>
        public static string CustomCoreImplementationGradleLines = null;

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
                    var coreLines = BuildCoreGradleLinesForMarkerBlock();
                    var depsBlock = $@"
    {Marker}
{coreLines}    implementation '{AppLovinSdkGradleCoordinate}'
    implementation 'androidx.media3:media3-common:1.4.1'
    implementation 'androidx.media3:media3-ui:1.4.1'
    implementation 'com.google.android.ump:user-messaging-platform:2.2.0'
    implementation 'com.google.android.gms:play-services-ads-identifier:18.0.1'
    implementation 'com.google.ads.interactivemedia.v3:interactivemedia:3.33.0'
    implementation 'androidx.cardview:cardview:1.0.0'
    implementation 'com.google.android.material:material:1.12.0'
    implementation 'com.github.bumptech.glide:glide:4.15.1'
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
                        File.WriteAllText(unityLib, text);
                        Debug.Log("[BidscubeSDK] Injected Bidscube Android SDK Maven dependencies into " + unityLib);
                    }
                }

                EnsureMinCompileSdkInFile(unityLib, MinCompileSdkForBidscubeDeps);
                EnsureMinMinSdkInFile(unityLib, MinMinSdkForBidscube);

                NormalizeBidscubeCoreSdkCoordinateInFile(unityLib);
                EnsureCoreBidscubeSdkAfterMarker(unityLib);
                ValidateCoreBidscubeSdkDependency(unityLib);
            }

            var launcher = FindLauncherBuildGradle(path);
            if (!string.IsNullOrEmpty(launcher))
            {
                EnsureMinCompileSdkInFile(launcher, MinCompileSdkForBidscubeDeps);
                EnsureMinMinSdkInFile(launcher, MinMinSdkForBidscube);
                EnsureLauncherCoreLibraryDesugaring(launcher);
            }

            var gradleRoot = FindGradleProjectRoot(path, unityLib, launcher);
            if (!string.IsNullOrEmpty(gradleRoot))
                EnsureGradlePropertiesForAarMetadata(gradleRoot);

            if (NoDesugarMode)
            {
                Debug.LogWarning(
                    "[BidscubeSDK] Android Gradle export: NoDesugarMode=true — skipping launcher coreLibraryDesugaring injection. " +
                    "Ensure host Gradle enables coreLibraryDesugaring for :launcher (e.g. bidscube-sdk CheckAarMetadata). " +
                    "Default is NoDesugarMode=false (plugin injects desugar_jdk_libs " + DesugarJdkLibsVersion + ").");
            }
        }

        /// <summary>
        /// Satisfies AGP <c>CheckAarMetadata</c> when the core SDK AAR declares that <c>:launcher</c> must enable core library desugaring.
        /// Idempotent via <c>// __BIDSCUBE_CORE_LIBRARY_DESUGARING__</c> markers; skipped when <see cref="NoDesugarMode"/> is <c>true</c> or the file already has equivalent lines.
        /// </summary>
        private static void EnsureLauncherCoreLibraryDesugaring(string launcherGradlePath)
        {
            if (NoDesugarMode || string.IsNullOrEmpty(launcherGradlePath) || !File.Exists(launcherGradlePath))
                return;

            try
            {
                var text = File.ReadAllText(launcherGradlePath);
                if (text.Contains("__BIDSCUBE_CORE_LIBRARY_DESUGARING__", StringComparison.Ordinal))
                    return;

                var hasEnabled = Regex.IsMatch(text, @"coreLibraryDesugaringEnabled\s+true", RegexOptions.Multiline);
                var hasDep = Regex.IsMatch(text, @"coreLibraryDesugaring\s+['\"]", RegexOptions.Multiline);
                if (hasEnabled && hasDep)
                    return;

                var desugarCoordinate = "com.android.tools:desugar_jdk_libs:" + DesugarJdkLibsVersion;
                var original = text;

                if (!hasEnabled)
                {
                    var injected = Regex.Replace(
                        text,
                        @"(compileOptions\s*\{\r?\n)",
                        "$1        // __BIDSCUBE_CORE_LIBRARY_DESUGARING__\n        coreLibraryDesugaringEnabled true\n",
                        RegexOptions.Multiline);
                    if (injected == text)
                    {
                        Debug.LogWarning(
                            "[BidscubeSDK] launcher/build.gradle: no compileOptions { block found; cannot inject coreLibraryDesugaringEnabled. " +
                            "Add compileOptions + coreLibraryDesugaring in Custom Launcher Gradle, or use Unity's default launcher template.");
                    }
                    else
                        text = injected;
                }

                if (!Regex.IsMatch(text, @"coreLibraryDesugaring\s+['\"]", RegexOptions.Multiline))
                {
                    var injectedDeps = Regex.Replace(
                        text,
                        @"(dependencies\s*\{\r?\n)",
                        "$1    // __BIDSCUBE_CORE_LIBRARY_DESUGARING_DEPS__\n    coreLibraryDesugaring '" + desugarCoordinate + "'\n",
                        RegexOptions.Multiline);
                    if (injectedDeps == text)
                    {
                        Debug.LogWarning(
                            "[BidscubeSDK] launcher/build.gradle: no dependencies { block found; cannot inject coreLibraryDesugaring dependency.");
                    }
                    else
                        text = injectedDeps;
                }

                if (text == original)
                    return;

                File.WriteAllText(launcherGradlePath, text);
                Debug.Log("[BidscubeSDK] Injected core library desugaring into launcher/build.gradle (" + desugarCoordinate + " / CheckAarMetadata).");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] EnsureLauncherCoreLibraryDesugaring: {e.Message}");
            }
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
                if (CoreDependencyMode != BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar)
                    return;

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

        private static string BuildCoreGradleLinesForMarkerBlock()
        {
            switch (CoreDependencyMode)
            {
                case BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar:
                    return "    implementation 'com.bidscube:bidscube-sdk:" + Constants.NativeAndroidBidscubeSdkVersion + "@aar'\n";
                case BidscubeAndroidCoreDependencyMode.CustomGradleLines:
                {
                    var raw = CustomCoreImplementationGradleLines?.Trim();
                    if (string.IsNullOrEmpty(raw))
                    {
                        Debug.LogError(
                            "[BidscubeSDK] CoreDependencyMode=CustomGradleLines but CustomCoreImplementationGradleLines is empty; " +
                            "falling back to Maven @aar for this export.");
                        return "    implementation 'com.bidscube:bidscube-sdk:" + Constants.NativeAndroidBidscubeSdkVersion + "@aar'\n";
                    }

                    return NormalizeCustomCoreGradleLinesIndent(raw);
                }
                case BidscubeAndroidCoreDependencyMode.SkipInjectionIntegratorOwnsCore:
                    return "    // Core bidscube-sdk: supplied by integrator (BidscubeAndroidGradlePostprocessor.CoreDependencyMode=SkipInjectionIntegratorOwnsCore)\n";
                default:
                    return "    implementation 'com.bidscube:bidscube-sdk:" + Constants.NativeAndroidBidscubeSdkVersion + "@aar'\n";
            }
        }

        private static string NormalizeCustomCoreGradleLinesIndent(string raw)
        {
            var sb = new StringBuilder();
            using (var reader = new StringReader(raw))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var t = line.TrimEnd();
                    if (string.IsNullOrWhiteSpace(t))
                        continue;
                    if (t.StartsWith("    ", StringComparison.Ordinal))
                        sb.AppendLine(t);
                    else
                        sb.Append("    ").AppendLine(t.TrimStart());
                }
            }

            var s = sb.ToString();
            if (string.IsNullOrEmpty(s))
            {
                Debug.LogError("[BidscubeSDK] CustomCoreImplementationGradleLines produced no non-empty lines; falling back to Maven @aar.");
                return "    implementation 'com.bidscube:bidscube-sdk:" + Constants.NativeAndroidBidscubeSdkVersion + "@aar'\n";
            }

            return s.EndsWith("\n", StringComparison.Ordinal) ? s : s + "\n";
        }

        private static bool GradleDeclaresBidscubeCoreSdk(string text)
        {
            if (Regex.IsMatch(text, @"implementation\s+['""]com\.bidscube:bidscube-sdk:", RegexOptions.Multiline))
                return true;
            if (Regex.IsMatch(text, @"implementation\s+files\s*\([^)]*bidscube[^)]*\)", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                return true;
            if (Regex.IsMatch(text, @"implementation\s+fileTree\s*\([^)]*bidscube", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                return true;
            if (Regex.IsMatch(text, @"implementation\s+project\s*\([^)]*bidscube", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                return true;
            return Regex.IsMatch(text, @"implementation\s+name\s*:\s*['""][^'""]*bidscube", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        private static void ValidateCoreBidscubeSdkDependency(string unityLibGradlePath)
        {
            try
            {
                if (string.IsNullOrEmpty(unityLibGradlePath) || !File.Exists(unityLibGradlePath))
                    return;

                var text = File.ReadAllText(unityLibGradlePath);
                if (!text.Contains(Marker))
                    return;

                switch (CoreDependencyMode)
                {
                    case BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar:
                    {
                        var ver = Regex.Escape(Constants.NativeAndroidBidscubeSdkVersion);
                        if (Regex.IsMatch(
                                text,
                                @"implementation\s+['""]com\.bidscube:bidscube-sdk:" + ver + @"@aar['""]",
                                RegexOptions.Multiline))
                            return;

                        Debug.LogError(
                            "Bidscube core SDK dependency was injected without @aar; Android Java classes may be missing at runtime.");
                        return;
                    }
                    case BidscubeAndroidCoreDependencyMode.CustomGradleLines:
                    {
                        var needle = CustomCoreImplementationGradleLines?.Trim();
                        if (string.IsNullOrEmpty(needle))
                            return;
                        var normalized = NormalizeCustomCoreGradleLinesIndent(needle).TrimEnd();
                        if (text.IndexOf(normalized, StringComparison.Ordinal) >= 0)
                            return;

                        Debug.LogWarning(
                            "[BidscubeSDK] CustomGradleLines: expected CustomCoreImplementationGradleLines (normalized) not found in unityLibrary/build.gradle after export — verify the block was inserted.");
                        return;
                    }
                    case BidscubeAndroidCoreDependencyMode.SkipInjectionIntegratorOwnsCore:
                    {
                        if (GradleDeclaresBidscubeCoreSdk(text))
                            return;

                        Debug.LogWarning(
                            "[BidscubeSDK] SkipInjectionIntegratorOwnsCore: no implementation line referencing bidscube core SDK was detected in unityLibrary/build.gradle. " +
                            "Add one core source (Maven @aar, files('libs/….aar'), or project module) to avoid ClassNotFoundException for com.bidscube.sdk.BidscubeSDK.");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] ValidateCoreBidscubeSdkDependency: {e.Message}");
            }
        }

        /// <summary>
        /// Ensures the core Bidscube Android SDK appears after <see cref="Marker"/> per <see cref="CoreDependencyMode"/>.
        /// </summary>
        private static void EnsureCoreBidscubeSdkAfterMarker(string unityLibGradlePath)
        {
            try
            {
                if (string.IsNullOrEmpty(unityLibGradlePath) || !File.Exists(unityLibGradlePath))
                    return;

                var text = File.ReadAllText(unityLibGradlePath);
                if (CoreDependencyMode == BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar)
                    text = NormalizeBidscubeCoreSdkImplementationToAar(text);

                if (CoreDependencyMode == BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar)
                {
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
                    return;
                }

                if (CoreDependencyMode == BidscubeAndroidCoreDependencyMode.SkipInjectionIntegratorOwnsCore)
                {
                    File.WriteAllText(unityLibGradlePath, text);
                    return;
                }

                // CustomGradleLines
                var customRaw = CustomCoreImplementationGradleLines?.Trim();
                if (string.IsNullOrEmpty(customRaw))
                {
                    Debug.LogError("[BidscubeSDK] CustomGradleLines with empty CustomCoreImplementationGradleLines — skipping core insert.");
                    File.WriteAllText(unityLibGradlePath, text);
                    return;
                }

                var normalizedCustom = NormalizeCustomCoreGradleLinesIndent(customRaw).TrimEnd();
                if (text.IndexOf(normalizedCustom, StringComparison.Ordinal) >= 0)
                {
                    File.WriteAllText(unityLibGradlePath, text);
                    return;
                }

                var idx = text.IndexOf(Marker, StringComparison.Ordinal);
                if (idx < 0)
                {
                    File.WriteAllText(unityLibGradlePath, text);
                    return;
                }

                var block = "\n" + normalizedCustom + "\n";
                text = text.Insert(idx + Marker.Length, block);
                File.WriteAllText(unityLibGradlePath, text);
                Debug.Log("[BidscubeSDK] Injected CustomCoreImplementationGradleLines after " + Marker + ".");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BidscubeSDK] EnsureCoreBidscubeSdkAfterMarker: {e.Message}");
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
