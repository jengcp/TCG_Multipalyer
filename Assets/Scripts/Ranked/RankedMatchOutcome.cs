using TCG.Match;

namespace TCG.Ranked
{
    /// <summary>
    /// All information produced by resolving a ranked match:
    /// RP delta, resulting rank, and whether the player was promoted or demoted.
    /// Passed through <see cref="TCG.Core.GameEvents.OnRankedMatchResolved"/> so every
    /// subscriber gets a complete snapshot without additional queries.
    /// </summary>
    public struct RankedMatchOutcome
    {
        /// <summary>Change in RP this match (positive = gained, negative = lost).</summary>
        public int RpDelta;

        /// <summary>Player's RP within their current division after applying the delta.</summary>
        public int NewRP;

        /// <summary>Tier after applying promotion / demotion.</summary>
        public RankTier NewTier;

        /// <summary>Division after applying promotion / demotion.</summary>
        public RankDivision NewDivision;

        /// <summary>True when the player moved up a division or tier this match.</summary>
        public bool Promoted;

        /// <summary>True when the player moved down a division or tier this match.</summary>
        public bool Demoted;

        /// <summary>Raw outcome of the underlying match.</summary>
        public MatchResult MatchResult;
    }
}
