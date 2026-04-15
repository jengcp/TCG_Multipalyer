namespace TCG.Campaign
{
    /// <summary>
    /// Defines the condition a player must meet to earn one star on a campaign stage.
    /// </summary>
    public enum StarCriteriaType
    {
        /// <summary>Simply win the match. Always used for star 1.</summary>
        WinMatch,

        /// <summary>Win the match within <c>threshold</c> turns.</summary>
        WinWithinTurns,

        /// <summary>Win with local player HP >= <c>threshold</c> points remaining.</summary>
        KeepHealthAbove,

        /// <summary>Win without losing any of your creatures.</summary>
        LoseNoCreatures,
    }
}
