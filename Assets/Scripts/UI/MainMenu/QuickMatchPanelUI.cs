using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Campaign;
using TCG.Inventory.Deck;
using TCG.Match;
using TCG.Navigation;

namespace TCG.UI.MainMenu
{
    /// <summary>
    /// Deck-selection panel for a casual (no-stakes) practice match vs the AI.
    ///
    /// The player cycles through their saved decks with Previous / Next buttons,
    /// then presses Play to launch a match via <see cref="MatchManager"/>.
    ///
    /// Inspector setup:
    ///   • <see cref="deckNameText"/>     — name of the currently selected deck
    ///   • <see cref="deckInfoText"/>     — e.g. "24 cards" or "Invalid — needs 20+"
    ///   • <see cref="prevButton"/>       — cycles to the previous deck
    ///   • <see cref="nextButton"/>       — cycles to the next deck
    ///   • <see cref="playButton"/>       — starts the match (disabled if no valid deck)
    ///   • <see cref="noDeckMessage"/>    — shown when the player has no decks at all
    ///   • <see cref="localController"/>  — LocalPlayerController in the scene
    ///   • <see cref="aiController"/>     — AIPlayerController in the scene
    ///   • <see cref="defaultAiDeck"/>    — fallback AI deck; if empty, uses the first
    ///       opponent from <see cref="TCG.Ranked.RankedManager.CurrentSeason"/>
    /// </summary>
    public class QuickMatchPanelUI : MonoBehaviour
    {
        [Header("Deck Selector")]
        [SerializeField] private TMP_Text   deckNameText;
        [SerializeField] private TMP_Text   deckInfoText;
        [SerializeField] private Button     prevButton;
        [SerializeField] private Button     nextButton;
        [SerializeField] private Button     playButton;
        [SerializeField] private GameObject noDeckMessage;

        [Header("Controllers")]
        [SerializeField] private LocalPlayerController localController;
        [SerializeField] private AIPlayerController    aiController;

        [Header("Default AI Deck (fallback)")]
        [Tooltip("Deck entries the AI uses for quick match. If empty, falls back to the first " +
                 "RankedManager season opponent.")]
        [SerializeField] private AiDeckEntry[] defaultAiDeck;

        // ── State ──────────────────────────────────────────────────────────────────
        private int _deckIndex;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            prevButton?.onClick.AddListener(OnPrevClicked);
            nextButton?.onClick.AddListener(OnNextClicked);
            playButton?.onClick.AddListener(OnPlayClicked);
        }

        private void OnDestroy()
        {
            prevButton?.onClick.RemoveListener(OnPrevClicked);
            nextButton?.onClick.RemoveListener(OnNextClicked);
            playButton?.onClick.RemoveListener(OnPlayClicked);
        }

        private void OnEnable() => Refresh();

        // ── Display ────────────────────────────────────────────────────────────────

        private void Refresh()
        {
            var decks = DeckManager.Instance?.Decks;
            bool hasDecks = decks != null && decks.Count > 0;

            if (noDeckMessage != null) noDeckMessage.SetActive(!hasDecks);
            if (playButton    != null) playButton.interactable = false;

            if (!hasDecks)
            {
                if (deckNameText != null) deckNameText.text = "No decks";
                if (deckInfoText != null) deckInfoText.text = "Build a deck in Collection first.";
                SetNavButtonsActive(false);
                return;
            }

            SetNavButtonsActive(true);

            _deckIndex = Mathf.Clamp(_deckIndex, 0, decks.Count - 1);
            var deck   = decks[_deckIndex];

            if (deckNameText != null)
                deckNameText.text = deck.DeckName;

            if (deckInfoText != null)
            {
                bool valid = DeckManager.Instance.ValidateDeck(deck.DeckId);
                deckInfoText.text = valid
                    ? $"{deck.TotalCards} cards  ·  Ready"
                    : $"{deck.TotalCards} cards  ·  Needs more cards";
                if (playButton != null) playButton.interactable = valid;
            }

            // Prev / Next interactability
            if (prevButton != null) prevButton.interactable = _deckIndex > 0;
            if (nextButton != null) nextButton.interactable = _deckIndex < decks.Count - 1;
        }

        private void SetNavButtonsActive(bool active)
        {
            if (prevButton != null) prevButton.gameObject.SetActive(active);
            if (nextButton != null) nextButton.gameObject.SetActive(active);
        }

        // ── Button Handlers ───────────────────────────────────────────────────────

        private void OnPrevClicked() { _deckIndex--; Refresh(); }
        private void OnNextClicked() { _deckIndex++; Refresh(); }

        private void OnPlayClicked()
        {
            var decks = DeckManager.Instance?.Decks;
            if (decks == null || decks.Count == 0) return;

            _deckIndex = Mathf.Clamp(_deckIndex, 0, decks.Count - 1);
            var playerDeck = decks[_deckIndex];

            if (localController == null || aiController == null)
            {
                Debug.LogWarning("[QuickMatchPanelUI] Controllers not assigned.");
                return;
            }

            var aiDeck = BuildAiDeck();
            if (aiDeck == null)
            {
                Debug.LogWarning("[QuickMatchPanelUI] Could not build an AI deck. " +
                                 "Assign defaultAiDeck entries in the Inspector.");
                return;
            }

            // Show Match panel first so MatchUI can subscribe to OnMatchStarted
            PanelNavigator.Instance?.Show(PanelNavigator.MatchKey);
            MatchManager.Instance.StartMatch(playerDeck, aiDeck, localController, aiController);
        }

        // ── AI Deck Building ──────────────────────────────────────────────────────

        private DeckData BuildAiDeck()
        {
            // Prefer the inspector-configured default deck
            if (defaultAiDeck != null && defaultAiDeck.Length > 0)
                return BuildFromEntries("AI Opponent", defaultAiDeck);

            // Fall back to the first ranked-season AI opponent
            var season = TCG.Ranked.RankedManager.Instance?.CurrentSeason;
            if (season?.aiOpponents != null && season.aiOpponents.Length > 0)
            {
                var opp = season.aiOpponents[0];
                return BuildFromEntries(opp.opponentName, opp.deckEntries);
            }

            return null;
        }

        private static DeckData BuildFromEntries(string deckName, AiDeckEntry[] entries)
        {
            var deck = new DeckData(deckName);
            if (entries == null) return deck;

            foreach (var entry in entries)
            {
                if (entry?.card == null) continue;
                for (int i = 0; i < entry.count; i++)
                    deck.TryAddCard(entry.card);
            }

            return deck;
        }
    }
}
