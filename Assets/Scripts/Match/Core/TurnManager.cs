using System.Collections;
using UnityEngine;
using TCG.Core;

namespace TCG.Match
{
    /// <summary>
    /// Singleton MonoBehaviour that sequences the four phases of a turn:
    /// DrawPhase → MainPhase → CombatPhase → EndPhase.
    ///
    /// MatchManager drives RunTurn. EndTurnButtonUI calls AdvancePhase() to skip
    /// the current phase mid-execution.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        private MatchState  _state;
        private bool        _phaseAdvanceRequested;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        public void Initialize(MatchState state)
        {
            _state = state;
            _phaseAdvanceRequested = false;
        }

        /// <summary>
        /// Called by EndTurnButtonUI.  Signals the current phase loop to exit early.
        /// </summary>
        public void AdvancePhase()
        {
            _phaseAdvanceRequested = true;
        }

        // ── Core turn loop ─────────────────────────────────────────────────────────

        /// <summary>
        /// Runs one complete turn for the active player.
        /// MatchManager passes itself so this coroutine can call RunMainPhase / RunCombatPhase.
        /// </summary>
        public IEnumerator RunTurn(IPlayerController controller, MatchManager manager)
        {
            _state.IncrementTurn();
            int playerIdx = _state.CurrentPlayerIndex;
            GameEvents.RaiseTurnStarted(_state.TurnNumber, playerIdx);

            // ── Draw Phase ────────────────────────────────────────────────────────
            yield return TransitionToPhase(MatchPhase.DrawPhase);
            Effects.CardEffectProcessor.DrawCardForPlayer(_state.ActivePlayer, _state);
            // Brief pause so the player can see the drawn card
            yield return new WaitForSeconds(0.3f);

            // ── Main Phase ────────────────────────────────────────────────────────
            yield return TransitionToPhase(MatchPhase.MainPhase);
            yield return manager.RunMainPhase(controller);

            // ── Combat Phase ──────────────────────────────────────────────────────
            yield return TransitionToPhase(MatchPhase.CombatPhase);
            yield return manager.RunCombatPhase(controller);

            // ── End Phase ─────────────────────────────────────────────────────────
            yield return TransitionToPhase(MatchPhase.EndPhase);
            EndPhaseCleanup();
            yield return new WaitForSeconds(0.2f);

            // Hand off turn: swap active player and give them mana
            int nextPlayer = 1 - playerIdx;
            _state.SetCurrentPlayer(nextPlayer);
            _state.ActivePlayer.RefreshMana();

            GameEvents.RaiseTurnEnded(_state.TurnNumber, playerIdx);
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        private IEnumerator TransitionToPhase(MatchPhase phase)
        {
            _phaseAdvanceRequested = false;
            _state.SetPhase(phase);
            GameEvents.RaisePhaseChanged(phase);
            yield return null;
        }

        private void EndPhaseCleanup()
        {
            // Untap all creatures owned by the player whose turn just ended
            foreach (var slot in _state.ActivePlayer.Battlefield)
                slot.Occupant?.Untap();

            // Trim hand to maximum size (discard from end, i.e. most recently drawn)
            var hand = _state.ActivePlayer.Hand;
            while (hand.Count > PlayerState.MaxHandSize)
            {
                var discarded = hand[hand.Count - 1];
                hand.RemoveAt(hand.Count - 1);
                discarded.SetStatus(CardInstanceStatus.InGraveyard);
                _state.ActivePlayer.Graveyard.Add(discarded);
                GameEvents.RaiseCardSentToGraveyard(discarded, _state.ActivePlayer.PlayerIndex);
            }
        }
    }
}
