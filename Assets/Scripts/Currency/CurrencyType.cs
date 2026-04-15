namespace TCG.Currency
{
    /// <summary>
    /// All spendable currencies in the game.
    /// Gold   - earned through gameplay and quests.
    /// Gems   - premium currency (purchased or earned rarely).
    /// Shards - crafting currency obtained by dismantling cards.
    /// </summary>
    public enum CurrencyType
    {
        Gold   = 0,
        Gems   = 1,
        Shards = 2
    }
}
