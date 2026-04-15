using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Narrative;

namespace TCG.UI.Narrative
{
    /// <summary>
    /// One entry row in the Story Log panel.
    ///
    /// Inspector setup:
    ///   • <see cref="illustration"/>   — Image showing the event's art (hidden when null)
    ///   • <see cref="titleText"/>      — Large heading (logTitle)
    ///   • <see cref="bodyText"/>       — Scrollable body (logBody)
    ///   • <see cref="replayButton"/>   — Hidden when the event has no cutscene; calls ReplayCutscene
    /// </summary>
    public class StoryLogEntryUI : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private Image    illustration;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;

        [Header("Actions")]
        [SerializeField] private Button   replayButton;

        // ── Runtime ────────────────────────────────────────────────────────────────
        private string _eventId;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            replayButton?.onClick.AddListener(OnReplayClicked);
        }

        private void OnDestroy()
        {
            replayButton?.onClick.RemoveListener(OnReplayClicked);
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Populates the entry from <paramref name="ev"/>.</summary>
        public void Bind(NarrativeEventData ev)
        {
            _eventId = ev.eventId;

            if (titleText != null) titleText.text = ev.logTitle;
            if (bodyText  != null) bodyText.text  = ev.logBody;

            bool hasArt = ev.logIllustration != null;
            if (illustration != null)
            {
                illustration.gameObject.SetActive(hasArt);
                if (hasArt) illustration.sprite = ev.logIllustration;
            }

            if (replayButton != null)
                replayButton.gameObject.SetActive(ev.cutscene != null);
        }

        // ── Handlers ──────────────────────────────────────────────────────────────

        private void OnReplayClicked() => NarrativeManager.Instance?.ReplayCutscene(_eventId);
    }
}
