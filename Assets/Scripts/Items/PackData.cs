using System;
using UnityEngine;

namespace TCG.Items
{
    [Serializable]
    public class PackRarityWeight
    {
        public ItemRarity rarity;
        [Range(0f, 1f)]
        [Tooltip("Relative probability weight for this rarity slot.")]
        public float weight;
    }

    [Serializable]
    public class CardPool
    {
        public ItemRarity rarity;
        public CardData[] cards;
    }

    /// <summary>
    /// ScriptableObject representing a card pack that can be purchased
    /// and opened to receive random cards.
    /// </summary>
    [CreateAssetMenu(fileName = "New Pack", menuName = "TCG/Items/Card Pack")]
    public class PackData : ItemData
    {
        [Header("Pack Contents")]
        [Tooltip("Number of cards dealt when a pack is opened.")]
        public int cardsPerPack = 5;
        [Tooltip("Guaranteed minimum rare-or-higher cards per pack.")]
        public int guaranteedRareCount = 1;

        [Header("Rarity Weights")]
        [Tooltip("Probability weights for each rarity tier.")]
        public PackRarityWeight[] rarityWeights;

        [Header("Card Pools")]
        [Tooltip("Pools of cards that can appear in this pack, grouped by rarity.")]
        public CardPool[] cardPools;

        [Header("Visual")]
        public Sprite packOpenAnimation;
        [Tooltip("Tint or glow color representing the pack's theme.")]
        public Color packColor = Color.white;

        private void Awake()
        {
            itemType = ItemType.CardPack;
            isStackable = true;
            maxStack = 99;
        }

        /// <summary>
        /// Rolls and returns an array of CardData using the defined rarity weights.
        /// Guarantees at least <see cref="guaranteedRareCount"/> rare-or-higher cards.
        /// </summary>
        public CardData[] OpenPack()
        {
            if (cardPools == null || cardPools.Length == 0)
            {
                Debug.LogWarning($"[PackData] '{displayName}' has no card pools defined.");
                return Array.Empty<CardData>();
            }

            var result = new CardData[cardsPerPack];
            int guaranteedSlotsLeft = guaranteedRareCount;

            for (int i = 0; i < cardsPerPack; i++)
            {
                bool forceRare = guaranteedSlotsLeft > 0 && (cardsPerPack - i) <= guaranteedSlotsLeft;
                result[i] = RollCard(forceRare);
                if (result[i] != null && result[i].rarity >= ItemRarity.Rare)
                    guaranteedSlotsLeft--;
            }

            return result;
        }

        private CardData RollCard(bool forceRareOrHigher)
        {
            ItemRarity rolledRarity = RollRarity(forceRareOrHigher);
            CardPool pool = GetPool(rolledRarity);

            if (pool == null || pool.cards == null || pool.cards.Length == 0)
            {
                Debug.LogWarning($"[PackData] No cards in pool for rarity {rolledRarity}.");
                return null;
            }

            return pool.cards[UnityEngine.Random.Range(0, pool.cards.Length)];
        }

        private ItemRarity RollRarity(bool forceRareOrHigher)
        {
            if (rarityWeights == null || rarityWeights.Length == 0)
                return ItemRarity.Common;

            float totalWeight = 0f;
            foreach (var rw in rarityWeights)
            {
                if (!forceRareOrHigher || rw.rarity >= ItemRarity.Rare)
                    totalWeight += rw.weight;
            }

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var rw in rarityWeights)
            {
                if (forceRareOrHigher && rw.rarity < ItemRarity.Rare) continue;
                cumulative += rw.weight;
                if (roll <= cumulative) return rw.rarity;
            }

            return ItemRarity.Rare;
        }

        private CardPool GetPool(ItemRarity rarity)
        {
            foreach (var pool in cardPools)
                if (pool.rarity == rarity) return pool;
            return null;
        }
    }
}
