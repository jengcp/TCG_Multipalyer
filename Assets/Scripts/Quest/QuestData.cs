using System.Collections.Generic;
using UnityEngine;

namespace TCG.Quest
{
    /// <summary>
    /// ScriptableObject that defines a quest — its objectives, rewards, category,
    /// and expiry rules. Create one asset per quest via the context menu.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuest", menuName = "TCG/Quest/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique ID used for save data lookups.")]
        public string questId;
        public string questName;
        [TextArea(2, 4)]
        public string questDescription;
        public Sprite questIcon;

        [Header("Category & Ordering")]
        public QuestCategory category;
        [Tooltip("Lower values appear at the top of the quest list.")]
        public int sortOrder;

        [Header("Objectives")]
        [Tooltip("All objectives must be complete to finish the quest.")]
        public List<QuestObjectiveData> objectives = new();

        [Header("Rewards")]
        public List<QuestRewardData> rewards = new();

        [Header("Expiry")]
        [Tooltip("Hours until this quest expires from when it becomes Active. 0 = never expires.")]
        public float expiryHours;

        [Header("Prerequisites")]
        [Tooltip("Quest IDs that must be Claimed before this quest unlocks.")]
        public List<string> prerequisiteQuestIds = new();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(questId))
                questId = name.ToLower().Replace(" ", "_");
        }
    }
}
