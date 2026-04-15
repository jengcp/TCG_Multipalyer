namespace TCG.Quest
{
    public enum QuestStatus
    {
        Locked,     // Prerequisites not met
        Active,     // In progress
        Completed,  // All objectives done — reward ready to claim
        Claimed,    // Reward already claimed
        Expired,    // Time ran out before completion
        Failed      // Explicitly failed (not used for most quests, reserved for timed challenge quests)
    }
}
