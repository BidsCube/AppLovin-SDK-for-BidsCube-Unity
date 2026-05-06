using BidscubeSDK.Mediation;
using UnityEditor;
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
            BidscubeAndroidGradleProjectPatcher.OnPostGenerateGradleAndroidProject(
                path,
                logPrefix: "[Bidscube AppLovin]",
                packageAssembly: typeof(AdapterPackageInfo).Assembly,
                appendAppLovinSdkDependency: true,
                new BidscubeAndroidBundledCoreAarNames(
                    AdapterPackageInfo.NativeAndroidBidscubeSdkVersion,
                    AdapterPackageInfo.NativeAndroidBundledCoreAarLiteFileName,
                    AdapterPackageInfo.NativeAndroidBundledCoreAarFullFileName));
        }
    }
#endif
}
