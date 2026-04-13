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
    /// Fires GameEvents when items are added or removed.
    /// </summary>
    public class PlayerInventory : MonoBehaviour, IInventory
    {
        public static PlayerInventory Instance { get; private set; }

        private readonly List<InventoryItem> _items = new();

        public IReadOnlyList<InventoryItem> Items => _items.AsReadOnly();

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
            LoadInventory();
        }

        // ─── IInventory Implementation ─────────────────────────────────────────

        public bool HasItem(string itemId, int quantity = 1)
            => GetQuantity(itemId) >= quantity;

        public int GetQuantity(string itemId)
        {
            var entry = _items.FirstOrDefault(i => i.itemData.itemId == itemId);
            return entry?.quantity ?? 0;
        }

        public bool TryAddItem(ItemData item, int quantity = 1)
        {
            if (item == null || !item.IsValid() || quantity <= 0) return false;

            var existing = _items.FirstOrDefault(i => i.itemData.itemId == item.itemId);

            if (existing != null)
            {
                if (!item.isStackable)
                {
                    Debug.LogWarning($"[Inventory] '{item.displayName}' is not stackable and already owned.");
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
            }

            GameEvents.RaiseItemAdded(new InventoryItem(item, quantity));
            SaveInventory();
            return true;
        }

        public bool TryRemoveItem(string itemId, int quantity = 1)
        {
            var entry = _items.FirstOrDefault(i => i.itemData.itemId == itemId);
            if (entry == null || entry.quantity < quantity) return false;

            entry.quantity -= quantity;
            var removed = new InventoryItem(entry.itemData, quantity);

            if (entry.quantity <= 0)
                _items.Remove(entry);

            GameEvents.RaiseItemRemoved(removed);
            SaveInventory();
            return true;
        }

        public void Clear()
        {
            _items.Clear();
            SaveInventory();
        }

        // ─── Filtering helpers ─────────────────────────────────────────────────

        public List<InventoryItem> GetByType(ItemType type)
            => _items.Where(i => i.itemData.itemType == type).ToList();

        public List<InventoryItem> GetByRarity(ItemRarity rarity)
            => _items.Where(i => i.itemData.rarity == rarity).ToList();

        // ─── Persistence ───────────────────────────────────────────────────────

        private void LoadInventory()
        {
            // Inventory save data is item IDs + quantities.
            // ItemData assets are resolved via Resources.Load at runtime.
            var save = SaveSystem.Load();
            if (save?.inventoryItems == null) return;

            foreach (var entry in save.inventoryItems)
            {
                var itemData = Resources.Load<ItemData>($"Items/{entry.itemId}");
                if (itemData == null)
                {
                    Debug.LogWarning($"[Inventory] Could not find ItemData asset for id '{entry.itemId}'.");
                    continue;
                }
                _items.Add(new InventoryItem(itemData, entry.quantity));
            }
        }

        private void SaveInventory()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.inventoryItems = _items
                .Select(i => new InventoryItemSaveData
                {
                    itemId   = i.itemData.itemId,
                    quantity = i.quantity
                })
                .ToList();
            SaveSystem.Save(save);
        }
    }
}
