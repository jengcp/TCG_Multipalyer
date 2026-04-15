namespace TCG.Ranked
{
    /// <summary>
    /// The six competitive tiers on the ranked ladder, ordered from lowest to highest.
    /// Cast to int for arithmetic comparisons (e.g. Bronze = 0, Master = 5).
    /// </summary>
    public enum RankTier
    {
        Bronze   = 0,
        Silver   = 1,
        Gold     = 2,
        Platinum = 3,
        Diamond  = 4,
        Master   = 5,
    }
}
