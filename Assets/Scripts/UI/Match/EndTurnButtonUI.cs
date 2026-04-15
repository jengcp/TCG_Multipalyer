using UnityEngine;
using UnityEngine.UI;
using TCG.Match;

namespace TCG.UI.Match
{
    /// <summary>
    /// "End Turn" button handler.
    /// Clicking advances the current phase (via TurnManager) and flushes any
    /// pending local player input (via MatchManager) so the match loop unblocks.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EndTurnButtonUI : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnEndTurnClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnEndTurnClicked);
        }

        // ── Handler ────────────────────────────────────────────────────────────────

        private void OnEndTurnClicked()
        {
            TurnManager.Instance?.AdvancePhase();
            MatchManager.Instance?.FlushPendingInput();
        }
    }
}
