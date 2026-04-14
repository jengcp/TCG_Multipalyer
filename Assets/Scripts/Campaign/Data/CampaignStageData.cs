using System;
using System.Collections.Generic;
using UnityEngine;
using TCG.Items;

namespace TCG.Campaign
{
    /// <summary>
    /// ScriptableObject describing one node in the campaign hex grid.
    /// Stores layout position, the AI enemy deck, star criteria, and rewards.
    /// </summary>
    [CreateAssetMenu(fileName = "New Stage", menuName = "TCG/Campaign/Stage")]
    public class CampaignStageData : ScriptableObject
    {
        [Header("Identity")]
        public string stageId;
        public string stageName;
        [TextArea(1, 3)]
        public string description;
        public Sprite stageIcon;

        [Header("Hex Grid Position")]
        [Tooltip("Column in the hex grid (left = 0).")]
        public int gridColumn;
        [Tooltip("Row in the hex grid (bottom = 0). Odd columns are offset upward.")]
        public int gridRow;

        [Header("Unlock Prerequisites")]
        [Tooltip("Stages that must each have at least 1 star before this stage unlocks.")]
        public CampaignStageData[] prerequisites;

        [Header("AI Enemy Deck")]
        [Tooltip("Cards that make up the enemy deck. Each entry is one card type.")]
        public AiDeckEntry[] aiDeckEntries;
        [Tooltip("Minimum total cards in the AI deck (padded with first entry if needed).")]
        public int minAiDeckSize = 20;

        [Header("Star Criteria")]
        [Tooltip("Up to 3 entries — each one defines what earns the matching star.")]
        public StarCriteriaData[] starCriteria;

        [Header("Rewards per Star")]
        [Tooltip("Card awarded when that star index (0-based) is earned for the first time. Null = no card.")]
        public CardData[] starCardRewards;   // length should match starCriteria

        [Header("Full-Star Gemstone Bonus")]
        [Tooltip("Gemstones awarded when all 3 stars are earned for the first time.")]
        public int gemstoneOnFullStars = 50;
    }

    /// <summary>Card + copy count for building an AI deck from a stage asset.</summary>
    [Serializable]
    public class AiDeckEntry
    {
        public CardData card;
        [Range(1, 4)]
        public int count = 2;
    }
}
