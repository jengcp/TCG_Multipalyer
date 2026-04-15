using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Inventory;
using TCG.Items;

namespace TCG.UI.Inventory
{
    /// <summary>
    /// Side-panel that exposes filter and sort controls.
    /// Raises OnFilterChanged / OnSortChanged when the player changes a setting.
    /// Wire these callbacks to InventoryUI.ApplyFilter / ApplySort.
    /// </summary>
    public class InventoryFilterPanelUI : MonoBehaviour
    {
        public event Action<InventoryFilter>    OnFilterChanged;
        public event Action<InventorySortOrder> OnSortChanged;

        [Header("Search")]
        [SerializeField] private TMP_InputField searchInput;

        [Header("Type Filter")]
        [SerializeField] private Toggle toggleAll;
        [SerializeField] private Toggle toggleCards;
        [SerializeField] private Toggle togglePacks;
        [SerializeField] private Toggle toggleCosmetics;

        [Header("Rarity Filter")]
        [SerializeField] private Toggle toggleRarityAll;
        [SerializeField] private Toggle toggleCommon;
        [SerializeField] private Toggle toggleUncommon;
        [SerializeField] private Toggle toggleRare;
        [SerializeField] private Toggle toggleEpic;
        [SerializeField] private Toggle toggleLegendary;

        [Header("Sort")]
        [SerializeField] private TMP_Dropdown sortDropdown;

        [Header("Clear Button")]
        [SerializeField] private Button clearButton;

        private ItemType?          _activeType;
        private ItemRarity?        _activeRarity;
        private InventorySortOrder _activeSortOrder = InventorySortOrder.NameAscending;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            searchInput?.onValueChanged.AddListener(_ => EmitFilter());

            BindTypeToggle(toggleAll,       null);
            BindTypeToggle(toggleCards,     ItemType.Card);
            BindTypeToggle(togglePacks,     ItemType.CardPack);
            BindTypeToggle(toggleCosmetics, ItemType.Cosmetic);

            BindRarityToggle(toggleRarityAll,  null);
            BindRarityToggle(toggleCommon,     ItemRarity.Common);
            BindRarityToggle(toggleUncommon,   ItemRarity.Uncommon);
            BindRarityToggle(toggleRare,       ItemRarity.Rare);
            BindRarityToggle(toggleEpic,       ItemRarity.Epic);
            BindRarityToggle(toggleLegendary,  ItemRarity.Legendary);

            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(new System.Collections.Generic.List<string>(
                    Enum.GetNames(typeof(InventorySortOrder))));
                sortDropdown.onValueChanged.AddListener(idx =>
                {
                    _activeSortOrder = (InventorySortOrder)idx;
                    OnSortChanged?.Invoke(_activeSortOrder);
                });
            }

            clearButton?.onClick.AddListener(ClearAll);
        }

        // ─── Helpers ───────────────────────────────────────────────────────────

        private void BindTypeToggle(Toggle toggle, ItemType? type)
        {
            if (toggle == null) return;
            toggle.onValueChanged.AddListener(on =>
            {
                if (!on) return;
                _activeType = type;
                EmitFilter();
            });
        }

        private void BindRarityToggle(Toggle toggle, ItemRarity? rarity)
        {
            if (toggle == null) return;
            toggle.onValueChanged.AddListener(on =>
            {
                if (!on) return;
                _activeRarity = rarity;
                EmitFilter();
            });
        }

        private void EmitFilter()
        {
            var filter = new InventoryFilter(
                searchText:   searchInput?.text ?? string.Empty,
                typeFilter:   _activeType,
                rarityFilter: _activeRarity);

            OnFilterChanged?.Invoke(filter);
        }

        private void ClearAll()
        {
            if (searchInput != null) searchInput.SetTextWithoutNotify(string.Empty);
            _activeType   = null;
            _activeRarity = null;
            if (toggleAll        != null) toggleAll.isOn        = true;
            if (toggleRarityAll  != null) toggleRarityAll.isOn  = true;
            EmitFilter();
        }
    }
}
