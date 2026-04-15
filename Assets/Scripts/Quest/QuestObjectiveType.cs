namespace TCG.Quest
{
    /// <summary>
    /// Defines what player action advances a quest objective.
    /// The QuestTracker listens to GameEvents and maps them to these types.
    /// </summary>
    public enum QuestObjectiveType
    {
        // ── Match ────────────────────────────────────────────────────────────
        WinMatch,           // Win N matches
        PlayMatch,          // Play N matches (win or loss)
        WinMatchWithElement,// Win N matches with a specific card element as majority
        WinMatchWithClass,  // Win N matches using a specific card class

        // ── Cards ────────────────────────────────────────────────────────────
        PlayCard,           // Play N cards during matches
        CollectUniqueCard,  // Own N distinct card types
        CollectTotalCards,  // Own N card copies in total
        OpenPack,           // Open N card packs

        // ── Shop ─────────────────────────────────────────────────────────────
        PurchaseItem,       // Buy N items from the shop
        SpendGold,          // Spend N Gold
        SpendGems,          // Spend N Gems
        SpendShards,        // Spend N Shards

        // ── Deck Builder ─────────────────────────────────────────────────────
        CreateDeck,         // Create N decks
        ReachDeckSize,      // Have at least one deck with N cards

        // ── Login / Misc ──────────────────────────────────────────────────────
        LoginDay,           // Log in N days (cumulative, not consecutive)
        EarnGold            // Earn (receive) N Gold
    }
}
