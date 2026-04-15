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

        // Match History
        public MatchHistorySaveData matchHistory = new();

        // Shop
        public ShopSaveData shop = new();

        // Campaign
        public CampaignSaveData campaign = new();

        // Gacha pity counters (one entry per pool)
        public List<GachaPitySaveData> gachaPity = new();

        // Characters
        public List<string> unlockedCharacterIds = new();

        // Ranked
        public RankedSaveData ranked = new();
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

    // ─── Match History ─────────────────────────────────────────────────────────

    [Serializable]
    public class MatchHistorySaveData
    {
        public List<MatchRecordEntry> entries = new();
    }

    [Serializable]
    public class MatchRecordEntry
    {
        public long dateTicks;
        public int  result;      // MatchResult cast to int
        public int  turnsPlayed;
        public int  goldEarned;
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

    // ─── Campaign ──────────────────────────────────────────────────────────────

    [Serializable]
    public class CampaignSaveData
    {
        public List<StageSaveEntry> stages = new();
    }

    [Serializable]
    public class StageSaveEntry
    {
        public string stageId;
        /// <summary>Stars earned on this stage (0–3).</summary>
        public int    starsEarned;
        /// <summary>True once the stage has been unlocked (prerequisites met).</summary>
        public bool   isUnlocked;
    }

    // ─── Gacha Pity ────────────────────────────────────────────────────────────

    [Serializable]
    public class GachaPitySaveData
    {
        public string poolId;
        /// <summary>Consecutive pulls since the last Rare-or-better card.</summary>
        public int    pullsSinceRare;
        /// <summary>Consecutive pulls since the last Epic-or-better card.</summary>
        public int    pullsSinceEpic;
    }

    // ─── Ranked ────────────────────────────────────────────────────────────────

    [Serializable]
    public class RankedSaveData
    {
        /// <summary>(int)RankTier — starts at Bronze (0).</summary>
        public int  tier              = 0;
        /// <summary>(int)RankDivision — starts at DivIII (0).</summary>
        public int  division          = 0;
        /// <summary>RP within the current division (0–99; uncapped for Master).</summary>
        public int  rp                = 0;

        /// <summary>Highest tier reached this season (int cast of RankTier).</summary>
        public int  peakTier          = 0;
        /// <summary>Division at peak tier.</summary>
        public int  peakDivision      = 0;

        public int  wins              = 0;
        public int  losses            = 0;
        public int  draws             = 0;

        /// <summary>
        /// When true, the next loss at 0 RP is forgiven instead of causing demotion.
        /// Granted automatically on each promotion.
        /// </summary>
        public bool hasDemotionShield = false;

        /// <summary>SeasonId of the season this data belongs to.</summary>
        public string currentSeasonId = string.Empty;
    }
}
