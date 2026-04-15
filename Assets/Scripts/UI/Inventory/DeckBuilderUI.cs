using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Inventory;
using TCG.Inventory.Deck;
using TCG.Items;

namespace TCG.UI.Inventory
{
    /// <summary>
    /// Side-by-side deck builder:
    ///   Left  — card collection (filtered from PlayerInventory)
    ///   Right — current deck slot list + validation bar
    ///
    /// Requires DeckManager and PlayerInventory singletons to be present.
    /// </summary>
    public class DeckBuilderUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button     openButton;
        [SerializeField] private Button     closeButton;

        // ── Collection (left) ────────────────────────────────────────────────
        [Header("Collection Grid")]
        [SerializeField] private Transform  collectionGrid;
        [SerializeField] private GameObject collectionSlotPrefab; // has InventorySlotUI

        // ── Deck List (right) ────────────────────────────────────────────────
        [Header("Deck List")]
        [SerializeField] private Transform  deckList;
        [SerializeField] private GameObject deckSlotPrefab; // has DeckSlotUI
        [SerializeField] private TextMeshProUGUI deckNameText;
        [SerializeField] private TMP_InputField  deckNameInput;
        [SerializeField] private TextMeshProUGUI cardCountText;
        [SerializeField] private TextMeshProUGUI validationText;
        [SerializeField] private Image           validationIcon;
        [SerializeField] private Sprite          validSprite;
        [SerializeField] private Sprite          invalidSprite;

        // ── Deck Selection ───────────────────────────────────────────────────
        [Header("Deck Management Buttons")]
        [SerializeField] private Button     newDeckButton;
        [SerializeField] private Button     deleteDeckButton;
        [SerializeField] private Button     renameDeckButton;
        [SerializeField] private Transform  deckTabBar;
        [SerializeField] private GameObject deckTabPrefab;

        // ── Runtime State ────────────────────────────────────────────────────
        private DeckData _activeDeck;
        private readonly List<InventorySlotUI> _collectionPool = new();
        private readonly List<DeckSlotUI>      _deckSlotPool   = new();
        private readonly List<Button>          _deckTabs       = new();

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            openButton?.onClick.AddListener(Open);
            closeButton?.onClick.AddListener(Close);
            newDeckButton?.onClick.AddListener(OnNewDeck);
            deleteDeckButton?.onClick.AddListener(OnDeleteDeck);
            renameDeckButton?.onClick.AddListener(OnRenameDeck);
            deckNameInput?.onEndEdit.AddListener(OnDeckNameEndEdit);

            Close();
        }

        private void OnEnable()
        {
            GameEvents.OnDeckChanged += _ => RefreshDeckView();
            GameEvents.OnDeckDeleted += _ => { RefreshDeckTabs(); RefreshDeckView(); };
            GameEvents.OnItemAdded   += _ => RefreshCollection();
            GameEvents.OnItemRemoved += _ => RefreshCollection();
        }

        private void OnDisable()
        {
            GameEvents.OnDeckChanged -= _ => RefreshDeckView();
            GameEvents.OnDeckDeleted -= _ => { RefreshDeckTabs(); RefreshDeckView(); };
            GameEvents.OnItemAdded   -= _ => RefreshCollection();
            GameEvents.OnItemRemoved -= _ => RefreshCollection();
        }

        // ─── Open / Close ──────────────────────────────────────────────────────

        public void Open()
        {
            if (panel != null) panel.SetActive(true);
            RefreshDeckTabs();
            RefreshCollection();
            RefreshDeckView();
        }

        public void Close()
        {
            if (panel != null) panel.SetActive(false);
        }

        // ─── Collection (left panel) ───────────────────────────────────────────

        private void RefreshCollection()
        {
            var inventory = PlayerInventory.Instance;
            if (inventory == null || collectionGrid == null) return;

            var cards = inventory.GetFiltered(new InventoryFilter(typeFilter: ItemType.Card));

            // Grow pool
            while (_collectionPool.Count < cards.Count)
            {
                var go   = Instantiate(collectionSlotPrefab, collectionGrid);
                var slot = go.GetComponent<InventorySlotUI>();
                if (slot != null)
                {
                    _collectionPool.Add(slot);
                    // Wire click → add to active deck
                    slot.GetComponent<Button>()?.onClick.AddListener(() =>
                        OnCollectionSlotClicked(slot));
                }
            }

            for (int i = 0; i < _collectionPool.Count; i++)
            {
                if (i < cards.Count)
                {
                    _collectionPool[i].gameObject.SetActive(true);
                    _collectionPool[i].Populate(cards[i]);
                }
                else
                {
                    _collectionPool[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnCollectionSlotClicked(InventorySlotUI slot)
        {
            if (_activeDeck == null || slot.Item == null) return;
            if (slot.Item.itemData is not CardData card) return;

            DeckManager.Instance?.TryAddCardToDeck(_activeDeck.DeckId, card);
        }

        // ─── Deck View (right panel) ───────────────────────────────────────────

        private void RefreshDeckView()
        {
            if (_activeDeck == null)
            {
                // Pick first available deck automatically
                if (DeckManager.Instance != null && DeckManager.Instance.Decks.Count > 0)
                    _activeDeck = DeckManager.Instance.Decks[0];
                else
                {
                    ClearDeckList();
                    return;
                }
            }

            if (deckNameText  != null) deckNameText.text  = _activeDeck.DeckName;
            if (deckNameInput != null) deckNameInput.SetTextWithoutNotify(_activeDeck.DeckName);
            if (cardCountText != null) cardCountText.text = $"{_activeDeck.TotalCards} cards";

            PopulateDeckList();
            RefreshValidationBar();
        }

        private void PopulateDeckList()
        {
            if (deckList == null || deckSlotPrefab == null || _activeDeck == null) return;

            var slots = _activeDeck.Slots;

            while (_deckSlotPool.Count < slots.Count)
            {
                var go  = Instantiate(deckSlotPrefab, deckList);
                var dsu = go.GetComponent<DeckSlotUI>();
                if (dsu != null)
                {
                    _deckSlotPool.Add(dsu);
                    dsu.OnAddClicked    += id => DeckManager.Instance?.TryAddCardToDeck(
                                                    _activeDeck.DeckId,
                                                    PlayerInventory.Instance?.GetItem(id)?.itemData as CardData);
                    dsu.OnRemoveClicked += id => DeckManager.Instance?.TryRemoveCardFromDeck(
                                                    _activeDeck.DeckId, id);
                }
            }

            for (int i = 0; i < _deckSlotPool.Count; i++)
            {
                if (i < slots.Count)
                {
                    _deckSlotPool[i].gameObject.SetActive(true);
                    _deckSlotPool[i].Populate(slots[i]);
                }
                else
                {
                    _deckSlotPool[i].gameObject.SetActive(false);
                }
            }
        }

        private void ClearDeckList()
        {
            foreach (var ds in _deckSlotPool) ds.gameObject.SetActive(false);
            if (cardCountText  != null) cardCountText.text  = "0 cards";
            if (validationText != null) validationText.text = "No deck selected.";
        }

        private void RefreshValidationBar()
        {
            if (_activeDeck == null) return;

            var report = DeckManager.Instance?.ValidateDeck(_activeDeck.DeckId);
            if (report == null) return;

            if (validationText != null) validationText.text = report.Message;
            if (validationIcon != null)
                validationIcon.sprite = report.IsValid ? validSprite : invalidSprite;
        }

        // ─── Deck Tabs ─────────────────────────────────────────────────────────

        private void RefreshDeckTabs()
        {
            if (deckTabBar == null || deckTabPrefab == null) return;

            foreach (var btn in _deckTabs) Destroy(btn.gameObject);
            _deckTabs.Clear();

            if (DeckManager.Instance == null) return;

            foreach (var deck in DeckManager.Instance.Decks)
            {
                var go  = Instantiate(deckTabPrefab, deckTabBar);
                var btn = go.GetComponentInChildren<Button>();
                var lbl = go.GetComponentInChildren<TextMeshProUGUI>();

                if (lbl != null) lbl.text = deck.DeckName;

                DeckData captured = deck;
                btn?.onClick.AddListener(() =>
                {
                    _activeDeck = captured;
                    RefreshDeckView();
                });

                if (btn != null) _deckTabs.Add(btn);
            }
        }

        // ─── Deck Management Buttons ───────────────────────────────────────────

        private void OnNewDeck()
        {
            var deck = DeckManager.Instance?.CreateDeck("New Deck");
            if (deck != null)
            {
                _activeDeck = deck;
                RefreshDeckTabs();
                RefreshDeckView();
            }
        }

        private void OnDeleteDeck()
        {
            if (_activeDeck == null) return;
            DeckManager.Instance?.DeleteDeck(_activeDeck.DeckId);
            _activeDeck = null;
            RefreshDeckTabs();
            RefreshDeckView();
        }

        private void OnRenameDeck()
        {
            if (deckNameInput != null)
                deckNameInput.gameObject.SetActive(true);
        }

        private void OnDeckNameEndEdit(string newName)
        {
            if (_activeDeck == null || string.IsNullOrWhiteSpace(newName)) return;
            DeckManager.Instance?.RenameDeck(_activeDeck.DeckId, newName);
            if (deckNameInput != null) deckNameInput.gameObject.SetActive(false);
            RefreshDeckTabs();
        }
    }
}
