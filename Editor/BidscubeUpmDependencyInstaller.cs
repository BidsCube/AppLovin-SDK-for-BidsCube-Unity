#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace BidscubeSDK.Editor
{
    /// <summary>
    /// Ensures Unity UI and TextMeshPro are installed, matching <see cref="BidscubeUpmDependencyInstaller.UguiPackageId"/> /
    /// <see cref="BidscubeUpmDependencyInstaller.TextMeshProPackageId"/> in <c>package.json</c> (same behavior as
    /// <a href="https://github.com/BidsCube/bidscube-sdk-unity">bidscube-sdk-unity</a> README: no manual TMP/UGUI setup for typical UPM flows).
    /// Runs once per Editor session after the first domain reload.
    /// </summary>
    [InitializeOnLoad]
    internal static class BidscubeUpmDependencyInstaller
    {
        internal const string UguiPackageId = "com.unity.ugui@2.0.0";
        internal const string TextMeshProPackageId = "com.unity.textmeshpro@3.0.6";

        private const string SessionKey = "com.bidscube.sdk.UpmDependencyInstallerRan";

        private static ListRequest _listRequest;

        static BidscubeUpmDependencyInstaller()
        {
            EditorApplication.delayCall += TrySchedule;
        }

        private static void TrySchedule()
        {
            if (Application.isBatchMode)
                return;

            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);

            _listRequest = Client.List(true);
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (_listRequest == null || !_listRequest.IsCompleted)
                return;

            EditorApplication.update -= OnEditorUpdate;

            if (_listRequest.Status != StatusCode.Success)
            {
                Debug.LogWarning("[BidscubeSDK] Package Manager list failed; UGUI/TextMeshPro were not auto-verified. " +
                                 "Ensure manifest.json includes dependencies from package.json.");
                return;
            }

            var haveUgui = false;
            var haveTmp = false;
            foreach (var info in _listRequest.Result)
            {
                if (info.name == "com.unity.ugui")
                    haveUgui = true;
                if (info.name == "com.unity.textmeshpro")
                    haveTmp = true;
            }

            if (!haveUgui)
            {
                Debug.Log("[BidscubeSDK] Installing required dependency: " + UguiPackageId);
                Client.Add(UguiPackageId);
            }

            if (!haveTmp)
            {
                Debug.Log("[BidscubeSDK] Installing required dependency: " + TextMeshProPackageId);
                Client.Add(TextMeshProPackageId);
            }
        }
    }
}
#endif
