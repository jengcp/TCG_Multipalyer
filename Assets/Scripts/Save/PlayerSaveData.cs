using System;
using System.Collections.Generic;

namespace TCG.Save
{
    /// <summary>
    /// Root save data object serialized to disk as JSON.
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        // Currency
        public int gold;
        public int gems;
        public int shards;

        // Inventory
        public List<InventoryItemSaveData> inventoryItems = new();

        // Shop
        public ShopSaveData shop = new();
    }

    [Serializable]
    public class InventoryItemSaveData
    {
        public string itemId;
        public int    quantity;
    }

    [Serializable]
    public class ShopSaveData
    {
        public long                      lastRefreshTicks;               // DateTime.Ticks
        public List<string>              purchasedListingIds  = new();   // IDs bought in current rotation
        public List<ShopListingSaveData> featuredListings     = new();
        public List<ShopListingSaveData> dailyListings        = new();
        public List<ShopListingSaveData> permanentListingStates = new(); // persisted stock for permanent listings
    }

    [Serializable]
    public class ShopListingSaveData
    {
        public string listingId;
        public int    remainingStock;   // -1 = unlimited
        public bool   isPurchased;
    }
}
