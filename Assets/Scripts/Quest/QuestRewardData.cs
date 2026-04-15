using System;
using UnityEngine;
using TCG.Currency;
using TCG.Items;

namespace TCG.Quest
{
    /// <summary>
    /// Defines a single reward granted when a quest is claimed.
    /// Serialized inside QuestData (ScriptableObject).
    /// </summary>
    [Serializable]
    public class QuestRewardData
    {
        public QuestRewardType rewardType;

        [Header("Currency Reward")]
        public CurrencyType currencyType;
        public int          currencyAmount;

        [Header("Item Reward")]
        public ItemData itemReward;
        public int      itemQuantity = 1;

        [Header("XP Reward")]
        public int xpAmount;

        public override string ToString()
        {
            return rewardType switch
            {
                QuestRewardType.Currency => $"{currencyAmount} {currencyType}",
                QuestRewardType.Item     => $"{itemQuantity}x {itemReward?.displayName ?? "Item"}",
                QuestRewardType.XP       => $"{xpAmount} XP",
                _                        => "Unknown Reward"
            };
        }
    }
}
