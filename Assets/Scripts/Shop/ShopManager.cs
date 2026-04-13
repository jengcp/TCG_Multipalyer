using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TCG.Core;
using TCG.Currency;
using TCG.Inventory;
using TCG.Items;
using TCG.Save;

namespace TCG.Shop
{
    /// <summary>
    /// Core shop logic: manages active listings, rotation schedule, and purchase processing.
    /// Does NOT contain UI code. Attach ShopController to your scene for wiring.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private ShopData shopData;

        // Active listings exposed to UI
        public IReadOnlyList<ShopItemListing> ActiveListings => _activeListings.AsReadOnly();
        public DateTime NextRefreshTime { get; private set; }

        private readonly List<ShopItemListing> _activeListings = new();
        private ShopSaveData _shopSave;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (shopData == null)
            {
                Debug.LogError("[ShopManager] No ShopData assigned. Shop will not function.");
                return;
            }

            LoadShopState();
            RefreshIfNeeded();
        }

        private void Update()
        {
            if (shopData.refreshIntervalHours > 0 && DateTime.UtcNow >= NextRefreshTime)
                Refresh();
        }

        // ─── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to purchase the given listing.
        /// Returns a <see cref="PurchaseResult"/> describing the outcome.
        /// </summary>
        public PurchaseResult TryPurchase(ShopItemListing listing)
        {
            if (listing == null || listing.item == null)
                return PurchaseResult.Fail(PurchaseStatus.InvalidListing, listing, "Listing is invalid.");

            if (!_activeListings.Contains(listing))
                return PurchaseResult.Fail(PurchaseStatus.InvalidListing, listing, "Listing is not active in this shop.");

            if (!listing.IsAvailable)
            {
                if (listing.isPurchased)
                    return PurchaseResult.Fail(PurchaseStatus.AlreadyOwned, listing, $"'{listing.item.displayName}' has already been purchased.");

                return PurchaseResult.Fail(PurchaseStatus.OutOfStock, listing, $"'{listing.item.displayName}' is out of stock.");
            }

            var currency = CurrencyManager.Instance;
            if (currency == null)
                return PurchaseResult.Fail(PurchaseStatus.InvalidListing, listing, "CurrencyManager not found.");

            if (!currency.HasEnough(listing.currency, listing.FinalPrice))
                return PurchaseResult.Fail(PurchaseStatus.InsufficientFunds, listing,
                    $"Not enough {listing.currency}. Need {listing.FinalPrice}, have {currency.GetBalance(listing.currency)}.");

            var inventory = PlayerInventory.Instance;
            if (inventory == null)
                return PurchaseResult.Fail(PurchaseStatus.InventoryFull, listing, "PlayerInventory not found.");

            // Deduct currency
            currency.TrySpend(listing.currency, listing.FinalPrice);

            // Add item(s) to inventory
            bool added = inventory.TryAddItem(listing.item, listing.quantityPerPurchase);
            if (!added)
            {
                // Refund on inventory failure
                currency.Add(listing.currency, listing.FinalPrice);
                return PurchaseResult.Fail(PurchaseStatus.InventoryFull, listing, "Inventory is full.");
            }

            // Handle card packs: auto-open if configured
            if (listing.item is PackData pack)
                ProcessPackOpening(pack, listing.quantityPerPurchase);

            // Update listing state
            if (listing.maxStock > 0)
                listing.remainingStock = Mathf.Max(0, listing.remainingStock - 1);

            if (listing.maxStock == 1)
                listing.isPurchased = true;

            GameEvents.RaiseItemPurchased(listing);
            SaveShopState();

            Debug.Log($"[ShopManager] Purchased '{listing.item.displayName}' for {listing.FinalPrice} {listing.currency}.");
            return PurchaseResult.Success(listing);
        }

        /// <summary>Forces an immediate shop rotation regardless of the timer.</summary>
        public void Refresh()
        {
            BuildActiveListings();
            SetNextRefreshTime();
            GameEvents.RaiseShopRefreshed();
            SaveShopState();
            Debug.Log($"[ShopManager] Shop refreshed. Next refresh at {NextRefreshTime:u}");
        }

        /// <summary>Returns the time remaining until the next scheduled refresh.</summary>
        public TimeSpan TimeUntilRefresh()
            => NextRefreshTime > DateTime.UtcNow ? NextRefreshTime - DateTime.UtcNow : TimeSpan.Zero;

        // ─── Private Helpers ───────────────────────────────────────────────────

        private void RefreshIfNeeded()
        {
            if (shopData.refreshIntervalHours <= 0 || DateTime.UtcNow >= NextRefreshTime)
                Refresh();
        }

        private void BuildActiveListings()
        {
            _activeListings.Clear();

            // 1. Always include permanent listings
            foreach (var listing in shopData.permanentListings)
            {
                listing.InitListingId();
                RestockFromSave(listing);
                _activeListings.Add(listing);
            }

            // 2. Pick random featured listings
            var featured = PickRandom(shopData.featuredPool, shopData.featuredListingCount);
            foreach (var listing in featured)
            {
                listing.isFeatured = true;
                listing.category   = ShopCategory.Featured;
                listing.InitListingId();
                listing.ResetStock();
                _activeListings.Add(listing);
            }

            // 3. Pick random daily listings
            var daily = PickRandom(shopData.dailyPool, shopData.dailyListingCount);
            foreach (var listing in daily)
            {
                listing.category = ShopCategory.Daily;
                listing.InitListingId();
                listing.ResetStock();
                _activeListings.Add(listing);
            }
        }

        private void RestockFromSave(ShopItemListing listing)
        {
            if (_shopSave?.permanentListingStates == null) { listing.ResetStock(); return; }

            var saved = _shopSave.permanentListingStates
                .FirstOrDefault(s => s.listingId == listing.listingId);

            if (saved != null)
            {
                listing.remainingStock = saved.remainingStock;
                listing.isPurchased    = saved.isPurchased;
            }
            else
            {
                listing.ResetStock();
            }
        }

        private static List<T> PickRandom<T>(List<T> source, int count)
        {
            if (source == null || source.Count == 0) return new List<T>();
            count = Mathf.Min(count, source.Count);
            var copy = new List<T>(source);
            var result = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                int idx = UnityEngine.Random.Range(0, copy.Count);
                result.Add(copy[idx]);
                copy.RemoveAt(idx);
            }

            return result;
        }

        private void ProcessPackOpening(PackData pack, int packCount)
        {
            var inventory = PlayerInventory.Instance;
            // Remove packs from inventory and add individual cards
            inventory.TryRemoveItem(pack.itemId, packCount);

            for (int p = 0; p < packCount; p++)
            {
                CardData[] cards = pack.OpenPack();
                foreach (var card in cards)
                {
                    if (card != null)
                        inventory.TryAddItem(card);
                }
                Debug.Log($"[ShopManager] Opened pack '{pack.displayName}', received {cards.Length} cards.");
            }
        }

        // ─── Persistence ───────────────────────────────────────────────────────

        private void LoadShopState()
        {
            var save = SaveSystem.Load();
            _shopSave = save?.shop ?? new ShopSaveData();

            if (_shopSave.lastRefreshTicks > 0)
                NextRefreshTime = new DateTime(_shopSave.lastRefreshTicks, DateTimeKind.Utc)
                    .AddHours(shopData.refreshIntervalHours);
            else
                NextRefreshTime = DateTime.UtcNow; // trigger immediate refresh
        }

        private void SaveShopState()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.shop ??= new ShopSaveData();
            save.shop.lastRefreshTicks = DateTime.UtcNow.Ticks;

            save.shop.permanentListingStates = _activeListings
                .Where(l => shopData.permanentListings.Contains(l))
                .Select(l => new ShopListingSaveData
                {
                    listingId      = l.listingId,
                    remainingStock = l.remainingStock,
                    isPurchased    = l.isPurchased
                })
                .ToList();

            SaveSystem.Save(save);
        }

        private void SetNextRefreshTime()
        {
            NextRefreshTime = DateTime.UtcNow.AddHours(
                shopData.refreshIntervalHours > 0 ? shopData.refreshIntervalHours : double.MaxValue / 2);
        }
    }
}
