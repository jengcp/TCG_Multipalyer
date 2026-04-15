using UnityEngine;
using TCG.Core;
using TCG.Navigation;
using TCG.Quest;
using TCG.Ranked;

namespace TCG.UI.MainMenu
{
    /// <summary>
    /// Root controller for the main menu scene.
    ///
    /// Responsibilities:
    ///   1. Register all game-mode panels with <see cref="PanelNavigator"/> at startup.
    ///   2. Show the home panel (this GameObject's panel) as the initial view.
    ///   3. Bind each <see cref="ModeCardUI"/> tile to its target panel / action.
    ///   4. Refresh notification badges (quests completed, rank tier subtitle).
    ///
    /// Scene setup:
    ///   - Attach to the root GameObject that contains the home-screen card grid.
    ///   - Assign every panel reference in the Inspector.
    ///   - The home panel is this component's own GameObject.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        // ── Panel references ──────────────────────────────────────────────────────
        [Header("Panels — assign every root panel GameObject")]
        [SerializeField] private GameObject matchPanel;
        [SerializeField] private GameObject campaignPanel;
        [SerializeField] private GameObject rankedPanel;
        [SerializeField] private GameObject quickMatchPanel;
        [SerializeField] private GameObject collectionPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject gachaPanel;
        [SerializeField] private GameObject charactersPanel;
        [SerializeField] private GameObject questsPanel;

        // ── Mode cards ────────────────────────────────────────────────────────────
        [Header("Mode Cards — one per tile in the grid")]
        [SerializeField] private ModeCardUI campaignCard;
        [SerializeField] private ModeCardUI rankedCard;
        [SerializeField] private ModeCardUI quickMatchCard;
        [SerializeField] private ModeCardUI collectionCard;
        [SerializeField] private ModeCardUI shopCard;
        [SerializeField] private ModeCardUI gachaCard;
        [SerializeField] private ModeCardUI charactersCard;
        [SerializeField] private ModeCardUI questsCard;

        // ── Card icons (assign matching sprites) ───────────────────────────────────
        [Header("Mode Card Icons (optional)")]
        [SerializeField] private Sprite campaignIcon;
        [SerializeField] private Sprite rankedIcon;
        [SerializeField] private Sprite quickMatchIcon;
        [SerializeField] private Sprite collectionIcon;
        [SerializeField] private Sprite shopIcon;
        [SerializeField] private Sprite gachaIcon;
        [SerializeField] private Sprite charactersIcon;
        [SerializeField] private Sprite questsIcon;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Start()
        {
            RegisterPanels();
            BindCards();
            PanelNavigator.Instance?.ShowHome();
        }

        private void OnEnable()
        {
            GameEvents.OnQuestCompleted        += OnQuestStateChanged;
            GameEvents.OnQuestClaimed          += OnQuestStateChanged;
            GameEvents.OnRankedMatchResolved   += OnRankedMatchResolved;
            RefreshBadges();
        }

        private void OnDisable()
        {
            GameEvents.OnQuestCompleted        -= OnQuestStateChanged;
            GameEvents.OnQuestClaimed          -= OnQuestStateChanged;
            GameEvents.OnRankedMatchResolved   -= OnRankedMatchResolved;
        }

        // ── Setup ─────────────────────────────────────────────────────────────────

        private void RegisterPanels()
        {
            var nav = PanelNavigator.Instance;
            if (nav == null) return;

            // Home panel = this GameObject itself
            nav.RegisterPanel(PanelNavigator.HomeKey,       gameObject);
            nav.RegisterPanel(PanelNavigator.MatchKey,      matchPanel);
            nav.RegisterPanel(PanelNavigator.CampaignKey,   campaignPanel);
            nav.RegisterPanel(PanelNavigator.RankedKey,     rankedPanel);
            nav.RegisterPanel(PanelNavigator.QuickMatchKey, quickMatchPanel);
            nav.RegisterPanel(PanelNavigator.CollectionKey, collectionPanel);
            nav.RegisterPanel(PanelNavigator.ShopKey,       shopPanel);
            nav.RegisterPanel(PanelNavigator.GachaKey,      gachaPanel);
            nav.RegisterPanel(PanelNavigator.CharactersKey, charactersPanel);
            nav.RegisterPanel(PanelNavigator.QuestsKey,     questsPanel);
        }

        private void BindCards()
        {
            campaignCard?.Bind(
                "Campaign",
                "Story Mode",
                campaignIcon,
                () => PanelNavigator.Instance?.Show(PanelNavigator.CampaignKey));

            rankedCard?.Bind(
                "Ranked",
                "Competitive",
                rankedIcon,
                () => PanelNavigator.Instance?.Show(PanelNavigator.RankedKey));

            quickMatchCard?.Bind(
                "Quick Match",
                "Practice vs AI",
                quickMatchIcon,
                () => PanelNavigator.Instance?.Show(PanelNavigator.QuickMatchKey));

            collectionCard?.Bind(
                "Collection",
                "Cards & Decks",
                collectionIcon,
                () => PanelNavigator.Instance?.Show(PanelNavigator.CollectionKey));

            shopCard?.Bind(
                "Shop",
                "Daily Offers",
                shopIcon,
                () => PanelNavigator.Instance?.Show(PanelNavigator.ShopKey));

            gachaCard?.Bind(
                "Card Packs",
                "Open Packs",
                gachaIcon,
                () => PanelNavigator.Instance?.Show(PanelNavigator.GachaKey));

            charactersCard?.Bind(
                "Characters",
                "Roster",
                charactersIcon,
                () => PanelNavigator.Instance?.Show(PanelNavigator.CharactersKey));

            questsCard?.Bind(
                "Quests",
                "Daily & Weekly",
                questsIcon,
                () => PanelNavigator.Instance?.Show(PanelNavigator.QuestsKey));
        }

        // ── Notification Badges ───────────────────────────────────────────────────

        private void RefreshBadges()
        {
            RefreshQuestBadge();
            RefreshRankedSubtitle();
        }

        private void RefreshQuestBadge()
        {
            if (questsCard == null) return;

            int claimable = 0;
            var mgr = QuestManager.Instance;
            if (mgr != null)
            {
                foreach (var q in mgr.DailyQuests)
                    if (q.Status == QuestStatus.Completed) claimable++;
                foreach (var q in mgr.WeeklyQuests)
                    if (q.Status == QuestStatus.Completed) claimable++;
            }

            questsCard.SetBadge(claimable);
        }

        private void RefreshRankedSubtitle()
        {
            if (rankedCard == null || RankedManager.Instance == null) return;

            var (tier, division, _) = RankedManager.Instance.GetCurrentRank();
            string divLabel = division == RankDivision.None
                ? string.Empty
                : $" {RomanDivision(division)}";

            rankedCard.SetSubtitle($"{tier}{divLabel}");
        }

        private static string RomanDivision(RankDivision div) => div switch
        {
            RankDivision.DivIII => "III",
            RankDivision.DivII  => "II",
            RankDivision.DivI   => "I",
            _                   => string.Empty,
        };

        // ── Event Callbacks ───────────────────────────────────────────────────────

        private void OnQuestStateChanged(QuestProgress _)    => RefreshQuestBadge();
        private void OnRankedMatchResolved(RankedMatchOutcome _) => RefreshRankedSubtitle();
    }
}
