using System;
using TCG.Ranked;

namespace TCG.Narrative
{
    /// <summary>
    /// Defines the condition under which a <see cref="NarrativeEventData"/> fires.
    /// Only the fields relevant to <see cref="type"/> are evaluated — unused fields are ignored.
    /// </summary>
    [Serializable]
    public class NarrativeTrigger
    {
        /// <summary>Which game event causes this narrative event to fire.</summary>
        public NarrativeTriggerType type;

        // ── Campaign fields (OnCampaignStageCompleted / OnCampaignStageFullyStarred) ──

        /// <summary>
        /// Stage ID that must match for the trigger to fire.
        /// Leave empty to match any stage.
        /// </summary>
        public string stageId;

        /// <summary>
        /// Minimum stars required on the completed stage.
        /// Ignored when 0.
        /// </summary>
        public int minStars;

        // ── Ranked fields (OnRankedPromotion) ─────────────────────────────────────

        /// <summary>
        /// Minimum rank tier the player must have reached for the trigger to fire.
        /// Only evaluated for <see cref="NarrativeTriggerType.OnRankedPromotion"/>.
        /// </summary>
        public RankTier minRankTier;

        // ── Character fields (OnCharacterUnlocked) ────────────────────────────────

        /// <summary>
        /// Character ID that must be unlocked.
        /// Leave empty to fire on any character unlock.
        /// </summary>
        public string characterId;
    }
}
