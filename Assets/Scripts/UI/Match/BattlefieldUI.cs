using System.Collections.Generic;
using UnityEngine;
using TCG.Match;

namespace TCG.UI.Match
{
    /// <summary>
    /// Manages both players' 5-slot battlefield rows.
    /// Creates slot UIs at Awake and refreshes them from MatchState.
    /// </summary>
    public class BattlefieldUI : MonoBehaviour
    {
        [Header("Slot Prefab & Containers")]
        [SerializeField] private BattlefieldSlotUI slotPrefab;
        [SerializeField] private Transform         localPlayerRow;
        [SerializeField] private Transform         opponentRow;
        [SerializeField] private LocalPlayerController controller;

        private readonly List<BattlefieldSlotUI> _localSlots    = new();
        private readonly List<BattlefieldSlotUI> _opponentSlots = new();

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            for (int i = 0; i < PlayerState.MaxBattlefieldSlots; i++)
            {
                _localSlots.Add(CreateSlot(localPlayerRow, isOpponent: false));
                _opponentSlots.Add(CreateSlot(opponentRow,  isOpponent: true));
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Refreshes all slots from the current MatchState.</summary>
        public void Refresh(MatchState state)
        {
            if (state == null) return;

            var local    = state.Players[0];
            var opponent = state.Players[1];

            for (int i = 0; i < PlayerState.MaxBattlefieldSlots; i++)
            {
                _localSlots[i].Bind(local.Battlefield[i],    controller, isOpponentRow: false);
                _opponentSlots[i].Bind(opponent.Battlefield[i], controller, isOpponentRow: true);
            }
        }

        /// <summary>Refreshes a single slot (more efficient than full Refresh).</summary>
        public void RefreshSlot(int playerIdx, int slotIdx, MatchState state)
        {
            var slots = playerIdx == 0 ? _localSlots : _opponentSlots;
            if (slotIdx < 0 || slotIdx >= slots.Count) return;
            slots[slotIdx].Bind(
                state.Players[playerIdx].Battlefield[slotIdx],
                controller,
                isOpponentRow: playerIdx != 0);
        }

        // ── Factory ────────────────────────────────────────────────────────────────

        private BattlefieldSlotUI CreateSlot(Transform parent, bool isOpponent)
        {
            var slot = Instantiate(slotPrefab, parent);
            slot.IsOpponentRow = isOpponent;
            return slot;
        }
    }
}
