using System;

namespace TCG.Campaign
{
    /// <summary>
    /// Defines the condition for earning one star on a campaign stage.
    /// Embedded directly in <see cref="CampaignStageData"/> as a serializable array.
    /// </summary>
    [Serializable]
    public class StarCriteriaData
    {
        /// <summary>What the player must accomplish.</summary>
        public StarCriteriaType type;

        /// <summary>
        /// Numeric threshold whose meaning depends on <see cref="type"/>:
        /// <list type="bullet">
        ///   <item><b>WinWithinTurns</b> — maximum turn number allowed (inclusive).</item>
        ///   <item><b>KeepHealthAbove</b> — minimum remaining HP (inclusive).</item>
        ///   <item>Other types — not used.</item>
        /// </list>
        /// </summary>
        public int threshold;

        /// <summary>Human-readable description shown in the stage detail panel.</summary>
        public string description;
    }
}
