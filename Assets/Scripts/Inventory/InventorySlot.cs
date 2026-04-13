using System;
using TCG.Items;

namespace TCG.Inventory
{
    /// <summary>
    /// Represents one slot in a grid-based inventory view.
    /// A slot can be empty (Item == null) or occupied by an InventoryItem.
    /// SlotIndex is the position in the display grid.
    /// </summary>
    [Serializable]
    public class InventorySlot
    {
        public int           SlotIndex { get; }
        public InventoryItem Item      { get; private set; }
        public bool          IsEmpty   => Item == null;
        public bool          IsLocked  { get; private set; }

        public InventorySlot(int index, InventoryItem item = null)
        {
            SlotIndex = index;
            Item      = item;
        }

        /// <summary>Assigns an item to this slot. Returns false if the slot is locked.</summary>
        public bool Set(InventoryItem item)
        {
            if (IsLocked) return false;
            Item = item;
            return true;
        }

        /// <summary>Clears the slot without touching inventory data.</summary>
        public void Clear()
        {
            if (!IsLocked) Item = null;
        }

        public void Lock()   => IsLocked = true;
        public void Unlock() => IsLocked = false;

        public override string ToString() =>
            IsEmpty ? $"[Slot {SlotIndex}] Empty" : $"[Slot {SlotIndex}] {Item}";
    }
}
