using System;
using TCG.Currency;
using TCG.Inventory;
using TCG.Inventory.Deck;
using TCG.Shop;

namespace TCG.Core
{
    /// <summary>
    /// Central static event bus for the entire game.
    /// All systems communicate through here — no direct references required.
    /// </summary>
    public static class GameEvents
    {
        // ── Currency ────────────────────────────────────────────────────────────
        public static event Action<CurrencyType, int>       OnCurrencyChanged;
        public static event Action<CurrencyType, int, bool> OnPurchaseAttempted;

        // ── Shop ────────────────────────────────────────────────────────────────
        public static event Action<ShopItemListing> OnItemPurchased;
        public static event Action                  OnShopRefreshed;
        public static event Action<string>          OnShopCategoryChanged;

        // ── Inventory ───────────────────────────────────────────────────────────
        public static event Action<InventoryItem>   OnItemAdded;
        public static event Action<InventoryItem>   OnItemRemoved;
        public static event Action<InventoryItem>   OnItemInspected;
        public static event Action                  OnInventoryOpened;
        public static event Action                  OnInventoryClosed;

        // ── Deck ────────────────────────────────────────────────────────────────
        public static event Action<DeckData>  OnDeckChanged;
        public static event Action<string>    OnDeckDeleted;       // deckId

        // ── Raisers ─────────────────────────────────────────────────────────────

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

        public static void RaiseItemInspected(InventoryItem item)
            => OnItemInspected?.Invoke(item);

        public static void RaiseInventoryOpened()
            => OnInventoryOpened?.Invoke();

        public static void RaiseInventoryClosed()
            => OnInventoryClosed?.Invoke();

        public static void RaiseDeckChanged(DeckData deck)
            => OnDeckChanged?.Invoke(deck);

        public static void RaiseDeckDeleted(string deckId)
            => OnDeckDeleted?.Invoke(deckId);
    }
}
