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
        }
    }
}
