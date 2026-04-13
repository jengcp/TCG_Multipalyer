using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TCG.Core;
using TCG.Items;
using TCG.Save;

namespace TCG.Inventory.Deck
{
    /// <summary>
    /// Singleton MonoBehaviour that manages all of the player's decks.
    /// Handles create / rename / delete / validation and persistence.
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        public static DeckManager Instance { get; private set; }

        [Header("Rules")]
        [SerializeField] private int minDeckSize = 20;
        [SerializeField] private int maxDeckSize = 60;
        [SerializeField] private int maxDecks    = 10;

        private readonly List<DeckData> _decks = new();
        private DeckValidator _validator;

        public IReadOnlyList<DeckData> Decks => _decks.AsReadOnly();

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _validator = new DeckValidator(minDeckSize, maxDeckSize);
        }

        private void Start() => LoadDecks();

        // ─── Public API ────────────────────────────────────────────────────────

        public DeckData GetDeck(string deckId)
            => _decks.FirstOrDefault(d => d.DeckId == deckId);

        /// <summary>Creates a new empty deck. Returns null if the deck limit is reached.</summary>
        public DeckData CreateDeck(string name = "New Deck")
        {
            if (_decks.Count >= maxDecks)
            {
                Debug.LogWarning($"[DeckManager] Deck limit ({maxDecks}) reached.");
                return null;
            }

            var deck = new DeckData(name);
            _decks.Add(deck);
            GameEvents.RaiseDeckChanged(deck);
            SaveDecks();
            return deck;
        }

        public bool RenameDeck(string deckId, string newName)
        {
            var deck = GetDeck(deckId);
            if (deck == null) return false;
            deck.Rename(newName);
            GameEvents.RaiseDeckChanged(deck);
            SaveDecks();
            return true;
        }

        public bool DeleteDeck(string deckId)
        {
            var deck = GetDeck(deckId);
            if (deck == null) return false;
            _decks.Remove(deck);
            GameEvents.RaiseDeckDeleted(deckId);
            SaveDecks();
            return true;
        }

        /// <summary>Adds one copy of <paramref name="card"/> to <paramref name="deckId"/>.</summary>
        public bool TryAddCardToDeck(string deckId, CardData card)
        {
            var deck = GetDeck(deckId);
            if (deck == null || card == null) return false;

            // Ensure the player actually owns a copy they aren't already using in this deck
            int owned    = PlayerInventory.Instance?.GetQuantity(card.itemId) ?? 0;
            int inDeck   = deck.GetCount(card.itemId);
            if (inDeck >= owned)
            {
                Debug.LogWarning($"[DeckManager] No spare copies of '{card.displayName}' to add.");
                return false;
            }

            bool added = deck.TryAddCard(card);
            if (added)
            {
                GameEvents.RaiseDeckChanged(deck);
                SaveDecks();
            }
            return added;
        }

        /// <summary>Removes one copy of a card from the deck.</summary>
        public bool TryRemoveCardFromDeck(string deckId, string cardId)
        {
            var deck = GetDeck(deckId);
            if (deck == null) return false;

            bool removed = deck.TryRemoveCard(cardId);
            if (removed)
            {
                GameEvents.RaiseDeckChanged(deck);
                SaveDecks();
            }
            return removed;
        }

        /// <summary>Validates a deck. Returns the full validation report.</summary>
        public DeckValidationReport ValidateDeck(string deckId)
        {
            var deck = GetDeck(deckId);
            if (deck == null)
                return new DeckValidationReport(DeckValidationResult.TooFewCards, "Deck not found.");

            return _validator.Validate(deck, PlayerInventory.Instance);
        }

        // ─── Persistence ───────────────────────────────────────────────────────

        private void LoadDecks()
        {
            var save = SaveSystem.Load();
            if (save?.decks == null) return;

            foreach (var ds in save.decks)
            {
                var deck = new DeckData(ds.deckName, ds.deckId);

                foreach (var entry in ds.cards)
                {
                    var card = Resources.Load<CardData>($"Items/{entry.cardId}");
                    if (card == null)
                    {
                        Debug.LogWarning($"[DeckManager] Card asset not found: '{entry.cardId}'.");
                        continue;
                    }
                    for (int i = 0; i < entry.count; i++)
                        deck.TryAddCard(card);
                }

                _decks.Add(deck);
            }
        }

        private void SaveDecks()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.decks = _decks.Select(d => new DeckSaveData
            {
                deckId            = d.DeckId,
                deckName          = d.DeckName,
                createdTicks      = d.CreatedAt.Ticks,
                lastModifiedTicks = d.LastModified.Ticks,
                cards             = d.Slots.Select(s => new DeckCardEntry
                {
                    cardId = s.Card.itemId,
                    count  = s.Count
                }).ToList()
            }).ToList();
            SaveSystem.Save(save);
        }
    }
}
