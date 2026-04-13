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

        // Quests
        public QuestsSaveData quests = new();

        // Shop
        public ShopSaveData shop = new();
    }

    // ─── Inventory ─────────────────────────────────────────────────────────────

    [Serializable]
    public class InventoryItemSaveData
    {
        public string itemId;
        public int    quantity;
        public long   acquiredTicks;
    }

    // ─── Deck ──────────────────────────────────────────────────────────────────

    [Serializable]
    public class DeckSaveData
    {
        public string              deckId;
        public string              deckName;
        public long                createdTicks;
        public long                lastModifiedTicks;
        public List<DeckCardEntry> cards = new();
    }

    [Serializable]
    public class DeckCardEntry
    {
        public string cardId;
        public int    count;
    }

    // ─── Quest ─────────────────────────────────────────────────────────────────

    [Serializable]
    public class QuestsSaveData
    {
        public long                       nextDailyRefreshTicks;
        public long                       nextWeeklyRefreshTicks;
        public List<QuestProgressSaveData> entries = new();
    }

    [Serializable]
    public class QuestProgressSaveData
    {
        public string     questId;
        public int        status;           // QuestStatus cast to int
        public long       activatedTicks;
        public List<int>  objectiveCounts = new();
    }

    // ─── Shop ──────────────────────────────────────────────────────────────────

    [Serializable]
    public class ShopSaveData
    {
        public long                      lastRefreshTicks;
        public List<string>              purchasedListingIds    = new();
        public List<ShopListingSaveData> featuredListings       = new();
        public List<ShopListingSaveData> dailyListings          = new();
        public List<ShopListingSaveData> permanentListingStates = new();
    }

    [Serializable]
    public class ShopListingSaveData
    {
        public string listingId;
        public int    remainingStock;
        public bool   isPurchased;
    }
}
