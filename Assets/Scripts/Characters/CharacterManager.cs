using System.Collections.Generic;
using UnityEngine;
using TCG.Core;
using TCG.Currency;
using TCG.Save;

namespace TCG.Characters
{
    /// <summary>
    /// Singleton that tracks which characters the player owns and handles purchases.
    /// Characters are bought with Gemstones (CurrencyType.Gems).
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        [Header("All Characters in the Game")]
        [SerializeField] private List<CharacterData> allCharacters = new();

        private readonly HashSet<string> _ownedIds = new();

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadOwned();
            UnlockStarters();
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Returns true if the player owns the character with the given id.</summary>
        public bool IsOwned(string characterId) => _ownedIds.Contains(characterId);

        /// <summary>All character definitions registered with this manager.</summary>
        public IReadOnlyList<CharacterData> AllCharacters => allCharacters.AsReadOnly();

        /// <summary>
        /// Attempts to purchase a character with Gemstones.
        /// Returns false if already owned, too expensive, or character not found.
        /// </summary>
        public bool TryPurchase(string characterId)
        {
            if (_ownedIds.Contains(characterId))
            {
                Debug.LogWarning($"[CharacterManager] Character '{characterId}' already owned.");
                return false;
            }

            var data = allCharacters.Find(c => c.characterId == characterId);
            if (data == null)
            {
                Debug.LogWarning($"[CharacterManager] Character '{characterId}' not found.");
                return false;
            }

            if (!CurrencyManager.Instance.TrySpend(CurrencyType.Gems, data.gemCost))
                return false;

            GrantCharacter(data);
            return true;
        }

        // ── Internal grant ─────────────────────────────────────────────────────────

        private void GrantCharacter(CharacterData data)
        {
            _ownedIds.Add(data.characterId);
            SaveOwned();
            GameEvents.RaiseCharacterUnlocked(data);
        }

        // ── Starters ───────────────────────────────────────────────────────────────

        private void UnlockStarters()
        {
            bool changed = false;
            foreach (var c in allCharacters)
            {
                if (c.isStarterCharacter && !_ownedIds.Contains(c.characterId))
                {
                    _ownedIds.Add(c.characterId);
                    changed = true;
                }
            }
            if (changed) SaveOwned();
        }

        // ── Persistence ────────────────────────────────────────────────────────────

        private void LoadOwned()
        {
            var save = SaveSystem.Load();
            if (save?.unlockedCharacterIds == null) return;
            foreach (var id in save.unlockedCharacterIds)
                _ownedIds.Add(id);
        }

        private void SaveOwned()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.unlockedCharacterIds = new List<string>(_ownedIds);
            SaveSystem.Save(save);
        }
    }
}
