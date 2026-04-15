using System.Collections.Generic;
using UnityEngine;
using TCG.Ranked;

namespace TCG.UI.Ranked
{
    /// <summary>
    /// Scrollable leaderboard panel.
    /// Calls <see cref="RankedManager.GetLeaderboard"/> on <see cref="Refresh"/> and
    /// binds each <see cref="LeaderboardEntry"/> to a pooled <see cref="LeaderboardEntryUI"/> row.
    ///
    /// Inspector setup:
    ///   • <see cref="entryPrefab"/>    — LeaderboardEntryUI prefab
    ///   • <see cref="listContainer"/>  — Content transform of the Scroll View
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [SerializeField] private LeaderboardEntryUI entryPrefab;
        [SerializeField] private Transform          listContainer;

        private readonly List<LeaderboardEntryUI> _pool = new();

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void OnEnable() => Refresh();

        // ── Public API ─────────────────────────────────────────────────────────────

        public void Refresh()
        {
            if (RankedManager.Instance == null || entryPrefab == null) return;

            var entries = RankedManager.Instance.GetLeaderboard();
            EnsurePoolSize(entries.Count);

            for (int i = 0; i < _pool.Count; i++)
            {
                bool active = i < entries.Count;
                _pool[i].gameObject.SetActive(active);
                if (active) _pool[i].Bind(i + 1, entries[i]);
            }
        }

        // ── Pool ──────────────────────────────────────────────────────────────────

        private void EnsurePoolSize(int needed)
        {
            while (_pool.Count < needed)
            {
                var row = Instantiate(entryPrefab, listContainer);
                _pool.Add(row);
            }
        }
    }
}
