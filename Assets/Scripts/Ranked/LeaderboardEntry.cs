namespace TCG.Ranked
{
    /// <summary>
    /// A single row on the ranked leaderboard.
    /// Produced by <see cref="RankedManager.GetLeaderboard"/> and consumed by leaderboard UI.
    /// </summary>
    public class LeaderboardEntry
    {
        public string PlayerName;
        public RankTier Tier;
        public RankDivision Division;
        public int RP;
        public int Wins;
        public int Losses;
        /// <summary>True for the local human player's row; used by UI for highlighting.</summary>
        public bool IsLocalPlayer;

        /// <summary>
        /// Sortable score combining tier, division, and current RP.
        /// Higher is better. Formula: tier×300 + division×100 + RP.
        /// Master (tier=5, division=None) = 1500 + RP.
        /// </summary>
        public int AbsoluteRP =>
            (int)Tier * 300 +
            (Division == RankDivision.None ? 0 : (int)Division * 100) +
            RP;
    }
}
