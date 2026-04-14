using TCG.Items;

namespace TCG.Campaign
{
    /// <summary>
    /// Passed through GameEvents.OnCampaignStageCompleted after a campaign match ends.
    /// Contains everything the result UI needs without re-querying managers.
    /// </summary>
    public struct CampaignStageResult
    {
        /// <summary>Id of the stage that was played.</summary>
        public string      StageId;

        /// <summary>Total stars earned on this stage now (after this run).</summary>
        public int         StarsEarned;

        /// <summary>Stars the stage had before this run.</summary>
        public int         PreviousStars;

        /// <summary>Cards newly unlocked as star rewards during this run (may be empty).</summary>
        public CardData[]  NewCardRewards;

        /// <summary>Gemstones awarded for reaching 3 stars for the first time (0 if not applicable).</summary>
        public int         GemsEarned;

        /// <summary>True if all 3 stars were completed for the first time in this run.</summary>
        public bool        FullStarBonusEarned;

        /// <summary>Whether the player won (false on loss/draw — stars are only awarded on win).</summary>
        public bool        MatchWon;
    }
}
