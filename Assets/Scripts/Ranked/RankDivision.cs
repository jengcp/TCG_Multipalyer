namespace TCG.Ranked
{
    /// <summary>
    /// Division within a rank tier.
    /// Ordered from lowest (DivIII) to highest (DivI).
    /// Master uses <see cref="None"/> — it has no divisions.
    /// </summary>
    public enum RankDivision
    {
        DivIII = 0,
        DivII  = 1,
        DivI   = 2,
        None   = 3,   // Master only
    }
}
