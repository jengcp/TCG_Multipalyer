using UnityEngine;
using TCG.Currency;

namespace TCG.Items
{
    /// <summary>
    /// Base ScriptableObject for every purchasable or collectable item.
    /// Create derived types (CardData, PackData, CosmeticData) for specifics.
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "TCG/Items/Base Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier used for save data and lookups.")]
        public string itemId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Classification")]
        public ItemType itemType;
        public ItemRarity rarity;

        [Header("Shop Pricing")]
        public bool isSellable = true;
        public CurrencyType primaryCurrency = CurrencyType.Gold;
        public int primaryPrice;
        public bool hasAlternatePrice;
        public CurrencyType alternateCurrency = CurrencyType.Gems;
        public int alternatePrice;

        [Header("Stack Rules")]
        public bool isStackable = false;
        public int maxStack = 1;

        /// <summary>Returns true if the item has a valid, non-empty itemId.</summary>
        public bool IsValid() => !string.IsNullOrEmpty(itemId);

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(itemId))
                itemId = name.ToLower().Replace(" ", "_");
        }
    }
}
