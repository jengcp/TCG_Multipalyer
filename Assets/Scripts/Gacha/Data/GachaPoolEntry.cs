using System;
using TCG.Items;

namespace TCG.Gacha
{
    /// <summary>
    /// One card in a gacha pool, with its rarity tier and relative pull weight.
    /// </summary>
    [Serializable]
    public class GachaPoolEntry
    {
        public CardData    card;

        /// <summary>
        /// Rarity tier used for pity tracking. Drawn from the shared ItemRarity enum
        /// so it aligns with inventory display and filtering.
        /// </summary>
        public ItemRarity  rarity;

        /// <summary>
        /// Relative probability weight. Higher = more likely relative to other entries.
        /// </summary>
        public int         weight = 100;
    }
}
