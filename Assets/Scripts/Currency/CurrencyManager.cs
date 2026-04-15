using System;
using System.Collections.Generic;
using UnityEngine;
using TCG.Core;
using TCG.Save;

namespace TCG.Currency
{
    /// <summary>
    /// Singleton MonoBehaviour that owns and manages all player currencies.
    /// Persists via SaveSystem. Subscribe to GameEvents.OnCurrencyChanged for UI updates.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [Header("Starting Balances")]
        [SerializeField] private int startingGold   = 500;
        [SerializeField] private int startingGems   = 0;
        [SerializeField] private int startingShards = 0;

        private Dictionary<CurrencyType, int> _balances = new();

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
            LoadBalances();
        }

        // ─── Public API ────────────────────────────────────────────────────────

        /// <summary>Returns the current balance for the given currency.</summary>
        public int GetBalance(CurrencyType type)
        {
            return _balances.TryGetValue(type, out int amount) ? amount : 0;
        }

        /// <summary>Returns true if the player has at least <paramref name="amount"/> of the currency.</summary>
        public bool HasEnough(CurrencyType type, int amount)
        {
            return GetBalance(type) >= amount;
        }

        /// <summary>
        /// Attempts to spend <paramref name="amount"/> of <paramref name="type"/>.
        /// Returns false (and does nothing) if the balance is insufficient.
        /// </summary>
        public bool TrySpend(CurrencyType type, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");

            if (!HasEnough(type, amount))
            {
                GameEvents.RaisePurchaseAttempted(type, amount, false);
                return false;
            }

            _balances[type] -= amount;
            GameEvents.RaiseCurrencyChanged(type, _balances[type]);
            GameEvents.RaisePurchaseAttempted(type, amount, true);
            SaveBalances();
            return true;
        }

        /// <summary>Adds <paramref name="amount"/> to the player's balance.</summary>
        public void Add(CurrencyType type, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");

            _balances.TryAdd(type, 0);
            _balances[type] += amount;
            GameEvents.RaiseCurrencyChanged(type, _balances[type]);
            SaveBalances();
        }

        /// <summary>Directly sets a currency balance (use for loading save data).</summary>
        public void SetBalance(CurrencyType type, int amount)
        {
            _balances[type] = Mathf.Max(0, amount);
            GameEvents.RaiseCurrencyChanged(type, _balances[type]);
        }

        // ─── Persistence ───────────────────────────────────────────────────────

        private void LoadBalances()
        {
            var save = SaveSystem.Load();

            _balances[CurrencyType.Gold]   = save?.gold   ?? startingGold;
            _balances[CurrencyType.Gems]   = save?.gems   ?? startingGems;
            _balances[CurrencyType.Shards] = save?.shards ?? startingShards;

            foreach (CurrencyType t in Enum.GetValues(typeof(CurrencyType)))
                GameEvents.RaiseCurrencyChanged(t, _balances[t]);
        }

        private void SaveBalances()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.gold   = GetBalance(CurrencyType.Gold);
            save.gems   = GetBalance(CurrencyType.Gems);
            save.shards = GetBalance(CurrencyType.Shards);
            SaveSystem.Save(save);
        }
    }
}
