using System.Collections.Generic;
using TCG.Items;

namespace TCG.Inventory
{
    public interface IInventory
    {
        // ── Data access ─────────────────────────────────────────────────────────
        IReadOnlyList<InventoryItem> Items { get; }

        bool HasItem(string itemId, int quantity = 1);
        int  GetQuantity(string itemId);
        InventoryItem GetItem(string itemId);

        // ── Mutation ────────────────────────────────────────────────────────────
        bool TryAddItem(ItemData item, int quantity = 1);
        bool TryRemoveItem(string itemId, int quantity = 1);
        bool TryMoveItem(string itemId, int quantity, IInventory destination);
        void Clear();

        // ── Query ───────────────────────────────────────────────────────────────
        List<InventoryItem> GetFiltered(InventoryFilter filter);
        List<InventoryItem> GetSorted(InventorySortOrder order);
        List<InventoryItem> GetByType(ItemType type);
        List<InventoryItem> GetByRarity(ItemRarity rarity);
        int TotalUniqueItems { get; }
        int TotalItemCount   { get; }
    }
}
