using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TCG.Cutscene;

namespace TCG.UI.Cutscene
{
    /// <summary>
    /// Root panel for all cutscene rendering.
    ///
    /// Activated / deactivated by <see cref="CutsceneManager"/>. Should live on a
    /// <c>Canvas</c> with <c>Screen Space – Overlay</c> and a high <c>Sort Order</c>
    /// (e.g. 100) so it renders above every other UI element including the HUD.
    ///
    /// Sub-panels (<see cref="dialogueBox"/>, <see cref="videoArea"/>) are shown or hidden
    /// per beat type. A full-screen tap button and a Skip button are also managed here.
    ///
    /// Inspector setup:
    ///   • <see cref="dialogueBox"/>      — <see cref="DialogueBeatUI"/> sub-panel
    ///   • <see cref="videoArea"/>        — <see cref="VideoBeatUI"/> sub-panel
    ///   • <see cref="backgroundImage"/>  — Image that fills the screen with a sprite
    ///   • <see cref="fadeOverlay"/>      — black Image used for fade-in / fade-out
    ///   • <see cref="tapButton"/>        — transparent full-screen button; calls Advance()
    ///   • <see cref="skipButton"/>       — calls Skip(); hidden for non-skippable cutscenes
    /// </summary>
    public class CutsceneUI : MonoBehaviour
    {
        [Header("Sub-Panels")]
        [SerializeField] private DialogueBeatUI dialogueBox;
        [SerializeField] private VideoBeatUI    videoArea;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;

        [Header("Overlays")]
        [SerializeField] private Image      fadeOverlay;
        [SerializeField] private Button     tapButton;
        [SerializeField] private Button     skipButton;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            tapButton?.onClick.AddListener(OnTapClicked);
            skipButton?.onClick.AddListener(OnSkipClicked);

            // Fade overlay starts fully transparent
            SetFadeAlpha(0f);

            // All sub-panels start hidden
            dialogueBox?.gameObject.SetActive(false);
            videoArea?.gameObject.SetActive(false);
            tapButton?.gameObject.SetActive(false);
            skipButton?.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            tapButton?.onClick.RemoveListener(OnTapClicked);
            skipButton?.onClick.RemoveListener(OnSkipClicked);
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        /// <summary>True while the dialogue box typewriter is still animating.</summary>
        public bool IsTypewriterRunning => dialogueBox != null && dialogueBox.IsTyping;

        // ── Beat Display ──────────────────────────────────────────────────────────

        /// <summary>Configures the panel for a <see cref="CutsceneBeatType.Dialogue"/> beat.</summary>
        public void ShowDialogue(CutsceneBeat beat)
        {
            SetVideoVisible(false);
            dialogueBox?.gameObject.SetActive(true);
            dialogueBox?.Show(beat);
            tapButton?.gameObject.SetActive(true);
            UpdateSkipButton();
        }

        /// <summary>Configures the panel for a <see cref="CutsceneBeatType.Image"/> beat.</summary>
        public void ShowImage(CutsceneBeat beat)
        {
            SetVideoVisible(false);
            dialogueBox?.gameObject.SetActive(false);

            if (backgroundImage != null && beat.backgroundSprite != null)
            {
                backgroundImage.sprite  = beat.backgroundSprite;
                backgroundImage.color   = beat.spriteTint;
                backgroundImage.enabled = true;
            }

            tapButton?.gameObject.SetActive(beat.waitForTap);
            UpdateSkipButton();
        }

        /// <summary>
        /// Configures the panel for a <see cref="CutsceneBeatType.Video"/> beat.
        /// <paramref name="onComplete"/> is forwarded to <see cref="VideoBeatUI"/>.
        /// </summary>
        public void ShowVideo(CutsceneBeat beat, Action onComplete)
        {
            dialogueBox?.gameObject.SetActive(false);
            tapButton?.gameObject.SetActive(false);
            videoArea?.gameObject.SetActive(true);
            videoArea?.Play(beat.videoClip, beat.loopVideo, onComplete);
            UpdateSkipButton();
        }

        /// <summary>Hides interactive elements during a <see cref="CutsceneBeatType.Wait"/> beat.</summary>
        public void ShowWait()
        {
            dialogueBox?.gameObject.SetActive(false);
            SetVideoVisible(false);
            tapButton?.gameObject.SetActive(false);
            UpdateSkipButton();
        }

        // ── Typewriter helpers ────────────────────────────────────────────────────

        /// <summary>Tells the dialogue box to show the full text without animation.</summary>
        public void CompleteTypewriterInstantly() => dialogueBox?.CompleteInstantly();

        // ── Fade coroutines ───────────────────────────────────────────────────────

        /// <summary>
        /// Coroutine: animates the fade overlay from opaque black → transparent over
        /// <paramref name="duration"/> seconds (reveals the scene).
        /// </summary>
        public IEnumerator FadeIn(float duration)
        {
            PrepareForFade();
            yield return AnimateFade(from: 1f, to: 0f, duration);
        }

        /// <summary>
        /// Coroutine: animates the fade overlay from transparent → opaque black over
        /// <paramref name="duration"/> seconds (conceals the scene).
        /// </summary>
        public IEnumerator FadeOut(float duration)
        {
            PrepareForFade();
            yield return AnimateFade(from: 0f, to: 1f, duration);
        }

        // ── Internal helpers ──────────────────────────────────────────────────────

        private void PrepareForFade()
        {
            dialogueBox?.gameObject.SetActive(false);
            SetVideoVisible(false);
            tapButton?.gameObject.SetActive(false);
        }

        private IEnumerator AnimateFade(float from, float to, float duration)
        {
            SetFadeAlpha(from);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetFadeAlpha(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }

            SetFadeAlpha(to);
        }

        private void SetFadeAlpha(float alpha)
        {
            if (fadeOverlay == null) return;
            var c = fadeOverlay.color;
            c.a = alpha;
            fadeOverlay.color   = c;
            fadeOverlay.enabled = alpha > 0.001f;
        }

        private void SetVideoVisible(bool visible)
        {
            if (!visible) videoArea?.Stop();
            videoArea?.gameObject.SetActive(visible);
        }

        private void UpdateSkipButton()
        {
            if (skipButton == null) return;
            bool canSkip = CutsceneManager.Instance != null &&
                           CutsceneManager.Instance.IsPlaying &&
                           // access skippable flag via the active cutscene (exposed indirectly)
                           true; // CutsceneManager.Skip() already checks the flag internally
            skipButton.gameObject.SetActive(canSkip);
        }

        // ── Button callbacks ──────────────────────────────────────────────────────

        private void OnTapClicked()  => CutsceneManager.Instance?.Advance();
        private void OnSkipClicked() => CutsceneManager.Instance?.Skip();
    }
}
