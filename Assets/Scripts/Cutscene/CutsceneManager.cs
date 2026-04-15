using System;
using System.Collections;
using UnityEngine;
using TCG.Core;
using TCG.UI.Cutscene;

namespace TCG.Cutscene
{
    /// <summary>
    /// Singleton that drives cutscene playback.
    ///
    /// Attach to a persistent GameObject alongside a <see cref="CutsceneUI"/> component
    /// (or assign <see cref="cutsceneUI"/> in the Inspector).
    ///
    /// Typical usage:
    /// <code>
    ///   CutsceneManager.Instance.Play(myCutsceneData, onComplete: () => StartMatch());
    /// </code>
    ///
    /// Beat execution rules:
    /// <list type="bullet">
    ///   <item><b>Dialogue</b> — waits until <see cref="Advance"/> is called. First tap completes
    ///       the typewriter; second tap advances to the next beat.</item>
    ///   <item><b>Image</b> — shows the sprite; waits for tap or auto-advances after
    ///       <c>beat.autoAdvanceDelay</c> seconds.</item>
    ///   <item><b>Video</b> — plays the clip; auto-advances when it ends (or on Skip).</item>
    ///   <item><b>Wait / FadeIn / FadeOut</b> — auto-advances after <c>beat.duration</c> seconds.</item>
    /// </list>
    /// </summary>
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }

        [SerializeField] private CutsceneUI cutsceneUI;

        // ── Runtime state ──────────────────────────────────────────────────────────
        private CutsceneData _current;
        private Action       _onComplete;
        private bool         _playing;
        private bool         _advance;     // set by Advance() when typewriter is done
        private bool         _videoComplete; // set by VideoBeatUI callback

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // CutsceneUI starts hidden
            if (cutsceneUI != null) cutsceneUI.gameObject.SetActive(false);
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Returns true while a cutscene is in progress.</summary>
        public bool IsPlaying => _playing;

        /// <summary>
        /// Starts playing <paramref name="data"/>. The optional <paramref name="onComplete"/>
        /// callback fires when all beats finish (or when the player skips).
        /// If a cutscene is already playing it will be interrupted.
        /// </summary>
        public void Play(CutsceneData data, Action onComplete = null)
        {
            if (data == null || data.beats == null || data.beats.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            if (_playing) Interrupt();

            _current    = data;
            _onComplete = onComplete;
            _playing    = true;
            _advance    = false;
            _videoComplete = false;

            if (cutsceneUI != null) cutsceneUI.gameObject.SetActive(true);

            GameEvents.RaiseCutsceneStarted(data);
            StartCoroutine(PlaySequence());
        }

        /// <summary>
        /// Called by the tap / continue button in the UI.
        /// If the typewriter is still running, it completes instantly.
        /// Otherwise it signals the current beat to advance.
        /// </summary>
        public void Advance()
        {
            if (!_playing) return;

            if (cutsceneUI != null && cutsceneUI.IsTypewriterRunning)
                cutsceneUI.CompleteTypewriterInstantly();
            else
                _advance = true;
        }

        /// <summary>
        /// Jumps to the end of the cutscene immediately.
        /// Only works when <see cref="CutsceneData.skippable"/> is true.
        /// </summary>
        public void Skip()
        {
            if (!_playing || _current == null || !_current.skippable) return;
            Interrupt();
            Finish();
        }

        // ── Internal helpers ───────────────────────────────────────────────────────

        /// <summary>Stops the coroutine without calling onComplete (used before restarting).</summary>
        private void Interrupt()
        {
            StopAllCoroutines();
            _playing = false;
            if (cutsceneUI != null) cutsceneUI.gameObject.SetActive(false);
        }

        private void Finish()
        {
            _playing = false;
            if (cutsceneUI != null) cutsceneUI.gameObject.SetActive(false);

            var data     = _current;
            var callback = _onComplete;

            _current    = null;
            _onComplete = null;

            GameEvents.RaiseCutsceneEnded(data);
            callback?.Invoke();
        }

        // ── Coroutines ─────────────────────────────────────────────────────────────

        private IEnumerator PlaySequence()
        {
            foreach (var beat in _current.beats)
            {
                GameEvents.RaiseCutsceneBeatStarted(beat);
                yield return ExecuteBeat(beat);
            }
            Finish();
        }

        private IEnumerator ExecuteBeat(CutsceneBeat beat)
        {
            // Optional one-shot SFX
            if (beat.sfx != null)
                AudioSource.PlayClipAtPoint(beat.sfx, Vector3.zero);

            switch (beat.type)
            {
                case CutsceneBeatType.Dialogue:
                    _advance = false;
                    cutsceneUI?.ShowDialogue(beat);
                    yield return new WaitUntil(() => _advance);
                    break;

                case CutsceneBeatType.Image:
                    _advance = false;
                    cutsceneUI?.ShowImage(beat);
                    if (beat.waitForTap)
                        yield return new WaitUntil(() => _advance);
                    else
                        yield return new WaitForSeconds(beat.autoAdvanceDelay);
                    break;

                case CutsceneBeatType.Video:
                    _videoComplete = false;
                    cutsceneUI?.ShowVideo(beat, () => _videoComplete = true);
                    yield return new WaitUntil(() => _videoComplete);
                    break;

                case CutsceneBeatType.Wait:
                    cutsceneUI?.ShowWait();
                    yield return new WaitForSeconds(beat.duration);
                    break;

                case CutsceneBeatType.FadeIn:
                    if (cutsceneUI != null) yield return cutsceneUI.FadeIn(beat.duration);
                    else yield return new WaitForSeconds(beat.duration);
                    break;

                case CutsceneBeatType.FadeOut:
                    if (cutsceneUI != null) yield return cutsceneUI.FadeOut(beat.duration);
                    else yield return new WaitForSeconds(beat.duration);
                    break;
            }
        }
    }
}
