namespace TCG.Narrative
{
    /// <summary>
    /// Defines what in-game event causes a <see cref="NarrativeEventData"/> to fire.
    /// </summary>
    public enum NarrativeTriggerType
    {
        /// <summary>Fires once on first game launch (or after save is reset).</summary>
        OnGameStart              = 0,

        /// <summary>Fires when any campaign stage (or a specific one) is completed with the required stars.</summary>
        OnCampaignStageCompleted = 1,

        /// <summary>Fires the first time all 3 stars are earned on a specific stage.</summary>
        OnCampaignStageFullyStarred = 2,

        /// <summary>Fires after winning any ranked match.</summary>
        OnRankedWin              = 3,

        /// <summary>Fires when the player is promoted to a new rank tier.</summary>
        OnRankedPromotion        = 4,

        /// <summary>Fires when a specific character is unlocked.</summary>
        OnCharacterUnlocked      = 5,

        /// <summary>Fires after any gacha pull completes.</summary>
        OnGachaPull              = 6,

        /// <summary>Fires on the daily login event.</summary>
        OnDayLogin               = 7,

        /// <summary>Never fires automatically — call <see cref="NarrativeManager.TriggerManual"/> explicitly.</summary>
        Manual                   = 8,
    }
}
