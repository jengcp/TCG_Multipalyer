using System;
using System.Collections.Generic;
using UnityEngine;

namespace TCG.Navigation
{
    /// <summary>
    /// Lightweight single-scene panel manager.
    ///
    /// Panels are registered by string key (<see cref="RegisterPanel"/>) and activated
    /// one at a time. A history stack enables back-navigation (<see cref="Back"/>).
    /// <see cref="ShowHome"/> always returns to <see cref="HomeKey"/> and clears the stack.
    ///
    /// Usage (in scene root MonoBehaviour or <see cref="UI.MainMenu.MainMenuUI"/>):
    /// <code>
    ///   PanelNavigator.Instance.RegisterPanel("Shop", shopPanel);
    ///   PanelNavigator.Instance.Show("Shop");
    ///   PanelNavigator.Instance.Back();
    /// </code>
    ///
    /// <para>
    /// All registered panels start inactive. Call <see cref="ShowHome"/> in Start()
    /// to activate the initial home panel.
    /// </para>
    /// </summary>
    public class PanelNavigator : MonoBehaviour
    {
        public static PanelNavigator Instance { get; private set; }

        // ── Keys (use these constants to avoid typo bugs) ─────────────────────────
        public const string HomeKey       = "Home";
        public const string MatchKey      = "Match";
        public const string CampaignKey   = "Campaign";
        public const string RankedKey     = "Ranked";
        public const string QuickMatchKey = "QuickMatch";
        public const string CollectionKey = "Collection";
        public const string ShopKey       = "Shop";
        public const string GachaKey      = "Gacha";
        public const string CharactersKey = "Characters";
        public const string QuestsKey     = "Quests";

        // ── Events ────────────────────────────────────────────────────────────────
        /// <summary>
        /// Fires whenever a panel is shown.
        /// Arguments: (previousKey, newKey). previousKey is null on first show.
        /// </summary>
        public static event Action<string, string> OnPanelChanged;

        // ── State ─────────────────────────────────────────────────────────────────
        private readonly Dictionary<string, GameObject> _panels  = new();
        private readonly Stack<string>                  _history = new();
        private string                                  _current;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Registration ──────────────────────────────────────────────────────────

        /// <summary>
        /// Registers a panel under <paramref name="key"/> and deactivates it immediately.
        /// Call this before any <see cref="Show"/> calls (typically in Start()).
        /// </summary>
        public void RegisterPanel(string key, GameObject panel)
        {
            if (panel == null)
            {
                Debug.LogWarning($"[PanelNavigator] Null panel passed for key '{key}'.");
                return;
            }

            _panels[key] = panel;
            panel.SetActive(false);
        }

        // ── Navigation ────────────────────────────────────────────────────────────

        /// <summary>
        /// Deactivates the current panel, activates the panel with <paramref name="key"/>,
        /// and pushes the previous key onto the history stack.
        /// </summary>
        public void Show(string key)
        {
            if (!_panels.TryGetValue(key, out var next))
            {
                Debug.LogWarning($"[PanelNavigator] Panel '{key}' not registered.");
                return;
            }

            string previous = _current;

            // Hide current
            if (_current != null && _panels.TryGetValue(_current, out var current))
                current.SetActive(false);

            // Push history (avoid duplicate consecutive entries)
            if (_current != null && _current != key)
                _history.Push(_current);

            // Show next
            _current = key;
            next.SetActive(true);

            OnPanelChanged?.Invoke(previous, key);
        }

        /// <summary>
        /// Returns to the previous panel. Falls back to <see cref="HomeKey"/> if the history is empty.
        /// </summary>
        public void Back()
        {
            string target = _history.Count > 0 ? _history.Pop() : HomeKey;
            Show(target);
        }

        /// <summary>
        /// Shows the <see cref="HomeKey"/> panel and clears the navigation history.
        /// </summary>
        public void ShowHome()
        {
            _history.Clear();
            Show(HomeKey);
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        /// <summary>The key of the currently visible panel, or null if none has been shown yet.</summary>
        public string CurrentPanel => _current;

        /// <summary>True when the history stack has at least one entry (Back() would go somewhere).</summary>
        public bool CanGoBack => _history.Count > 0;

        /// <summary>Returns the registered panel GameObject for <paramref name="key"/>, or null.</summary>
        public bool TryGetPanel(string key, out GameObject panel) => _panels.TryGetValue(key, out panel);
    }
}
