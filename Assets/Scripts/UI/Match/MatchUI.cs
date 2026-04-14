using UnityEngine;
using TCG.Core;
using TCG.Match;

namespace TCG.UI.Match
{
    /// <summary>
    /// Root UI orchestrator for a match scene.
    /// Subscribes to all relevant GameEvents and delegates to the sub-panels below it.
    ///
    /// Sub-panel references are assigned in the Inspector.
    /// </summary>
    public class MatchUI : MonoBehaviour
    {
        [Header("Player Status")]
        [SerializeField] private PlayerStatusUI localPlayerStatus;
        [SerializeField] private PlayerStatusUI opponentStatus;

        [Header("Battlefield")]
        [SerializeField] private BattlefieldUI battlefield;

        [Header("Hand")]
        [SerializeField] private HandUI handUI;

        [Header("Phase Indicator")]
        [SerializeField] private PhaseIndicatorUI phaseIndicator;

        [Header("Result Screen")]
        [SerializeField] private MatchResultUI resultUI;

        // Cached state reference set on OnMatchStarted
        private MatchState _state;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnMatchStarted         += OnMatchStarted;
            GameEvents.OnTurnStarted          += OnTurnStarted;
            GameEvents.OnPhaseChanged         += OnPhaseChanged;
            GameEvents.OnCardDrawn            += OnCardDrawn;
            GameEvents.OnCardPlacedOnBattlefield += OnCardPlacedOnBattlefield;
            GameEvents.OnCardSentToGraveyard  += OnCardSentToGraveyard;
            GameEvents.OnCardReturnedToHand   += OnCardReturnedToHand;
            GameEvents.OnPlayerHealthChanged  += OnPlayerHealthChanged;
            GameEvents.OnCreatureDied         += OnCreatureDied;
            GameEvents.OnMatchEnded           += OnMatchEnded;
        }

        private void OnDisable()
        {
            GameEvents.OnMatchStarted         -= OnMatchStarted;
            GameEvents.OnTurnStarted          -= OnTurnStarted;
            GameEvents.OnPhaseChanged         -= OnPhaseChanged;
            GameEvents.OnCardDrawn            -= OnCardDrawn;
            GameEvents.OnCardPlacedOnBattlefield -= OnCardPlacedOnBattlefield;
            GameEvents.OnCardSentToGraveyard  -= OnCardSentToGraveyard;
            GameEvents.OnCardReturnedToHand   -= OnCardReturnedToHand;
            GameEvents.OnPlayerHealthChanged  -= OnPlayerHealthChanged;
            GameEvents.OnCreatureDied         -= OnCreatureDied;
            GameEvents.OnMatchEnded           -= OnMatchEnded;
        }

        // ── Event Handlers ─────────────────────────────────────────────────────────

        private void OnMatchStarted(MatchState state)
        {
            _state = state;
            RefreshAll();
        }

        private void OnTurnStarted(int turn, int playerIndex)
        {
            RefreshStatus();
            RefreshHand();
        }

        private void OnPhaseChanged(MatchPhase phase)
        {
            phaseIndicator?.SetPhase(phase, _state?.CurrentPlayerIndex ?? 0);
            RefreshHand();
        }

        private void OnCardDrawn(CardInstance card, int playerIndex)
        {
            if (playerIndex == 0) RefreshHand();
            RefreshStatus();
        }

        private void OnCardPlacedOnBattlefield(CardInstance card, int playerIndex, int slotIndex)
        {
            battlefield?.RefreshSlot(playerIndex, slotIndex, _state);
            RefreshHand();
            RefreshStatus();
        }

        private void OnCardSentToGraveyard(CardInstance card, int playerIndex)
        {
            battlefield?.Refresh(_state);
            RefreshStatus();
        }

        private void OnCardReturnedToHand(CardInstance card, int playerIndex)
        {
            battlefield?.Refresh(_state);
            if (playerIndex == 0) RefreshHand();
        }

        private void OnPlayerHealthChanged(int playerIndex, int newHealth, int delta)
        {
            if (playerIndex == 0)
                localPlayerStatus?.UpdateHealth(newHealth);
            else
                opponentStatus?.UpdateHealth(newHealth);
        }

        private void OnCreatureDied(CardInstance card, int playerIndex)
        {
            battlefield?.Refresh(_state);
        }

        private void OnMatchEnded(MatchResult result, MatchState state, MatchRewards rewards)
        {
            resultUI?.Show(result, rewards);
        }

        // ── Full Refresh ───────────────────────────────────────────────────────────

        private void RefreshAll()
        {
            RefreshStatus();
            RefreshHand();
            battlefield?.Refresh(_state);
        }

        private void RefreshStatus()
        {
            if (_state == null) return;
            localPlayerStatus?.Refresh(_state.Players[0]);
            opponentStatus?.Refresh(_state.Players[1]);
        }

        private void RefreshHand()
        {
            if (_state == null) return;
            var local = _state.Players[0];
            handUI?.Refresh(local.Hand, local.CurrentMana);
        }
    }
}
