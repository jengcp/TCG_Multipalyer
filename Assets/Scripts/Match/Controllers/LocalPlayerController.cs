using System;
using System.Collections;
using UnityEngine;

namespace TCG.Match
{
    /// <summary>
    /// IPlayerController implementation for the local human player.
    /// Bridges the coroutine-based match loop to Unity UI clicks.
    ///
    /// Pattern:
    ///   1. Select* coroutine sets a pending callback then waits.
    ///   2. UI component (CardInHandUI, BattlefieldSlotUI) calls the matching Notify* method.
    ///   3. Notify* fires the callback and nulls it, unblocking the coroutine.
    ///   4. FlushPendingInput() can unblock all at once (used by EndTurnButton).
    /// </summary>
    public class LocalPlayerController : MonoBehaviour, IPlayerController
    {
        private Action<CardInstance>    _pendingCardSelection;
        private Action<BattlefieldSlot> _pendingAttackSelection;
        private Action<BattlefieldSlot> _pendingDefendSelection;

        // ── Called by UI ──────────────────────────────────────────────────────────

        /// <summary>Called by CardInHandUI when the player taps a card tile.</summary>
        public void NotifyCardSelected(CardInstance card)
        {
            var cb = _pendingCardSelection;
            _pendingCardSelection = null;
            cb?.Invoke(card);
        }

        /// <summary>Called by BattlefieldSlotUI (own row) when the player taps an attacker.</summary>
        public void NotifyAttackSlotSelected(BattlefieldSlot slot)
        {
            var cb = _pendingAttackSelection;
            _pendingAttackSelection = null;
            cb?.Invoke(slot);
        }

        /// <summary>Called by BattlefieldSlotUI (opponent row) to select a defend target. Null = direct.</summary>
        public void NotifyDefendSlotSelected(BattlefieldSlot slot)
        {
            var cb = _pendingDefendSelection;
            _pendingDefendSelection = null;
            cb?.Invoke(slot);
        }

        /// <summary>
        /// Flushes all pending selections with null, unblocking any waiting coroutines.
        /// Called by EndTurnButtonUI so the match loop can advance past mid-phase selections.
        /// </summary>
        public void FlushPendingInput()
        {
            NotifyCardSelected(null);
            NotifyAttackSlotSelected(null);
            NotifyDefendSlotSelected(null);
        }

        // ── IPlayerController ─────────────────────────────────────────────────────

        public IEnumerator TakeTurn(PlayerState myState, MatchState matchState)
        {
            yield return null; // Phases are driven externally by TurnManager/MatchManager
        }

        public IEnumerator SelectCardToPlay(PlayerState state, Action<CardInstance> onSelected)
        {
            _pendingCardSelection = onSelected;
            yield return new WaitUntil(() => _pendingCardSelection == null);
        }

        public IEnumerator SelectAttackSlot(PlayerState state, Action<BattlefieldSlot> onSelected)
        {
            _pendingAttackSelection = onSelected;
            yield return new WaitUntil(() => _pendingAttackSelection == null);
        }

        public IEnumerator SelectDefendSlot(PlayerState opponentState, Action<BattlefieldSlot> onSelected)
        {
            _pendingDefendSelection = onSelected;
            yield return new WaitUntil(() => _pendingDefendSelection == null);
        }
    }
}
