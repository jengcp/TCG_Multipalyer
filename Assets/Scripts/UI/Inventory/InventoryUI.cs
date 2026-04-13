using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Inventory;

namespace TCG.UI.Inventory
{
    /// <summary>
    /// Root controller for the inventory screen.
    /// Manages the grid of InventorySlotUI tiles and reacts to filter/sort changes
    /// from InventoryFilterPanelUI.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private Transform      slotGrid;
        [SerializeField] private GameObject     slotPrefab;
        [SerializeField] private int            columnCount = 6;

        [Header("Stats Bar")]
        [SerializeField] private TextMeshProUGUI uniqueItemsText;
        [SerializeField] private TextMeshProUGUI totalItemsText;

        [Header("Empty State")]
        [SerializeField] private GameObject     emptyStatePanel;
        [SerializeField] private TextMeshProUGUI emptyStateText;

        [Header("Open / Close")]
        [SerializeField] private GameObject     inventoryPanel;
        [SerializeField] private Button         openButton;
        [SerializeField] private Button         closeButton;

        [Header("Sub-Panels (auto-wired)")]
        [SerializeField] private InventoryFilterPanelUI filterPanel;
        [SerializeField] private CardDetailPanelUI      detailPanel;

        // Pool of slot UI objects
        private readonly List<InventorySlotUI> _slotPool = new();

        // Current display state
        private InventoryFilter    _activeFilter = InventoryFilter.None;
        private InventorySortOrder _activeSort   = InventorySortOrder.RarityDescending;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            openButton?.onClick.AddListener(OpenInventory);
            closeButton?.onClick.AddListener(CloseInventory);

            if (filterPanel != null)
            {
                filterPanel.OnFilterChanged += f => { _activeFilter = f; Refresh(); };
                filterPanel.OnSortChanged   += s => { _activeSort   = s; Refresh(); };
            }

            CloseInventory();
        }

        private void OnEnable()
        {
            GameEvents.OnItemAdded   += _ => Refresh();
            GameEvents.OnItemRemoved += _ => Refresh();
            GameEvents.OnInventoryOpened += OpenInventory;
            GameEvents.OnInventoryClosed += CloseInventory;
        }

        private void OnDisable()
        {
            GameEvents.OnItemAdded   -= _ => Refresh();
            GameEvents.OnItemRemoved -= _ => Refresh();
            GameEvents.OnInventoryOpened -= OpenInventory;
            GameEvents.OnInventoryClosed -= CloseInventory;
        }

        // ─── Public API ────────────────────────────────────────────────────────

        public void OpenInventory()
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(true);
            Refresh();
            GameEvents.RaiseInventoryOpened();
        }

        public void CloseInventory()
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            GameEvents.RaiseInventoryClosed();
        }

        public void Refresh()
        {
            var inventory = PlayerInventory.Instance;
            if (inventory == null) return;

            var items = inventory.GetFilteredAndSorted(_activeFilter, _activeSort);

            UpdateStatsBar(inventory);
            PopulateGrid(items);
        }

        // ─── Private Helpers ───────────────────────────────────────────────────

        private void PopulateGrid(List<InventoryItem> items)
        {
            // Grow pool
            while (_slotPool.Count < items.Count)
            {
                var go   = Instantiate(slotPrefab, slotGrid);
                var slot = go.GetComponent<InventorySlotUI>();
                if (slot != null) _slotPool.Add(slot);
            }

            // Fill or hide slots
            for (int i = 0; i < _slotPool.Count; i++)
            {
                if (i < items.Count)
                {
                    _slotPool[i].gameObject.SetActive(true);
                    _slotPool[i].Populate(items[i]);
                }
                else
                {
                    _slotPool[i].gameObject.SetActive(false);
                }
            }

            bool isEmpty = items.Count == 0;
            if (emptyStatePanel != null) emptyStatePanel.SetActive(isEmpty);
            if (emptyStateText  != null && isEmpty)
                emptyStateText.text = _activeFilter.IsEmpty
                    ? "Your inventory is empty."
                    : "No items match your search.";
        }

        private void UpdateStatsBar(PlayerInventory inventory)
        {
            if (uniqueItemsText != null)
                uniqueItemsText.text = $"Items: {inventory.TotalUniqueItems}";
            if (totalItemsText != null)
                totalItemsText.text  = $"Total: {inventory.TotalItemCount}";
        }
    }
}
