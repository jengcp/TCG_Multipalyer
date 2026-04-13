using System;
using System.Collections.Generic;

namespace TCG.Save
{
    /// <summary>Root save data object — serialized to disk as JSON.</summary>
    [Serializable]
    public class PlayerSaveData
    {
        // Currency
        public int gold;
        public int gems;
        public int shards;

        // Inventory
        public List<InventoryItemSaveData> inventoryItems = new();

        // Decks
        public List<DeckSaveData> decks = new();

        // Shop
        public ShopSaveData shop = new();
    }

    [Serializable]
    public class InventoryItemSaveData
    {
        public string itemId;
        public int    quantity;
        public long   acquiredTicks; // DateTime.UtcNow.Ticks when first obtained
    }

    // ─── Deck ──────────────────────────────────────────────────────────────────

    [Serializable]
    public class DeckSaveData
    {
        public string            deckId;
        public string            deckName;
        public long              createdTicks;
        public long              lastModifiedTicks;
        public List<DeckCardEntry> cards = new();
    }

    [Serializable]
    public class DeckCardEntry
    {
        public string cardId;
        public int    count;
    }

    // ─── Shop ──────────────────────────────────────────────────────────────────

    [Serializable]
    public class ShopSaveData
    {
        public long                      lastRefreshTicks;
        public List<string>              purchasedListingIds      = new();
        public List<ShopListingSaveData> featuredListings         = new();
        public List<ShopListingSaveData> dailyListings            = new();
        public List<ShopListingSaveData> permanentListingStates   = new();
    }

    [Serializable]
    public class ShopListingSaveData
    {
        public string listingId;
        public int    remainingStock; // -1 = unlimited
        public bool   isPurchased;
    }
}
