using System.Collections.Generic;
using TCG.Items;

namespace TCG.Inventory
{
    public interface IInventory
    {
        IReadOnlyList<InventoryItem> Items { get; }

        bool HasItem(string itemId, int quantity = 1);
        int  GetQuantity(string itemId);
        bool TryAddItem(ItemData item, int quantity = 1);
        bool TryRemoveItem(string itemId, int quantity = 1);
        void Clear();
    }
}
