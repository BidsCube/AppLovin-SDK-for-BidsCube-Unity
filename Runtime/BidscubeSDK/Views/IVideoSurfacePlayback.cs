using System;
using UnityEngine.UI;

namespace BidscubeSDK
{
    /// <summary>
    /// Direct SDK only: linear video for VAST / direct-URL paths on <c>VideoAdView</c> when not using IMA for that surface.
    /// Register via <see cref="SDKConfig.Builder.VideoPlaybackFactory"/> or <see cref="VideoAdView.VideoPlaybackFactory"/>; with <c>BIDSCUBE_DISABLE_UNITY_VIDEO</c>, a factory is required for that path. See <c>Documentation~/VIDEO_PLAYBACK.md</c>.
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
