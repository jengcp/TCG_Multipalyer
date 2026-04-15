using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Ranked;

namespace TCG.UI.Ranked
{
    /// <summary>
    /// A single row in the ranked leaderboard.
    /// Bind via <see cref="Bind(int, LeaderboardEntry)"/>.
    ///
    /// Inspector setup:
    ///   • <see cref="positionText"/>   — rank position number (e.g. "#1")
    ///   • <see cref="playerNameText"/> — player name
    ///   • <see cref="badge"/>          — compact RankBadgeUI (no RP bar needed; hide rpBar in prefab)
    ///   • <see cref="winsText"/>       — win count
    ///   • <see cref="lossesText"/>     — loss count
    ///   • <see cref="rpValueText"/>    — absolute RP number
    ///   • <see cref="rowBackground"/>  — Image tinted to highlight the local player's row
    ///   • <see cref="localPlayerColor"/>  — highlight tint
    ///   • <see cref="defaultRowColor"/>   — normal row tint
    /// </summary>
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [Header("Row Fields")]
        [SerializeField] private TMP_Text    positionText;
        [SerializeField] private TMP_Text    playerNameText;
        [SerializeField] private RankBadgeUI badge;
        [SerializeField] private TMP_Text    winsText;
        [SerializeField] private TMP_Text    lossesText;
        [SerializeField] private TMP_Text    rpValueText;

        [Header("Highlight")]
        [SerializeField] private Image rowBackground;
        [SerializeField] private Color localPlayerColor = new Color(1f, 0.92f, 0.6f, 0.35f);
        [SerializeField] private Color defaultRowColor  = new Color(1f, 1f, 1f, 0.07f);

        // ── Public API ─────────────────────────────────────────────────────────────

        public void Bind(int position, LeaderboardEntry entry)
        {
            if (positionText   != null) positionText.text   = $"#{position}";
            if (playerNameText != null) playerNameText.text  = entry.PlayerName;
            if (winsText       != null) winsText.text        = entry.Wins.ToString();
            if (lossesText     != null) lossesText.text      = entry.Losses.ToString();
            if (rpValueText    != null) rpValueText.text     = entry.AbsoluteRP.ToString();

            badge?.Bind(entry.Tier, entry.Division, entry.RP);

            if (rowBackground != null)
                rowBackground.color = entry.IsLocalPlayer ? localPlayerColor : defaultRowColor;
        }
    }
}
