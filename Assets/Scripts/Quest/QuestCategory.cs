namespace TCG.Quest
{
    public enum QuestCategory
    {
        Daily       = 0,  // Refresh every 24h
        Weekly      = 1,  // Refresh every 7 days
        Story       = 2,  // One-time narrative quests
        Achievement = 3   // Permanent milestones, never expire
    }
}
