using System;
using TCG.Currency;
using TCG.Inventory;
using TCG.Inventory.Deck;
using TCG.Items;
using TCG.Quest;
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
        public static event Action<string>    OnDeckDeleted;

        // ── Match (raised by the game session system) ────────────────────────────
        /// <summary>
        /// Raised when a match ends.
        /// Parameters: won, primaryElement (most-used), primaryClass (most-used)
        /// </summary>
        public static event Action<bool, CardElement, CardClass> OnMatchCompleted;
        /// <summary>Raised each time a card is played during a match.</summary>
        public static event Action<CardData>  OnCardPlayed;
        /// <summary>Raised when a card pack is opened (after cards are distributed).</summary>
        public static event Action<PackData>  OnPackOpened;
        /// <summary>Raised once per calendar-day login.</summary>
        public static event Action            OnDayLogin;
        /// <summary>Raised whenever Gold is added to the player's wallet.</summary>
        public static event Action<int>       OnGoldEarned;

        // ── Quest ───────────────────────────────────────────────────────────────
        public static event Action<QuestProgress> OnQuestCompleted;
        public static event Action<QuestProgress> OnQuestClaimed;
        public static event Action<QuestProgress> OnQuestExpired;
        public static event Action<QuestCategory> OnQuestRotationRefreshed;
        /// <summary>Raised when an XP reward is granted (reserved for future level system).</summary>
        public static event Action<int>           OnXPEarned;

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

        public static void RaiseMatchCompleted(bool won, CardElement element, CardClass cardClass)
            => OnMatchCompleted?.Invoke(won, element, cardClass);

        public static void RaiseCardPlayed(CardData card)
            => OnCardPlayed?.Invoke(card);

        public static void RaisePackOpened(PackData pack)
            => OnPackOpened?.Invoke(pack);

        public static void RaiseDayLogin()
            => OnDayLogin?.Invoke();

        public static void RaiseGoldEarned(int amount)
            => OnGoldEarned?.Invoke(amount);

        public static void RaiseQuestCompleted(QuestProgress quest)
            => OnQuestCompleted?.Invoke(quest);

        public static void RaiseQuestClaimed(QuestProgress quest)
            => OnQuestClaimed?.Invoke(quest);

        public static void RaiseQuestExpired(QuestProgress quest)
            => OnQuestExpired?.Invoke(quest);

        public static void RaiseQuestRotationRefreshed(QuestCategory category)
            => OnQuestRotationRefreshed?.Invoke(category);

        public static void RaiseXPEarned(int amount)
            => OnXPEarned?.Invoke(amount);
    }
}
