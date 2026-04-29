namespace BidscubeSDK
{
    /// <summary>
    /// Android/native SDK capacity: full stack (video, IMA/Media3) vs lite (no video player stack).
    /// Default is <see cref="FullWithVideo"/> — configure in Unity via <b>Tools → Bidscube SDK → Android Build Features</b>.
    /// </summary>
    public enum BidscubeFeatureSet
    {
        FullWithVideo = 0,
        LiteNoVideo = 1
    }
}
