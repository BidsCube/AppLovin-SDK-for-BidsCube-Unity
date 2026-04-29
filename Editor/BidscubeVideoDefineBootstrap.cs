using BidscubeSDK;
using UnityEditor;

namespace BidscubeSDK.Editor
{
    [InitializeOnLoad]
    internal static class BidscubeVideoDefineBootstrap
    {
        static BidscubeVideoDefineBootstrap()
        {
            if (BidscubeFeatureSetStore.HasInitializedDefaults())
                return;
            BidscubeFeatureSetStore.MarkInitializedDefaults();
            BidscubeFeatureSetStore.Save(BidscubeFeatureSet.FullWithVideo);
            BidscubeDefineApplicator.Apply(BidscubeFeatureSet.FullWithVideo);
        }
    }
}
