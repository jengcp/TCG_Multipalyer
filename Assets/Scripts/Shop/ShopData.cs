using System.Collections.Generic;
using UnityEngine;

namespace TCG.Shop
{
    /// <summary>
    /// ScriptableObject that configures a shop's item pool and rotation rules.
    /// Create one ShopData asset per distinct shop (main shop, event shop, etc.).
    /// </summary>
    [CreateAssetMenu(fileName = "ShopData", menuName = "TCG/Shop/Shop Data")]
    public class ShopData : ScriptableObject
    {
        [Header("Identity")]
        public string shopId;
        public string shopDisplayName = "Shop";

        [Header("Rotation")]
        [Tooltip("How many hours between shop refreshes. 0 = no auto-refresh.")]
        public float refreshIntervalHours = 24f;
        [Tooltip("Number of random daily listings to surface on refresh.")]
        public int dailyListingCount = 6;
        [Tooltip("Number of featured listings shown at the top.")]
        public int featuredListingCount = 2;

        [Header("Item Pools")]
        [Tooltip("Fixed listings always visible (e.g. gem bundles, starter packs).")]
        public List<ShopItemListing> permanentListings = new();
        [Tooltip("Pool of listings rotated daily.")]
        public List<ShopItemListing> dailyPool = new();
        [Tooltip("Pool of listings chosen for the featured banner.")]
        public List<ShopItemListing> featuredPool = new();
    }
}
