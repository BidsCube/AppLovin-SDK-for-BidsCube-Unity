using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BidscubeSDK.Editor.Bootstrap
{
    /// <summary>
    /// com.bidscube.sdk 1.2.16 and 1.2.17 shipped
    /// BidscubeAndroidSdkInterop.cs.meta and BidscubeIosSdkInterop.cs.meta with 31-character GUIDs.
    /// Unity then drops those scripts and the Android player fails with CS0103.
    /// This assembly does not reference BidscubeSDK, so it still compiles when that package is broken.
    /// </summary>
    [InitializeOnLoad]
    static class BidscubeSdkMetaGuidRepair
    {
        const string SessionKey = "BidscubeSdkMetaGuidRepair.applied";

        static readonly Regex GuidLine = new Regex(
            @"^guid:\s*([0-9a-fA-F]+)\s*$",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        static BidscubeSdkMetaGuidRepair()
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            var changed = false;
            var sawPackage = false;
            try
            {
                changed = RepairInstalledSdkPackages(out sawPackage);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Bidscube AppLovin] Could not repair com.bidscube.sdk .meta GUIDs: " + e.Message);
            }

            if (!sawPackage)
                return;

            SessionState.SetBool(SessionKey, true);
            if (changed)
                AssetDatabase.Refresh();
        }

        static bool RepairInstalledSdkPackages(out bool sawPackage)
        {
            var changed = false;
            sawPackage = false;
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
                return false;

            changed |= RepairTree(Path.Combine(projectRoot, "Packages", "com.bidscube.sdk"), ref sawPackage);
            changed |= RepairTree(Path.Combine(projectRoot, "Packages", "bidscube-sdk-unity"), ref sawPackage);

            var cache = Path.Combine(projectRoot, "Library", "PackageCache");
            if (!Directory.Exists(cache))
                return changed;

            foreach (var dir in Directory.GetDirectories(cache))
            {
                var name = Path.GetFileName(dir);
                if (name.StartsWith("com.bidscube.sdk@", StringComparison.Ordinal))
                    changed |= RepairTree(dir, ref sawPackage);
            }

            return changed;
        }

        static bool RepairTree(string root, ref bool sawPackage)
        {
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
                return false;

            var packageJson = Path.Combine(root, "package.json");
            if (!File.Exists(packageJson))
                return false;

            sawPackage = true;

            var changed = false;
            foreach (var meta in Directory.GetFiles(root, "*.meta", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(meta);
                if (fileName != "BidscubeAndroidSdkInterop.cs.meta"
                    && fileName != "BidscubeIosSdkInterop.cs.meta")
                    continue;
                if (RepairMeta(meta))
                    changed = true;
            }

            if (changed)
                Debug.Log("[Bidscube AppLovin] Repaired 31-character Unity GUIDs under " + root);
            return changed;
        }

        static bool RepairMeta(string path)
        {
            var text = File.ReadAllText(path);
            var match = GuidLine.Match(text);
            if (!match.Success)
                return false;

            var guid = match.Groups[1].Value;
            if (guid.Length == 32)
                return false;
            if (guid.Length != 31 || !IsHex(guid))
                return false;

            var repaired = guid + "0";
            var updated = text.Substring(0, match.Groups[1].Index)
                + repaired
                + text.Substring(match.Groups[1].Index + guid.Length);

            var info = new FileInfo(path);
            var wasReadOnly = info.IsReadOnly;
            if (wasReadOnly)
                info.IsReadOnly = false;
            File.WriteAllText(path, updated);
            if (wasReadOnly)
                info.IsReadOnly = true;
            return true;
        }

        static bool IsHex(string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                var hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!hex)
                    return false;
            }

            return true;
        }
    }
}
