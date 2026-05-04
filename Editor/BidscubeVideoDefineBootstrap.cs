using BidscubeSDK.Android;
using UnityEditor;

namespace BidscubeSDK.Editor
{
    [InitializeOnLoad]
    internal static class BidscubeVideoDefineBootstrap
    {
        static BidscubeVideoDefineBootstrap()
        {
            if (BidscubeAndroidFeatureSetStore.HasInitializedDefaults())
                return;
            BidscubeAndroidFeatureSetStore.MarkInitializedDefaults();
            BidscubeAndroidFeatureSetStore.Save(BidscubeAndroidFeatureSet.FullWithVideo);
            BidscubeDefineApplicator.Apply(BidscubeAndroidFeatureSet.FullWithVideo);
        }
    }
}
