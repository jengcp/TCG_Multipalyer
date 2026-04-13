using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TCG.Core;
using TCG.Items;
using TCG.Save;

namespace TCG.Inventory
{
    /// <summary>
    /// Singleton MonoBehaviour that manages the player's entire item collection.
    /// Supports add / remove / move / filter / sort.
    /// Fires GameEvents when items are added, removed, or the inventory is opened/closed.
    /// </summary>
    public class PlayerInventory : MonoBehaviour, IInventory
    {
        public static PlayerInventory Instance { get; private set; }

        // ─── State ─────────────────────────────────────────────────────────────

        private readonly List<InventoryItem> _items = new();

        // Tracks the DateTime (UTC ticks) each item was first obtained — used for DateAdded sorting.
        private readonly Dictionary<string, long> _acquiredTicks = new();

        public IReadOnlyList<InventoryItem> Items => _items.AsReadOnly();
        public int TotalUniqueItems => _items.Count;
        public int TotalItemCount   => _items.Sum(i => i.quantity);

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => LoadInventory();

        // ─── IInventory — Data Access ──────────────────────────────────────────

        public bool HasItem(string itemId, int quantity = 1)
            => GetQuantity(itemId) >= quantity;

        public int GetQuantity(string itemId)
        {
            var entry = Find(itemId);
            return entry?.quantity ?? 0;
        }

        public InventoryItem GetItem(string itemId) => Find(itemId);

        // ─── IInventory — Mutation ─────────────────────────────────────────────

        public bool TryAddItem(ItemData item, int quantity = 1)
        {
            if (item == null || !item.IsValid() || quantity <= 0) return false;

            var existing = Find(item.itemId);

            if (existing != null)
            {
                if (!item.isStackable)
                {
                    Debug.LogWarning($"[Inventory] '{item.displayName}' is not stackable — already owned.");
                    return false;
                }

                int room = item.maxStack - existing.quantity;
                if (room <= 0)
                {
                    Debug.LogWarning($"[Inventory] Stack full for '{item.displayName}'.");
                    return false;
                }

                existing.quantity += Mathf.Min(quantity, room);
            }
            else
            {
                _items.Add(new InventoryItem(item, Mathf.Min(quantity, item.maxStack)));
                _acquiredTicks.TryAdd(item.itemId, DateTime.UtcNow.Ticks);
            }

            GameEvents.RaiseItemAdded(new InventoryItem(item, quantity));
            SaveInventory();
            return true;
        }

        public bool TryRemoveItem(string itemId, int quantity = 1)
        {
            var entry = Find(itemId);
            if (entry == null || entry.quantity < quantity) return false;

            entry.quantity -= quantity;
            var removed = new InventoryItem(entry.itemData, quantity);

            if (entry.quantity <= 0)
            {
                _items.Remove(entry);
                _acquiredTicks.Remove(itemId);
            }

            GameEvents.RaiseItemRemoved(removed);
            SaveInventory();
            return true;
        }

        /// <summary>
        /// Moves <paramref name="quantity"/> units of an item from this inventory
        /// to <paramref name="destination"/>. Both inventories are updated atomically.
        /// </summary>
        public bool TryMoveItem(string itemId, int quantity, IInventory destination)
        {
            if (destination == null || destination == (IInventory)this) return false;

            var entry = Find(itemId);
            if (entry == null || entry.quantity < quantity) return false;

            bool added = destination.TryAddItem(entry.itemData, quantity);
            if (!added) return false;

            TryRemoveItem(itemId, quantity);
            return true;
        }

        public void Clear()
        {
            _items.Clear();
            _acquiredTicks.Clear();
            SaveInventory();
        }

        // ─── IInventory — Query ────────────────────────────────────────────────

        public List<InventoryItem> GetFiltered(InventoryFilter filter)
        {
            if (filter == null || filter.IsEmpty) return new List<InventoryItem>(_items);
            return _items.Where(filter.Matches).ToList();
        }

        public List<InventoryItem> GetSorted(InventorySortOrder order)
        {
            return order switch
            {
                InventorySortOrder.NameAscending    => _items.OrderBy(i  => i.itemData.displayName).ToList(),
                InventorySortOrder.NameDescending   => _items.OrderByDescending(i => i.itemData.displayName).ToList(),
                InventorySortOrder.RarityAscending  => _items.OrderBy(i  => i.itemData.rarity).ToList(),
                InventorySortOrder.RarityDescending => _items.OrderByDescending(i => i.itemData.rarity).ToList(),
                InventorySortOrder.TypeAscending    => _items.OrderBy(i  => i.itemData.itemType).ToList(),
                InventorySortOrder.QuantityAscending  => _items.OrderBy(i => i.quantity).ToList(),
                InventorySortOrder.QuantityDescending => _items.OrderByDescending(i => i.quantity).ToList(),
                InventorySortOrder.DateAddedNewest  => _items.OrderByDescending(i =>
                    _acquiredTicks.TryGetValue(i.itemData.itemId, out long t) ? t : 0).ToList(),
                InventorySortOrder.DateAddedOldest  => _items.OrderBy(i =>
                    _acquiredTicks.TryGetValue(i.itemData.itemId, out long t) ? t : 0).ToList(),
                _                                   => new List<InventoryItem>(_items)
            };
        }

        public List<InventoryItem> GetByType(ItemType type)
            => _items.Where(i => i.itemData.itemType == type).ToList();

        public List<InventoryItem> GetByRarity(ItemRarity rarity)
            => _items.Where(i => i.itemData.rarity == rarity).ToList();

        /// <summary>Applies both a filter and a sort in one call.</summary>
        public List<InventoryItem> GetFilteredAndSorted(InventoryFilter filter, InventorySortOrder order)
        {
            var filtered = GetFiltered(filter);
            return SortList(filtered, order);
        }

        // ─── Private Helpers ───────────────────────────────────────────────────

        private InventoryItem Find(string itemId)
            => _items.FirstOrDefault(i => i.itemData.itemId == itemId);

        private static List<InventoryItem> SortList(List<InventoryItem> list, InventorySortOrder order)
        {
            return order switch
            {
                InventorySortOrder.NameAscending      => list.OrderBy(i => i.itemData.displayName).ToList(),
                InventorySortOrder.NameDescending     => list.OrderByDescending(i => i.itemData.displayName).ToList(),
                InventorySortOrder.RarityAscending    => list.OrderBy(i => i.itemData.rarity).ToList(),
                InventorySortOrder.RarityDescending   => list.OrderByDescending(i => i.itemData.rarity).ToList(),
                InventorySortOrder.TypeAscending      => list.OrderBy(i => i.itemData.itemType).ToList(),
                InventorySortOrder.QuantityAscending  => list.OrderBy(i => i.quantity).ToList(),
                InventorySortOrder.QuantityDescending => list.OrderByDescending(i => i.quantity).ToList(),
                _                                     => list
            };
        }

        // ─── Persistence ───────────────────────────────────────────────────────

        private void LoadInventory()
        {
            var save = SaveSystem.Load();
            if (save?.inventoryItems == null) return;

            foreach (var entry in save.inventoryItems)
            {
                var itemData = Resources.Load<ItemData>($"Items/{entry.itemId}");
                if (itemData == null)
                {
                    Debug.LogWarning($"[Inventory] Could not find ItemData for id '{entry.itemId}'.");
                    continue;
                }
                _items.Add(new InventoryItem(itemData, entry.quantity));
                _acquiredTicks[entry.itemId] = entry.acquiredTicks;
            }
        }

        private void SaveInventory()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.inventoryItems = _items.Select(i => new InventoryItemSaveData
            {
                itemId        = i.itemData.itemId,
                quantity      = i.quantity,
                acquiredTicks = _acquiredTicks.TryGetValue(i.itemData.itemId, out long t)
                                    ? t : DateTime.UtcNow.Ticks
            }).ToList();
            SaveSystem.Save(save);
        }
    }
}
