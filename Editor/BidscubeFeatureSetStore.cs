using BidscubeSDK;
using UnityEditor;

namespace BidscubeSDK.Editor
{
    internal static class BidscubeFeatureSetStore
    {
        internal const string PrefKey = "Bidscube.Android.BidscubeFeatureSet";
        const string InitKey = "Bidscube.Android.BidscubeFeatureSetInitialized";

        public static BidscubeFeatureSet Load()
        {
            return (BidscubeFeatureSet)EditorPrefs.GetInt(PrefKey, (int)BidscubeFeatureSet.FullWithVideo);
        }

        public static void Save(BidscubeFeatureSet value)
        {
            EditorPrefs.SetInt(PrefKey, (int)value);
        }

        public static bool HasInitializedDefaults()
        {
            return EditorPrefs.GetBool(InitKey, false);
        }

        public static void MarkInitializedDefaults()
        {
            EditorPrefs.SetBool(InitKey, true);
        }
    }
}
