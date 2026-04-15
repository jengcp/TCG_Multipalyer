using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace TCG.UI.Cutscene
{
    /// <summary>
    /// Video playback sub-panel for cutscenes.
    ///
    /// Uses Unity's <see cref="VideoPlayer"/> to decode a <see cref="VideoClip"/> and
    /// render each frame onto a runtime <see cref="RenderTexture"/> displayed by a
    /// <see cref="RawImage"/> in the UI canvas.
    ///
    /// Inspector setup:
    ///   • <see cref="videoPlayer"/>   — VideoPlayer component (must be on this or a child GameObject)
    ///   • <see cref="videoDisplay"/>  — RawImage that shows the rendered frames
    ///   • <see cref="skipButton"/>    — optional button that stops the clip and calls onComplete
    ///   • <see cref="renderWidth"/> / <see cref="renderHeight"/> — RenderTexture resolution
    ///
    /// The <see cref="RenderTexture"/> is created in <see cref="Awake"/> and released in
    /// <see cref="OnDestroy"/>, so no manual asset management is needed.
    /// </summary>
    public class VideoBeatUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage    videoDisplay;
        [SerializeField] private Button      skipButton;

        [Header("Render Texture")]
        [SerializeField] private int renderWidth  = 1920;
        [SerializeField] private int renderHeight = 1080;

        // ── Runtime ────────────────────────────────────────────────────────────────
        private RenderTexture _renderTexture;
        private Action        _onComplete;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _renderTexture = new RenderTexture(renderWidth, renderHeight, depth: 0);

            if (videoPlayer != null)
            {
                videoPlayer.targetTexture    = _renderTexture;
                videoPlayer.renderMode       = VideoRenderMode.RenderTexture;
                videoPlayer.loopPointReached += OnLoopPointReached;
            }

            if (videoDisplay != null)
                videoDisplay.texture = _renderTexture;

            skipButton?.onClick.AddListener(OnSkipClicked);
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
                videoPlayer.loopPointReached -= OnLoopPointReached;

            skipButton?.onClick.RemoveListener(OnSkipClicked);

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Starts playing <paramref name="clip"/>.
        /// <paramref name="onComplete"/> is invoked when the clip finishes or is skipped.
        /// </summary>
        public void Play(VideoClip clip, bool loop, Action onComplete)
        {
            if (videoPlayer == null || clip == null)
            {
                onComplete?.Invoke();
                return;
            }

            _onComplete           = onComplete;
            videoPlayer.clip      = clip;
            videoPlayer.isLooping = loop;

            skipButton?.gameObject.SetActive(true);
            videoPlayer.Play();
        }

        /// <summary>Stops playback without calling the completion callback.</summary>
        public void Stop()
        {
            videoPlayer?.Stop();
            _onComplete = null;
            skipButton?.gameObject.SetActive(false);
        }

        // ── Handlers ──────────────────────────────────────────────────────────────

        private void OnLoopPointReached(VideoPlayer _) => Complete();

        private void OnSkipClicked() => Complete();

        private void Complete()
        {
            videoPlayer?.Stop();
            skipButton?.gameObject.SetActive(false);

            var cb  = _onComplete;
            _onComplete = null;
            cb?.Invoke();
        }
    }
}
