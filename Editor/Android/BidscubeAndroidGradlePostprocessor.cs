using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BidscubeSDK.Android;
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
    internal sealed class BidscubeAndroidGradlePostprocessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 50;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var featureSet = BidscubeAndroidExportSettingsResolver.GetEffectiveFeatureSet();
            var coreMode = BidscubeAndroidExportSettingsResolver.GetEffectiveCoreDependencyMode();
            var customLines = BidscubeAndroidExportSettingsResolver.GetEffectiveCustomGradleLines();

            if (featureSet == BidscubeAndroidFeatureSet.LiteNoVideo)
                UnityEngine.Debug.Log("[Bidscube AppLovin] Android feature set: LiteNoVideo");
            else
                UnityEngine.Debug.Log("[Bidscube AppLovin] Android feature set: FullWithVideo");

            var pkgRoot = ResolvePackageRoot();
            if (string.IsNullOrEmpty(pkgRoot))
            {
                UnityEngine.Debug.LogWarning("[Bidscube AppLovin] Could not resolve UPM package root; skipping Gradle/AAR integration.");
                return;
            }

            var plugins = Path.Combine(pkgRoot, "Runtime", "Plugins", "Android");
            var ver = AdapterPackageInfo.NativeAndroidBidscubeSdkVersion;
            var liteName = AdapterPackageInfo.NativeAndroidBundledCoreAarLiteFileName;
            var fullName = AdapterPackageInfo.NativeAndroidBundledCoreAarFullFileName;
            var liteSrc = Path.Combine(plugins, liteName);
            var fullSrc = Path.Combine(plugins, fullName);
            var libsDir = Path.Combine(path, "unityLibrary", "libs");
            Directory.CreateDirectory(libsDir);
            var liteDst = Path.Combine(libsDir, liteName);
            var fullDst = Path.Combine(libsDir, fullName);

            if (coreMode == BidscubeAndroidCoreDependencyMode.SkipInjectionIntegratorOwnsCore)
            {
                UnityEngine.Debug.LogWarning("[Bidscube AppLovin] CoreDependencyMode SkipInjectionIntegratorOwnsCore — not injecting Bidscube core lines.");
                RemoveManagedBlock(path);
                return;
            }

            if (coreMode == BidscubeAndroidCoreDependencyMode.CustomGradleLines)
            {
                if (string.IsNullOrWhiteSpace(customLines))
                {
                    UnityEngine.Debug.LogError(
                        "[Bidscube AppLovin] CoreDependencyMode is CustomGradleLines but customCoreImplementationGradleLines is empty. Set lines on BidscubeAndroidExportSettings or use another coreDependencyMode.");
                    return;
                }

                TryCopySelectedAarForReference(featureSet, liteSrc, fullSrc, liteDst, fullDst);
                PatchUnityLibraryGradle(path, featureSet, coreMode, customLines, ver, fullCoreFromMaven: false,
                    useBundledFullAar: false, useBundledLiteAar: false);
                return;
            }

            if (featureSet == BidscubeAndroidFeatureSet.LiteNoVideo)
            {
                TryDelete(fullDst);
                if (!File.Exists(liteSrc))
                {
                    UnityEngine.Debug.LogError($"[Bidscube AppLovin] LiteNoVideo: missing lite AAR at {liteSrc}");
                    return;
                }

                File.Copy(liteSrc, liteDst, true);
                UnityEngine.Debug.Log($"[Bidscube AppLovin] Copied bundled core AAR: {liteDst}");
                PatchUnityLibraryGradle(path, featureSet, coreMode, "", ver, fullCoreFromMaven: false,
                    useBundledFullAar: false, useBundledLiteAar: true);
                return;
            }

            // FullWithVideo
            TryDelete(liteDst);
            if (coreMode == BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar)
            {
                TryDelete(fullDst);
                PatchUnityLibraryGradle(path, featureSet, coreMode, "", ver, fullCoreFromMaven: true,
                    useBundledFullAar: false, useBundledLiteAar: false);
                return;
            }

            if (!File.Exists(fullSrc))
            {
                UnityEngine.Debug.LogError(
                    "[Bidscube AppLovin] FullWithVideo requires Runtime/Plugins/Android/bidscube-sdk-" + ver +
                    ".aar, or set coreDependencyMode to MavenBidscubeSdkAar with a reachable Maven artifact com.bidscube:bidscube-sdk:" +
                    ver + "@aar. Switch to LiteNoVideo for publisher demo / CI without the full AAR.");
                RemoveManagedBlock(path);
                return;
            }

            File.Copy(fullSrc, fullDst, true);
            UnityEngine.Debug.Log($"[Bidscube AppLovin] Copied bundled core AAR: {fullDst}");
            PatchUnityLibraryGradle(path, featureSet, coreMode, "", ver, fullCoreFromMaven: false,
                useBundledFullAar: true, useBundledLiteAar: false);
        }

        static void TryCopySelectedAarForReference(BidscubeAndroidFeatureSet fs, string liteSrc, string fullSrc,
            string liteDst, string fullDst)
        {
            try
            {
                if (fs == BidscubeAndroidFeatureSet.LiteNoVideo && File.Exists(liteSrc))
                    File.Copy(liteSrc, liteDst, true);
                else if (fs == BidscubeAndroidFeatureSet.FullWithVideo && File.Exists(fullSrc))
                    File.Copy(fullSrc, fullDst, true);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[Bidscube AppLovin] Optional AAR copy: {e.Message}");
            }
        }

        static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // ignored
            }
        }

        static void RemoveManagedBlock(string gradleProjectRoot)
        {
            var gradlePath = Path.Combine(gradleProjectRoot, "unityLibrary", "build.gradle");
            if (!File.Exists(gradlePath))
                return;
            var content = File.ReadAllText(gradlePath);
            const string start = "// __BIDSCUBE_ANDROID_MANAGED_START__";
            const string end = "// __BIDSCUBE_ANDROID_MANAGED_END__";
            if (!content.Contains(start))
                return;
            var pattern = new Regex(Regex.Escape(start) + "[\\s\\S]*?" + Regex.Escape(end), RegexOptions.Multiline);
            content = pattern.Replace(content, "", 1);
            File.WriteAllText(gradlePath, content);
        }

        static void PatchUnityLibraryGradle(string gradleProjectRoot, BidscubeAndroidFeatureSet featureSet,
            BidscubeAndroidCoreDependencyMode coreMode, string customLines, string ver, bool fullCoreFromMaven,
            bool useBundledFullAar, bool useBundledLiteAar)
        {
            var gradlePath = Path.Combine(gradleProjectRoot, "unityLibrary", "build.gradle");
            if (!File.Exists(gradlePath))
            {
                UnityEngine.Debug.LogWarning($"[Bidscube AppLovin] unityLibrary/build.gradle not found at {gradlePath}");
                return;
            }

            var content = File.ReadAllText(gradlePath);
            var sb = new StringBuilder();
            sb.AppendLine("// __BIDSCUBE_ANDROID_MANAGED_START__");

            if (coreMode == BidscubeAndroidCoreDependencyMode.CustomGradleLines &&
                !string.IsNullOrWhiteSpace(customLines))
            {
                foreach (var line in customLines.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    sb.AppendLine("    " + line.Trim());
            }
            else if (useBundledLiteAar)
            {
                sb.AppendLine($"    implementation files('libs/{AdapterPackageInfo.NativeAndroidBundledCoreAarLiteFileName}')");
            }
            else if (fullCoreFromMaven)
            {
                sb.AppendLine($"    implementation 'com.bidscube:bidscube-sdk:{ver}@aar'");
            }
            else if (useBundledFullAar)
            {
                sb.AppendLine($"    implementation files('libs/{AdapterPackageInfo.NativeAndroidBundledCoreAarFullFileName}')");
            }

            MaybeAppendAppLovinSdkLine(sb, content);

            if (featureSet == BidscubeAndroidFeatureSet.LiteNoVideo)
                UnityEngine.Debug.Log("[Bidscube AppLovin] Skipping Media3 and Google IMA dependencies");
            else
            {
                UnityEngine.Debug.Log("[Bidscube AppLovin] Including Media3 and Google IMA dependencies");
                AppendVideoDeps(sb, content);
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

        static void MaybeAppendAppLovinSdkLine(StringBuilder sb, string existingGradle)
        {
            if (existingGradle.IndexOf("com.applovin:applovin-sdk", StringComparison.OrdinalIgnoreCase) >= 0)
                return;
            if (sb.ToString().IndexOf("com.applovin:applovin-sdk", StringComparison.OrdinalIgnoreCase) >= 0)
                return;
            sb.AppendLine("    implementation 'com.applovin:applovin-sdk:13.+'");
        }

        static void AppendVideoDeps(StringBuilder sb, string existingGradle)
        {
            void AddIfMissing(string coordinate)
            {
                var parts = coordinate.Split(':');
                var artifact = parts.Length > 1 ? parts[1] : coordinate;
                if (existingGradle.IndexOf(artifact, StringComparison.OrdinalIgnoreCase) >= 0)
                    return;
                var cur = sb.ToString();
                if (cur.IndexOf(artifact, StringComparison.OrdinalIgnoreCase) >= 0)
                    return;
                sb.AppendLine($"    implementation '{coordinate}'");
            }

            AddIfMissing("androidx.media3:media3-common:1.4.1");
            AddIfMissing("androidx.media3:media3-ui:1.4.1");
            AddIfMissing("com.google.ads.interactivemedia.v3:interactivemedia:3.33.0");
        }

        static string InjectAfterDependenciesOpen(string gradle, string block)
        {
            var idx = gradle.IndexOf("dependencies", StringComparison.Ordinal);
            if (idx < 0)
                return gradle + "\n" + block + "\n";
            var brace = gradle.IndexOf('{', idx);
            if (brace < 0)
                return gradle + "\n" + block + "\n";
            return gradle.Insert(brace + 1, "\n" + block);
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
