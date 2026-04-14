using System.Collections.Generic;
using UnityEngine;
using TCG.Core;
using TCG.Currency;
using TCG.Inventory;
using TCG.Items;
using TCG.Save;

namespace TCG.Gacha
{
    /// <summary>
    /// Singleton that handles all gacha pulls:
    /// gem spending, weighted card selection, pity system, inventory delivery, and events.
    ///
    /// Pity rules (per pool, persisted across sessions):
    ///   • After <c>rarePityThreshold</c> pulls without a Rare+, the next pull is forced Rare+.
    ///   • After <c>epicPityThreshold</c> pulls without an Epic+, the next pull is forced Epic+.
    ///   • The last pull of any multi-pull is guaranteed at least Rare regardless of pity.
    /// </summary>
    public class GachaManager : MonoBehaviour
    {
        public static GachaManager Instance { get; private set; }

        // Pity counters keyed by poolId
        private readonly Dictionary<string, int> _sinceRare = new();
        private readonly Dictionary<string, int> _sinceEpic = new();

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => LoadPity();

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts a single pull. Spends gems, applies pity, delivers card to inventory.
        /// Returns the pulled CardData, or null if the player cannot afford it.
        /// </summary>
        public CardData PullSingle(GachaPoolData pool)
        {
            if (pool == null || !SpendGems(pool.singlePullCost)) return null;

            var card   = PickCard(pool, forceMinRarity: ItemRarity.Common);
            if (card == null) return null;

            PlayerInventory.Instance?.TryAddItem(card, 1);
            GameEvents.RaiseGachaPullCompleted(new List<CardData> { card });
            SavePity();
            return card;
        }

        /// <summary>
        /// Attempts a multi-pull. The last card is guaranteed Rare+.
        /// Returns the list of pulled cards, or null if the player cannot afford it.
        /// </summary>
        public List<CardData> PullMulti(GachaPoolData pool)
        {
            if (pool == null || !SpendGems(pool.multiPullCost)) return null;

            int             count   = pool.multiPullCount;
            var             results = new List<CardData>(count);

            for (int i = 0; i < count; i++)
            {
                // Last pull of a multi guarantees Rare+
                bool isLast     = (i == count - 1);
                var  minRarity  = isLast ? ItemRarity.Rare : ItemRarity.Common;
                var  card       = PickCard(pool, minRarity);

                if (card != null)
                {
                    results.Add(card);
                    PlayerInventory.Instance?.TryAddItem(card, 1);
                }
            }

            GameEvents.RaiseGachaPullCompleted(results);
            SavePity();
            return results;
        }

        /// <summary>True if the player can afford a single pull from this pool.</summary>
        public bool CanAffordSingle(GachaPoolData pool)
            => pool != null
            && CurrencyManager.Instance != null
            && CurrencyManager.Instance.GetBalance(CurrencyType.Gems) >= pool.singlePullCost;

        /// <summary>True if the player can afford a multi-pull from this pool.</summary>
        public bool CanAffordMulti(GachaPoolData pool)
            => pool != null
            && CurrencyManager.Instance != null
            && CurrencyManager.Instance.GetBalance(CurrencyType.Gems) >= pool.multiPullCost;

        /// <summary>
        /// How many more pulls until the next guaranteed Rare on this pool.
        /// Returns 0 when pity is already active.
        /// </summary>
        public int GetPullsUntilRarePity(GachaPoolData pool)
        {
            if (pool == null) return 0;
            return Mathf.Max(0, pool.rarePityThreshold - SinceRare(pool.poolId));
        }

        // ── Core pick logic ────────────────────────────────────────────────────────

        /// <summary>
        /// Selects one card from the pool, applying pity and recording the result.
        /// </summary>
        private CardData PickCard(GachaPoolData pool, ItemRarity forceMinRarity)
        {
            // Escalate minimum rarity based on current pity counters
            if (SinceEpic(pool.poolId) >= pool.epicPityThreshold)
                forceMinRarity = (ItemRarity)Mathf.Max((int)forceMinRarity, (int)GachaPoolData.EpicPityMinRarity);
            else if (SinceRare(pool.poolId) >= pool.rarePityThreshold)
                forceMinRarity = (ItemRarity)Mathf.Max((int)forceMinRarity, (int)GachaPoolData.RarePityMinRarity);

            var (card, rarity) = WeightedPick(pool.entries, forceMinRarity);
            if (card == null) return null;

            // Update pity counters
            if (rarity >= ItemRarity.Epic)
            {
                _sinceEpic[pool.poolId] = 0;
                _sinceRare[pool.poolId] = 0;
            }
            else if (rarity >= ItemRarity.Rare)
            {
                _sinceRare[pool.poolId] = 0;
                Increment(_sinceEpic, pool.poolId);
            }
            else
            {
                Increment(_sinceRare, pool.poolId);
                Increment(_sinceEpic, pool.poolId);
            }

            return card;
        }

        /// <summary>
        /// Weighted random pick from <paramref name="entries"/> where rarity >= <paramref name="minRarity"/>.
        /// Falls back to the full pool if no entries meet the minimum.
        /// Returns (card, rarity) of the selected entry.
        /// </summary>
        private static (CardData card, ItemRarity rarity) WeightedPick(GachaPoolEntry[] entries, ItemRarity minRarity)
        {
            if (entries == null || entries.Length == 0) return (null, ItemRarity.Common);

            int total = 0;
            foreach (var e in entries)
                if (e.card != null && e.rarity >= minRarity)
                    total += e.weight;

            // Fallback: drop rarity requirement if nothing qualifies
            if (total == 0)
            {
                minRarity = ItemRarity.Common;
                foreach (var e in entries)
                    if (e.card != null)
                        total += e.weight;
            }

            if (total == 0) return (null, ItemRarity.Common);

            int roll       = Random.Range(0, total);
            int cumulative = 0;

            foreach (var e in entries)
            {
                if (e.card == null || e.rarity < minRarity) continue;
                cumulative += e.weight;
                if (roll < cumulative)
                    return (e.card, e.rarity);
            }

            // Should never reach here
            return (entries[0].card, entries[0].rarity);
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        private bool SpendGems(int cost)
            => CurrencyManager.Instance != null
            && CurrencyManager.Instance.TrySpend(CurrencyType.Gems, cost);

        private int SinceRare(string poolId) { EnsureKey(poolId); return _sinceRare[poolId]; }
        private int SinceEpic(string poolId) { EnsureKey(poolId); return _sinceEpic[poolId]; }

        private void EnsureKey(string poolId)
        {
            _sinceRare.TryAdd(poolId, 0);
            _sinceEpic.TryAdd(poolId, 0);
        }

        private static void Increment(Dictionary<string, int> dict, string key)
        {
            if (dict.ContainsKey(key)) dict[key]++;
            else dict[key] = 1;
        }

        // ── Persistence ────────────────────────────────────────────────────────────

        private void LoadPity()
        {
            var save = SaveSystem.Load();
            if (save?.gachaPity == null) return;

            foreach (var p in save.gachaPity)
            {
                _sinceRare[p.poolId] = p.pullsSinceRare;
                _sinceEpic[p.poolId] = p.pullsSinceEpic;
            }
        }

        private void SavePity()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.gachaPity = new List<GachaPitySaveData>();

            foreach (var kv in _sinceRare)
            {
                save.gachaPity.Add(new GachaPitySaveData
                {
                    poolId         = kv.Key,
                    pullsSinceRare = kv.Value,
                    pullsSinceEpic = _sinceEpic.TryGetValue(kv.Key, out int e) ? e : 0
                });
            }

            SaveSystem.Save(save);
        }
    }
}
