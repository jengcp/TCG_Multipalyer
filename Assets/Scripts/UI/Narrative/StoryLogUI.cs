using System.Collections.Generic;
using UnityEngine;
using TCG.Core;
using TCG.Narrative;

namespace TCG.UI.Narrative
{
    /// <summary>
    /// Story Log panel — shows all narrative events the player has unlocked,
    /// in the order they were defined in <see cref="NarrativeConfig"/>.
    ///
    /// Uses a grow-never-shrink pool of <see cref="StoryLogEntryUI"/> tiles.
    /// Refreshes automatically when new narrative events fire.
    ///
    /// Inspector setup:
    ///   • <see cref="entryPrefab"/>    — <see cref="StoryLogEntryUI"/> prefab
    ///   • <see cref="contentParent"/>  — ScrollView Content transform
    ///   • <see cref="emptyLabel"/>     — Text shown when no entries are unlocked yet (optional)
    /// </summary>
    public class StoryLogUI : MonoBehaviour
    {
        [SerializeField] private StoryLogEntryUI entryPrefab;
        [SerializeField] private Transform       contentParent;
        [SerializeField] private GameObject      emptyLabel;

        // ── Pool ───────────────────────────────────────────────────────────────────
        private readonly List<StoryLogEntryUI> _pool = new();

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnNarrativeEventTriggered += OnNarrativeEventTriggered;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnNarrativeEventTriggered -= OnNarrativeEventTriggered;
        }

        // ── Refresh ────────────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (NarrativeManager.Instance == null) return;

            var entries = NarrativeManager.Instance.GetUnlockedEntries();

            // Grow pool if needed
            while (_pool.Count < entries.Count)
            {
                var tile = Instantiate(entryPrefab, contentParent);
                _pool.Add(tile);
            }

            // Bind visible tiles
            for (int i = 0; i < _pool.Count; i++)
            {
                bool active = i < entries.Count;
                _pool[i].gameObject.SetActive(active);
                if (active) _pool[i].Bind(entries[i]);
            }

            if (emptyLabel != null)
                emptyLabel.SetActive(entries.Count == 0);
        }

        // ── Handlers ──────────────────────────────────────────────────────────────

        private void OnNarrativeEventTriggered(NarrativeEventData _) => Refresh();
    }
}
