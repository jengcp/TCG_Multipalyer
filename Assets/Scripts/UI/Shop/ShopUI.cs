using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Shop;

namespace TCG.UI.Shop
{
    /// <summary>
    /// Main shop UI controller.
    /// Manages the tab bar, item grid, and delegates purchases to ShopManager.
    /// Assign the ShopItemSlot prefab (with ShopItemSlotUI component) in the Inspector.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private Transform      itemGrid;
        [SerializeField] private GameObject     shopItemSlotPrefab;
        [SerializeField] private ScrollRect     scrollRect;

        [Header("Category Tabs")]
        [SerializeField] private Transform      tabBar;
        [SerializeField] private GameObject     tabButtonPrefab;

        [Header("Confirmation Dialog")]
        [SerializeField] private PurchaseConfirmUI confirmDialog;
        [SerializeField] private bool useConfirmDialog = true;

        [Header("Feedback")]
        [SerializeField] private GameObject     purchaseFeedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private float          feedbackDuration = 2f;

        private readonly List<ShopItemSlotUI> _slotPool   = new();
        private readonly List<Button>         _tabButtons = new();
        private ShopCategory                  _activeCategory = ShopCategory.Daily;
        private float                         _feedbackTimer;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnShopRefreshed    += OnShopRefreshed;
            GameEvents.OnItemPurchased    += OnItemPurchased;
            PopulateShop();
        }

        private void OnDisable()
        {
            GameEvents.OnShopRefreshed -= OnShopRefreshed;
            GameEvents.OnItemPurchased -= OnItemPurchased;
        }

        private void Update()
        {
            if (_feedbackTimer > 0f)
            {
                _feedbackTimer -= Time.deltaTime;
                if (_feedbackTimer <= 0f && purchaseFeedbackPanel != null)
                    purchaseFeedbackPanel.SetActive(false);
            }
        }

        // ─── Public API ────────────────────────────────────────────────────────

        public void SelectCategory(ShopCategory category)
        {
            _activeCategory = category;
            GameEvents.RaiseShopCategoryChanged(category.ToString());
            PopulateGrid(category);
            RefreshTabHighlights();
        }

        // ─── Private Helpers ───────────────────────────────────────────────────

        private void PopulateShop()
        {
            BuildTabs();
            PopulateGrid(_activeCategory);
        }

        private void BuildTabs()
        {
            if (tabBar == null || tabButtonPrefab == null) return;

            // Collect distinct categories from active listings
            var categories = ShopManager.Instance?.ActiveListings
                .Select(l => l.category)
                .Distinct()
                .OrderBy(c => (int)c)
                .ToList();

            if (categories == null || categories.Count == 0) return;

            // Clear old tabs
            foreach (var btn in _tabButtons) Destroy(btn.gameObject);
            _tabButtons.Clear();

            foreach (var cat in categories)
            {
                var go  = Instantiate(tabButtonPrefab, tabBar);
                var btn = go.GetComponentInChildren<Button>();
                var lbl = go.GetComponentInChildren<TextMeshProUGUI>();

                if (lbl != null) lbl.text = cat.ToString();

                ShopCategory captured = cat;
                btn?.onClick.AddListener(() => SelectCategory(captured));

                if (btn != null) _tabButtons.Add(btn);
            }

            RefreshTabHighlights();
        }

        private void PopulateGrid(ShopCategory category)
        {
            if (itemGrid == null || shopItemSlotPrefab == null || ShopManager.Instance == null) return;

            var listings = ShopManager.Instance.ActiveListings
                .Where(l => l.category == category)
                .ToList();

            // Grow pool if needed
            while (_slotPool.Count < listings.Count)
            {
                var go   = Instantiate(shopItemSlotPrefab, itemGrid);
                var slot = go.GetComponent<ShopItemSlotUI>();
                if (slot != null) _slotPool.Add(slot);
            }

            // Populate or hide slots
            for (int i = 0; i < _slotPool.Count; i++)
            {
                if (i < listings.Count)
                {
                    _slotPool[i].gameObject.SetActive(true);
                    _slotPool[i].Populate(listings[i]);
                }
                else
                {
                    _slotPool[i].gameObject.SetActive(false);
                }
            }

            // Scroll back to top
            if (scrollRect != null)
                scrollRect.normalizedPosition = new Vector2(0f, 1f);
        }

        private void RefreshTabHighlights()
        {
            // Simple highlight: full alpha for active, half for inactive
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                var colors = _tabButtons[i].colors;
                colors.normalColor = (_tabButtons[i].GetComponentInChildren<TextMeshProUGUI>()?.text == _activeCategory.ToString())
                    ? Color.white
                    : new Color(1f, 1f, 1f, 0.5f);
                _tabButtons[i].colors = colors;
            }
        }

        private void ShowFeedback(string message)
        {
            if (purchaseFeedbackPanel == null) return;
            if (feedbackText != null) feedbackText.text = message;
            purchaseFeedbackPanel.SetActive(true);
            _feedbackTimer = feedbackDuration;
        }

        // ─── Event Handlers ────────────────────────────────────────────────────

        private void OnShopRefreshed()
        {
            BuildTabs();
            PopulateGrid(_activeCategory);
        }

        private void OnItemPurchased(ShopItemListing listing)
        {
            ShowFeedback($"Purchased {listing.item?.displayName}!");
            PopulateGrid(_activeCategory); // refresh availability states
        }

        // Called by ShopItemSlotUI when a buy button is tapped — wires the confirm dialog
        public void RequestPurchase(ShopItemListing listing)
        {
            if (listing == null) return;

            if (useConfirmDialog && confirmDialog != null)
            {
                confirmDialog.Show(listing,
                    onConfirm: () =>
                    {
                        var result = ShopManager.Instance?.TryPurchase(listing);
                        if (result != null && !result.Value.IsSuccess)
                            ShowFeedback(result.Value.Message);
                    });
            }
            else
            {
                var result = ShopManager.Instance?.TryPurchase(listing);
                if (result != null && !result.Value.IsSuccess)
                    ShowFeedback(result.Value.Message);
            }
        }
    }
}
