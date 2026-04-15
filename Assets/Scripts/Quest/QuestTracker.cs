using System.Collections.Generic;
using UnityEngine;
using TCG.Core;
using TCG.Currency;
using TCG.Inventory;
using TCG.Items;
using TCG.Shop;

namespace TCG.Quest
{
    /// <summary>
    /// Listens to all game-wide events and forwards progress increments to
    /// the active QuestManager. Does not own quest state — purely a listener.
    ///
    /// Attach alongside QuestManager on the same GameObject.
    /// </summary>
    public class QuestTracker : MonoBehaviour
    {
        private QuestManager _manager;

        private void Awake()
        {
            _manager = GetComponent<QuestManager>();
        }

        private void OnEnable()  => Subscribe();
        private void OnDisable() => Unsubscribe();

        // ─── Subscriptions ─────────────────────────────────────────────────────

        private void Subscribe()
        {
            // Shop / currency
            GameEvents.OnItemPurchased    += OnItemPurchased;
            GameEvents.OnPurchaseAttempted += OnPurchaseAttempted;
            GameEvents.OnCurrencyChanged  += OnCurrencyChanged;

            // Inventory
            GameEvents.OnItemAdded        += OnItemAdded;

            // Deck builder
            GameEvents.OnDeckChanged      += OnDeckChanged;

            // Match (raised by the game session system when built)
            GameEvents.OnMatchCompleted   += OnMatchCompleted;
            GameEvents.OnCardPlayed       += OnCardPlayed;
            GameEvents.OnPackOpened       += OnPackOpened;
            GameEvents.OnDayLogin         += OnDayLogin;
            GameEvents.OnGoldEarned       += OnGoldEarned;
        }

        private void Unsubscribe()
        {
            GameEvents.OnItemPurchased    -= OnItemPurchased;
            GameEvents.OnPurchaseAttempted -= OnPurchaseAttempted;
            GameEvents.OnCurrencyChanged  -= OnCurrencyChanged;
            GameEvents.OnItemAdded        -= OnItemAdded;
            GameEvents.OnDeckChanged      -= OnDeckChanged;
            GameEvents.OnMatchCompleted   -= OnMatchCompleted;
            GameEvents.OnCardPlayed       -= OnCardPlayed;
            GameEvents.OnPackOpened       -= OnPackOpened;
            GameEvents.OnDayLogin         -= OnDayLogin;
            GameEvents.OnGoldEarned       -= OnGoldEarned;
        }

        // ─── Event Handlers → Objective Types ─────────────────────────────────

        private void OnItemPurchased(ShopItemListing listing)
        {
            Progress(QuestObjectiveType.PurchaseItem, listing.item?.itemId);
        }

        private void OnPurchaseAttempted(CurrencyType type, int amount, bool success)
        {
            if (!success) return;

            switch (type)
            {
                case CurrencyType.Gold:   Progress(QuestObjectiveType.SpendGold,   amount: amount); break;
                case CurrencyType.Gems:   Progress(QuestObjectiveType.SpendGems,   amount: amount); break;
                case CurrencyType.Shards: Progress(QuestObjectiveType.SpendShards, amount: amount); break;
            }
        }

        private void OnCurrencyChanged(CurrencyType type, int _) { /* handled via OnGoldEarned */ }

        private void OnItemAdded(InventoryItem item)
        {
            if (item?.itemData == null) return;

            if (item.itemData.itemType == ItemType.Card)
            {
                var inventory = PlayerInventory.Instance;
                if (inventory != null)
                {
                    Progress(QuestObjectiveType.CollectUniqueCard,
                        targetValue: inventory.TotalUniqueItems);
                    Progress(QuestObjectiveType.CollectTotalCards,
                        targetValue: inventory.TotalItemCount);
                }
            }
        }

        private void OnDeckChanged(Inventory.Deck.DeckData deck)
        {
            var manager = Inventory.Deck.DeckManager.Instance;
            if (manager != null)
                Progress(QuestObjectiveType.CreateDeck, targetValue: manager.Decks.Count);

            if (deck != null)
                Progress(QuestObjectiveType.ReachDeckSize, targetValue: deck.TotalCards);
        }

        private void OnMatchCompleted(bool won, CardElement primaryElement, CardClass primaryClass)
        {
            Progress(QuestObjectiveType.PlayMatch);

            if (won)
            {
                Progress(QuestObjectiveType.WinMatch);
                Progress(QuestObjectiveType.WinMatchWithElement, elementContext: primaryElement);
                Progress(QuestObjectiveType.WinMatchWithClass,   classContext:   primaryClass);
            }
        }

        private void OnCardPlayed(CardData card)
        {
            Progress(QuestObjectiveType.PlayCard,
                elementContext: card?.element ?? CardElement.Neutral,
                classContext:   card?.cardClass ?? CardClass.Creature);
        }

        private void OnPackOpened(PackData pack)
        {
            Progress(QuestObjectiveType.OpenPack, specificItemId: pack?.itemId);
        }

        private void OnDayLogin()
        {
            Progress(QuestObjectiveType.LoginDay);
        }

        private void OnGoldEarned(int amount)
        {
            Progress(QuestObjectiveType.EarnGold, amount: amount);
        }

        // ─── Forwarding Helper ─────────────────────────────────────────────────

        /// <summary>
        /// Central dispatch: tells the QuestManager to advance all matching objectives.
        /// </summary>
        private void Progress(
            QuestObjectiveType type,
            string             specificItemId = null,
            CardElement        elementContext = CardElement.Neutral,
            CardClass          classContext   = CardClass.Creature,
            int                amount         = 1,
            int                targetValue    = -1)
        {
            _manager?.NotifyProgress(type, specificItemId, elementContext, classContext, amount, targetValue);
        }
    }
}
