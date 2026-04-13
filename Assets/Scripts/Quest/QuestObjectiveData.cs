using System;
using UnityEngine;
using TCG.Items;

namespace TCG.Quest
{
    /// <summary>
    /// Defines one objective inside a QuestData asset.
    /// Tracks what must be done and how many times.
    /// Uses bool+value pairs for optional filters (JsonUtility-compatible).
    /// </summary>
    [Serializable]
    public class QuestObjectiveData
    {
        [Tooltip("Short description shown in the UI (e.g. 'Win 3 matches').")]
        public string description;

        public QuestObjectiveType objectiveType;

        [Tooltip("How many times this action must be performed.")]
        public int targetCount = 1;

        [Header("Optional Filters")]
        [Tooltip("If true, only matches using this element count toward the objective.")]
        public bool          filterByElement;
        public CardElement   elementFilter;

        [Tooltip("If true, only actions involving this card class count.")]
        public bool          filterByCardClass;
        public CardClass     classFilter;

        [Tooltip("If set, only this specific item ID counts (e.g. for PurchaseItem).")]
        public string        specificItemId;

        public override string ToString() =>
            $"{description} (0/{targetCount})";
    }
}
