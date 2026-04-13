using System;
using TCG.Currency;
using TCG.Inventory;
using TCG.Shop;

namespace TCG.Core
{
    /// <summary>
    /// Central hub for game-wide events. Subscribe to receive notifications
    /// about shop purchases, currency changes, and inventory updates.
    /// </summary>
    public static class GameEvents
    {
        // Currency
        public static event Action<CurrencyType, int>        OnCurrencyChanged;
        public static event Action<CurrencyType, int, bool>  OnPurchaseAttempted; // type, amount, success

        // Shop
        public static event Action<ShopItemListing> OnItemPurchased;
        public static event Action                  OnShopRefreshed;
        public static event Action<string>          OnShopCategoryChanged;

        // Inventory
        public static event Action<InventoryItem>   OnItemAdded;
        public static event Action<InventoryItem>   OnItemRemoved;

        public static void RaiseCurrencyChanged(CurrencyType type, int newAmount)
            => OnCurrencyChanged?.Invoke(type, newAmount);

        public static void RaisePurchaseAttempted(CurrencyType type, int amount, bool success)
            => OnPurchaseAttempted?.Invoke(type, amount, success);

        public static void RaiseItemPurchased(ShopItemListing listing)
            => OnItemPurchased?.Invoke(listing);

        public static void RaiseShopRefreshed()
            => OnShopRefreshed?.Invoke();

        public static void RaiseShopCategoryChanged(string category)
            => OnShopCategoryChanged?.Invoke(category);

        public static void RaiseItemAdded(InventoryItem item)
            => OnItemAdded?.Invoke(item);

        public static void RaiseItemRemoved(InventoryItem item)
            => OnItemRemoved?.Invoke(item);
    }
}
