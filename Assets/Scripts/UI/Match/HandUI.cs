using System.Collections.Generic;
using UnityEngine;
using TCG.Match;

namespace TCG.UI.Match
{
    /// <summary>
    /// Manages the row of <see cref="CardInHandUI"/> tiles for one player's hand.
    /// Uses a pool that grows but never shrinks to avoid GC churn.
    /// </summary>
    public class HandUI : MonoBehaviour
    {
        [SerializeField] private CardInHandUI         cardPrefab;
        [SerializeField] private Transform            container;
        [SerializeField] private LocalPlayerController controller;

        private readonly List<CardInHandUI> _pool = new();

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Rebuilds the hand display for <paramref name="hand"/> at the given mana level.
        /// </summary>
        public void Refresh(IReadOnlyList<CardInstance> hand, int currentMana)
        {
            EnsurePoolSize(hand.Count);

            for (int i = 0; i < _pool.Count; i++)
            {
                bool active = i < hand.Count;
                _pool[i].gameObject.SetActive(active);

                if (active)
                    _pool[i].Bind(hand[i], currentMana, controller);
            }
        }

        /// <summary>Updates affordability dim on all visible tiles without re-binding.</summary>
        public void SetCurrentMana(int currentMana)
        {
            foreach (var tile in _pool)
                if (tile.gameObject.activeSelf)
                    tile.SetAffordable(true); // re-check affordability via Bind on next Refresh
        }

        // ── Pool ───────────────────────────────────────────────────────────────────

        private void EnsurePoolSize(int required)
        {
            while (_pool.Count < required)
            {
                var tile = Instantiate(cardPrefab, container);
                tile.gameObject.SetActive(false);
                _pool.Add(tile);
            }
        }
    }
}
