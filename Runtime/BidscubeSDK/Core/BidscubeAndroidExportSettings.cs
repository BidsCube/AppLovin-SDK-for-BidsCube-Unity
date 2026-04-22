using UnityEngine;

namespace BidscubeSDK
{
    /// <summary>
    /// Optional project asset: commit to Git so CI and the whole team share the same Android export mode (lite vs full video).
    /// Create via <b>Assets → Create → Bidscube → Android Export Settings</b>. If no asset exists, the Editor post-processor code fallback <c>BidscubeAndroidGradlePostprocessor.FeatureSet</c> is used.
    /// </summary>
    [CreateAssetMenu(fileName = "BidscubeAndroidExportSettings", menuName = "Bidscube/Android Export Settings", order = 500)]
    public sealed class BidscubeAndroidExportSettings : ScriptableObject
    {
        [Tooltip("LiteNoVideo: smaller APK, no Media3/IMA Gradle lines; Direct SDK ShowVideoAd disabled on Android. FullWithVideo: full core AAR + IMA/Media3.")]
        public BidscubeAndroidFeatureSet featureSet = BidscubeAndroidFeatureSet.LiteNoVideo;
    }
}
