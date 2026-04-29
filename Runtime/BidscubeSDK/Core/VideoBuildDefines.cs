namespace BidscubeSDK
{
    /// <summary>Player scripting define managed by this package’s Editor scripts.</summary>
    public static class VideoBuildDefines
    {
        public const string EnableVideoSymbol = "BIDSCUBE_ENABLE_VIDEO";
    }

    /// <summary>Compile-time mirror of <see cref="VideoBuildDefines.EnableVideoSymbol"/>.</summary>
    public static class VideoBuildInfo
    {
#if BIDSCUBE_ENABLE_VIDEO
        public const bool IsVideoEnabled = true;
#else
        public const bool IsVideoEnabled = false;
#endif
    }
}
