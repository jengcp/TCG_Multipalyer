using UnityEngine;
using TMPro;
using TCG.Match;

namespace TCG.UI.Match
{
    /// <summary>
    /// Displays the current phase name as a banner.
    /// Triggers the "Show" Animator parameter so the banner can animate in/out.
    /// </summary>
    public class PhaseIndicatorUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text  phaseLabel;
        [SerializeField] private TMP_Text  playerLabel;
        [SerializeField] private Animator  animator;

        private static readonly int ShowHash = Animator.StringToHash("Show");

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Updates the displayed phase name and player label, then triggers the Show animation.
        /// </summary>
        public void SetPhase(MatchPhase phase, int playerIndex)
        {
            if (phaseLabel != null)
                phaseLabel.text = FormatPhase(phase);

            if (playerLabel != null)
                playerLabel.text = playerIndex == 0 ? "Your Turn" : "Opponent's Turn";

            if (animator != null)
                animator.SetTrigger(ShowHash);
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        private static string FormatPhase(MatchPhase phase) => phase switch
        {
            MatchPhase.DrawPhase   => "Draw Phase",
            MatchPhase.MainPhase   => "Main Phase",
            MatchPhase.CombatPhase => "Combat Phase",
            MatchPhase.EndPhase    => "End Phase",
            MatchPhase.MatchOver   => "Match Over",
            _                      => phase.ToString()
        };
    }
}
