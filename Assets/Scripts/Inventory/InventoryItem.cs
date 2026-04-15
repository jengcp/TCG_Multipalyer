using System;
using UnityEngine;
using TCG.Items;

namespace TCG.Inventory
{
    /// <summary>
    /// Represents a stack of one ItemData in the player's inventory.
    /// Serialized to/from save data via <see cref="InventoryItemSaveData"/>.
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public ItemData itemData;
        public int quantity;

        public InventoryItem(ItemData data, int qty = 1)
        {
            itemData = data;
            quantity = qty;
        }

        public bool IsValid() => itemData != null && itemData.IsValid() && quantity > 0;

        /// <summary>Returns true if more of this item can be added to this stack.</summary>
        public bool CanStack(int additionalQty = 1)
        {
            if (!itemData.isStackable) return quantity == 0;
            return quantity + additionalQty <= itemData.maxStack;
        }

        public override string ToString() => $"{itemData?.displayName} x{quantity}";
    }
}
