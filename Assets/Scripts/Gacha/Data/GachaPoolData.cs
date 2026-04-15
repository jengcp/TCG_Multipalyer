using UnityEngine;
using TCG.Items;

namespace TCG.Gacha
{
    /// <summary>
    /// ScriptableObject that defines a gacha banner: cards, weights, pull costs, and pity rules.
    /// </summary>
    [CreateAssetMenu(fileName = "New Gacha Pool", menuName = "TCG/Gacha/Pool")]
    public class GachaPoolData : ScriptableObject
    {
        [Header("Identity")]
        public string poolId;
        public string poolName;
        public Sprite bannerArt;

        [Header("Pull Costs (Gemstones)")]
        [Tooltip("Cost for a single pull.")]
        public int singlePullCost  = 150;
        [Tooltip("Cost for a 10-pull (should be ~10% less than 10× single).")]
        public int multiPullCost   = 1350;
        [Tooltip("How many cards a multi-pull gives.")]
        public int multiPullCount  = 10;

        [Header("Pity Rules")]
        [Tooltip("After this many pulls without a Rare or better, the next pull is guaranteed Rare+.")]
        public int rarePityThreshold  = 10;
        [Tooltip("After this many pulls without an Epic or better, the next pull is guaranteed Epic+.")]
        public int epicPityThreshold  = 50;

        [Header("Card Pool")]
        public GachaPoolEntry[] entries;

        /// <summary>Minimum rarity for a 'Rare pity' forced pull.</summary>
        public const ItemRarity RarePityMinRarity  = ItemRarity.Rare;
        /// <summary>Minimum rarity for an 'Epic pity' forced pull.</summary>
        public const ItemRarity EpicPityMinRarity  = ItemRarity.Epic;
    }
}
