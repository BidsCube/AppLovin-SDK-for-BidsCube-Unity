using System;
using UnityEngine.UI;

namespace BidscubeSDK
{
    /// <summary>
    /// Plays linear video for VAST / direct-URL paths when IMA is not used.
    /// Assign <see cref="VideoAdView.VideoPlaybackFactory"/> to plug AVPro, native texture, etc.
    /// Omit Unity <see cref="UnityEngine.Video.VideoPlayer"/> from the build by defining <c>BIDSCUBE_DISABLE_UNITY_VIDEO</c> and supplying a factory.
    /// </summary>
    public interface IVideoSurfacePlayback
    {
        string SourceUrl { get; set; }
        bool IsPrepared { get; }
        bool IsPlaying { get; }
        long Frame { get; }
        long FrameCount { get; }
        void BindToRawImage(RawImage videoTexture);
        void Prepare();
        void Play();
        void Pause();
        void Stop();
        event Action Prepared;
        event Action Started;
        event Action Completed;
    }
}
