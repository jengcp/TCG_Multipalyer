using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace TCG.UI.MainMenu
{
    /// <summary>
    /// A single selectable tile on the main menu grid.
    ///
    /// Inspector setup:
    ///   • <see cref="icon"/>              — large icon inside the card
    ///   • <see cref="titleText"/>         — primary label (e.g. "Campaign")
    ///   • <see cref="subtitleText"/>      — secondary label (e.g. "Story Mode")
    ///   • <see cref="button"/>            — the clickable surface (covers the whole card)
    ///   • <see cref="notificationBadge"/> — small badge shown when there is a count > 0
    ///   • <see cref="badgeCountText"/>    — number inside the badge; hidden when count == 1
    ///   • <see cref="lockedOverlay"/>     — optional greyed-out overlay for unavailable modes
    /// </summary>
    public class ModeCardUI : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Image    icon;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;

        [Header("Button")]
        [SerializeField] private Button button;

        [Header("Notification Badge")]
        [SerializeField] private GameObject notificationBadge;
        [SerializeField] private TMP_Text   badgeCountText;

        [Header("State")]
        [SerializeField] private GameObject lockedOverlay;

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Configures the card's display text, icon, and click callback.
        /// Previous click listeners are removed before adding the new one.
        /// </summary>
        public void Bind(string title, string subtitle, Sprite cardIcon, UnityAction onClick)
        {
            if (titleText    != null) titleText.text    = title;
            if (subtitleText != null) subtitleText.text = subtitle;
            if (icon         != null)
            {
                icon.sprite  = cardIcon;
                icon.enabled = cardIcon != null;
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (onClick != null) button.onClick.AddListener(onClick);
            }

            SetBadge(0);
            SetLocked(false);
        }

        /// <summary>
        /// Shows or hides the notification badge.
        /// <paramref name="count"/> = 0 hides the badge entirely.
        /// The number is hidden when count is exactly 1 (badge icon alone is sufficient).
        /// </summary>
        public void SetBadge(int count)
        {
            if (notificationBadge == null) return;
            notificationBadge.SetActive(count > 0);

            if (badgeCountText != null)
                badgeCountText.text = count > 9 ? "9+" : count.ToString();
        }

        /// <summary>Activates or deactivates the locked overlay (greyed-out state).</summary>
        public void SetLocked(bool locked)
        {
            if (lockedOverlay != null) lockedOverlay.SetActive(locked);
            if (button        != null) button.interactable = !locked;
        }

        /// <summary>Programmatically changes the card title (e.g. to show current rank tier).</summary>
        public void SetSubtitle(string subtitle)
        {
            if (subtitleText != null) subtitleText.text = subtitle;
        }
    }
}
