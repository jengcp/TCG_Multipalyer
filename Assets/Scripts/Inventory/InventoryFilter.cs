using TCG.Items;

namespace TCG.Inventory
{
    /// <summary>
    /// Immutable value-type that describes how to filter an inventory view.
    /// Pass to PlayerInventory.GetFiltered() to retrieve a matching subset.
    /// </summary>
    public class InventoryFilter
    {
        public string      SearchText   { get; }
        public ItemType?   TypeFilter   { get; }
        public ItemRarity? RarityFilter { get; }
        public bool        OnlyOwned    { get; }

        public static readonly InventoryFilter None = new InventoryFilter();

        public InventoryFilter(
            string      searchText   = "",
            ItemType?   typeFilter   = null,
            ItemRarity? rarityFilter = null,
            bool        onlyOwned    = false)
        {
            SearchText   = searchText   ?? string.Empty;
            TypeFilter   = typeFilter;
            RarityFilter = rarityFilter;
            OnlyOwned    = onlyOwned;
        }

        public bool IsEmpty =>
            string.IsNullOrEmpty(SearchText) &&
            TypeFilter   == null &&
            RarityFilter == null &&
            !OnlyOwned;

        /// <summary>Returns true if the given item passes all active filter criteria.</summary>
        public bool Matches(InventoryItem item)
        {
            if (item == null || !item.IsValid()) return false;

            if (TypeFilter.HasValue   && item.itemData.itemType != TypeFilter.Value)   return false;
            if (RarityFilter.HasValue && item.itemData.rarity   != RarityFilter.Value) return false;
            if (OnlyOwned             && item.quantity <= 0)                            return false;

            if (!string.IsNullOrEmpty(SearchText))
            {
                string lower = SearchText.ToLowerInvariant();
                bool nameMatch = item.itemData.displayName?.ToLowerInvariant().Contains(lower) ?? false;
                bool descMatch = item.itemData.description?.ToLowerInvariant().Contains(lower) ?? false;
                if (!nameMatch && !descMatch) return false;
            }

            return true;
        }
    }
}
