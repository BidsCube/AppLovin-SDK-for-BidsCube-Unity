#if !BIDSCUBE_DISABLE_UNITY_VIDEO
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace BidscubeSDK
{
    /// <summary>Default VAST linear playback using Unity <see cref="VideoPlayer"/> (pulls Video module into the player).</summary>
    [DisallowMultipleComponent]
    public sealed class UnityEngineVideoSurfacePlayback : MonoBehaviour, IVideoSurfacePlayback
    {
        private VideoPlayer _player;
        private bool _subscribed;

        public string SourceUrl
        {
            get => _player != null ? _player.url : string.Empty;
            set
            {
                EnsurePlayer();
                _player.url = value ?? string.Empty;
            }
        }

        public bool IsPrepared => _player != null && _player.isPrepared;
        public bool IsPlaying => _player != null && _player.isPlaying;
        public long Frame => _player != null ? _player.frame : 0L;
        // Unity 6+: VideoPlayer.frameCount is ulong; interface stays long for VAST progress math.
        public long FrameCount => _player == null ? 0L : (long)_player.frameCount;

        public event Action Prepared;
        public event Action Started;
        public event Action Completed;

        public void BindToRawImage(RawImage videoTexture)
        {
            EnsurePlayer();
            if (videoTexture == null)
                return;

            var renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            _player.targetTexture = renderTexture;
            videoTexture.texture = renderTexture;
        }

        public void Prepare()
        {
            EnsurePlayer();
            _player.Prepare();
        }

        public void Play()
        {
            EnsurePlayer();
            _player.Play();
        }

        public void Pause()
        {
            if (_player != null)
                _player.Pause();
        }

        public void Stop()
        {
            if (_player != null)
                _player.Stop();
        }

        private void EnsurePlayer()
        {
            if (_player != null)
                return;

            _player = gameObject.GetComponent<VideoPlayer>();
            if (_player == null)
                _player = gameObject.AddComponent<VideoPlayer>();

            _player.playOnAwake = false;
            _player.isLooping = false;
            _player.renderMode = VideoRenderMode.RenderTexture;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_subscribed || _player == null)
                return;
            _subscribed = true;
            _player.prepareCompleted += OnPrepareCompleted;
            _player.started += OnStarted;
            _player.loopPointReached += OnLoopPointReached;
        }

        private void OnDestroy()
        {
            if (_player == null)
                return;
            _player.prepareCompleted -= OnPrepareCompleted;
            _player.started -= OnStarted;
            _player.loopPointReached -= OnLoopPointReached;
            _player.Stop();
            _player.url = string.Empty;
        }

        private void OnPrepareCompleted(VideoPlayer _)
        {
            Prepared?.Invoke();
        }

        private void OnStarted(VideoPlayer _)
        {
            Started?.Invoke();
        }

        private void OnLoopPointReached(VideoPlayer _)
        {
            Completed?.Invoke();
        }
    }
}
#endif
