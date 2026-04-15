using System;
using UnityEngine;
using TCG.Currency;
using TCG.Items;

namespace TCG.Shop
{
    public enum ShopCategory { Featured, Daily, Cards, Packs, Cosmetics, Bundles }

    /// <summary>
    /// Represents a single purchasable slot in the shop.
    /// Can be placed in the Inspector (on ShopData) or created at runtime.
    /// </summary>
    [Serializable]
    public class ShopItemListing
    {
        [Tooltip("Unique identifier for this listing (used in save data).")]
        public string listingId;

        [Tooltip("The item being sold.")]
        public ItemData item;

        [Tooltip("Quantity of the item received per purchase.")]
        public int quantityPerPurchase = 1;

        [Header("Pricing")]
        public CurrencyType currency = CurrencyType.Gold;
        public int price;

        [Header("Stock & Visibility")]
        [Tooltip("-1 means unlimited stock.")]
        public int maxStock = -1;
        [HideInInspector]
        public int remainingStock = -1;

        [Tooltip("If true the listing has already been purchased this rotation.")]
        [HideInInspector]
        public bool isPurchased;

        [Header("Display")]
        public ShopCategory category = ShopCategory.Daily;
        [Tooltip("Optional badge text shown on the item tile (e.g. 'NEW', 'SALE').")]
        public string badgeText;
        [Tooltip("Percentage discount applied on top of the listed price (0–100).")]
        [Range(0, 100)]
        public int discountPercent;
        public bool isFeatured;

        // ─── Computed ─────────────────────────────────────────────────────────

        /// <summary>Final price after discount.</summary>
        public int FinalPrice
        {
            get
            {
                if (discountPercent <= 0) return price;
                return Mathf.Max(1, Mathf.RoundToInt(price * (1f - discountPercent / 100f)));
            }
        }

        public bool IsAvailable => !isPurchased && (maxStock < 0 || remainingStock > 0);

        /// <summary>Initializes runtime stock from the defined maxStock value.</summary>
        public void ResetStock()
        {
            remainingStock = maxStock;
            isPurchased = false;
        }

        public void InitListingId()
        {
            if (string.IsNullOrEmpty(listingId))
                listingId = $"{item?.itemId}_{Guid.NewGuid():N}";
        }
    }
}
