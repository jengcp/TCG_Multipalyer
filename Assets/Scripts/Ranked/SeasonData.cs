using System;
using UnityEngine;
using TCG.Campaign;

namespace TCG.Ranked
{
    /// <summary>
    /// Designer-authored data for a single ranked season.
    /// Create via: Assets → Create → TCG → Ranked → Season.
    /// </summary>
    [CreateAssetMenu(menuName = "TCG/Ranked/Season", fileName = "Season_01")]
    public class SeasonData : ScriptableObject
    {
        [Header("Season Info")]
        public string seasonId         = "season_01";
        public string seasonName       = "Season 1";
        public string startDateDisplay = "Jan 1 – Mar 31";
        public string endDateDisplay   = "Mar 31";

        [Header("AI Opponents")]
        [Tooltip("Pool of AI opponents used for ranked matches. RankedManager picks the one closest to the player's rank.")]
        public RankedAiOpponent[] aiOpponents = Array.Empty<RankedAiOpponent>();

        [Header("Leaderboard Fillers")]
        [Tooltip("Simulated players that populate the leaderboard alongside the local player.")]
        public SimulatedPlayerEntry[] leaderboardEntries = Array.Empty<SimulatedPlayerEntry>();

        [Header("Season-End Rewards")]
        [Tooltip("Rewards granted when the season ends. Each entry defines a minimum rank threshold; " +
                 "the player receives the reward for the highest threshold they meet.")]
        public SeasonRewardEntry[] seasonRewards = Array.Empty<SeasonRewardEntry>();
    }

    // ─── Nested Serializable Types ────────────────────────────────────────────

    /// <summary>An AI opponent used during a ranked match.</summary>
    [Serializable]
    public class RankedAiOpponent
    {
        public string          opponentName    = "Rival";
        public RankTier        approximateRank = RankTier.Bronze;
        [Tooltip("Cards in the AI's deck. Each entry specifies a card and how many copies to include.")]
        public AiDeckEntry[]   deckEntries     = Array.Empty<AiDeckEntry>();
    }

    /// <summary>A simulated player entry shown on the leaderboard.</summary>
    [Serializable]
    public class SimulatedPlayerEntry
    {
        public string       playerName = "Player";
        public RankTier     tier       = RankTier.Bronze;
        public RankDivision division   = RankDivision.DivIII;
        [Range(0, 99)]
        public int          rp         = 0;
        public int          wins       = 0;
        public int          losses     = 0;
    }

    /// <summary>Gold + gem reward granted to the player at season end if they reached <see cref="minimumRank"/>.</summary>
    [Serializable]
    public class SeasonRewardEntry
    {
        [Tooltip("Player must reach at least this tier to receive this reward.")]
        public RankTier minimumRank = RankTier.Bronze;
        public int      goldReward  = 100;
        public int      gemReward   = 0;
    }
}
