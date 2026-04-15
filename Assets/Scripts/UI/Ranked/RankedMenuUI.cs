using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Currency;
using TCG.Inventory.Deck;
using TCG.Navigation;
using TCG.Ranked;

namespace TCG.UI.Ranked
{
    /// <summary>
    /// Root panel for the Ranked screen.
    ///
    /// Layout (set up in the Inspector):
    ///   • <see cref="rankBadge"/>         — displays current tier / division / RP
    ///   • <see cref="winsText"/>           — "W: 42"
    ///   • <see cref="lossesText"/>         — "L: 18"
    ///   • <see cref="winRateText"/>        — "Win Rate: 70 %"
    ///   • <see cref="playRankedButton"/>   — starts a ranked match
    ///   • <see cref="leaderboardPanel"/>   — show/hide the leaderboard sub-panel
    ///   • <see cref="leaderboardToggle"/> — tab button to open/close leaderboard
    ///   • <see cref="seasonNameText"/>     — e.g. "Season 1"
    ///   • <see cref="seasonDatesText"/>    — e.g. "Jan 1 – Mar 31"
    ///   • <see cref="seasonRewardsPanel"/> — optional panel listing season-end rewards
    ///   • <see cref="selectedDeck"/>       — deck to use for ranked matches; set externally
    /// </summary>
    public class RankedMenuUI : MonoBehaviour
    {
        [Header("Rank Display")]
        [SerializeField] private RankBadgeUI rankBadge;

        [Header("Stats")]
        [SerializeField] private TMP_Text winsText;
        [SerializeField] private TMP_Text lossesText;
        [SerializeField] private TMP_Text winRateText;

        [Header("Buttons")]
        [SerializeField] private Button playRankedButton;
        [SerializeField] private Button leaderboardToggle;

        [Header("Sub-Panels")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private GameObject seasonRewardsPanel;

        [Header("Season Info")]
        [SerializeField] private TMP_Text seasonNameText;
        [SerializeField] private TMP_Text seasonDatesText;

        /// <summary>
        /// Deck used when the player presses Play Ranked.
        /// Assign this from the Deck Builder or a deck selection screen.
        /// </summary>
        [HideInInspector] public DeckData selectedDeck;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnRankedMatchResolved += OnRankedMatchResolved;
            GameEvents.OnSeasonEnded         += OnSeasonEnded;
            GameEvents.OnCurrencyChanged     += OnCurrencyChanged;

            playRankedButton?.onClick.AddListener(OnPlayRankedClicked);
            leaderboardToggle?.onClick.AddListener(OnLeaderboardToggled);

            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnRankedMatchResolved -= OnRankedMatchResolved;
            GameEvents.OnSeasonEnded         -= OnSeasonEnded;
            GameEvents.OnCurrencyChanged     -= OnCurrencyChanged;

            playRankedButton?.onClick.RemoveListener(OnPlayRankedClicked);
            leaderboardToggle?.onClick.RemoveListener(OnLeaderboardToggled);
        }

        // ── Display ────────────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (RankedManager.Instance == null) return;

            var (tier, division, rp) = RankedManager.Instance.GetCurrentRank();
            rankBadge?.Bind(tier, division, rp);

            var (wins, losses, _) = RankedManager.Instance.GetStats();
            float wr = RankedManager.Instance.GetWinRate();

            if (winsText    != null) winsText.text    = $"W: {wins}";
            if (lossesText  != null) lossesText.text  = $"L: {losses}";
            if (winRateText != null) winRateText.text = $"Win Rate: {wr * 100f:0}%";

            RefreshSeasonInfo();
        }

        private void RefreshSeasonInfo()
        {
            // SeasonData can be read via RankedManager's inspector-assigned field.
            // Expose it via a public property if needed; for now the texts are set
            // by matching against the manager's current season through the scene.
            var mgr = RankedManager.Instance;
            if (mgr == null) return;

            // Access current season through the public property (added alongside this UI)
            var season = mgr.CurrentSeason;
            if (season == null) return;

            if (seasonNameText  != null) seasonNameText.text  = season.seasonName;
            if (seasonDatesText != null) seasonDatesText.text = season.startDateDisplay;
        }

        // ── Button Handlers ───────────────────────────────────────────────────────

        private void OnPlayRankedClicked()
        {
            if (RankedManager.Instance == null) return;

            if (selectedDeck == null)
            {
                Debug.LogWarning("[RankedMenuUI] No deck selected — assign selectedDeck before playing ranked.");
                return;
            }

            // Show the match panel before starting so MatchUI can subscribe to OnMatchStarted.
            PanelNavigator.Instance?.Show(PanelNavigator.MatchKey);
            RankedManager.Instance.StartRankedMatch(selectedDeck);
        }

        private void OnLeaderboardToggled()
        {
            if (leaderboardPanel == null) return;
            leaderboardPanel.SetActive(!leaderboardPanel.activeSelf);
        }

        // ── Event Callbacks ───────────────────────────────────────────────────────

        private void OnRankedMatchResolved(RankedMatchOutcome _) => Refresh();
        private void OnSeasonEnded(RankTier _, int __)           => Refresh();
        private void OnCurrencyChanged(CurrencyType _, int __)   { /* currency display handled elsewhere */ }
    }
}
