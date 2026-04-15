using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TCG.Characters;
using TCG.Core;
using TCG.Currency;

namespace TCG.UI.Characters
{
    /// <summary>
    /// Character shop grid. Displays all characters (owned + for sale).
    /// Refreshes on gem balance change and on character unlock events.
    /// </summary>
    public class CharacterShopUI : MonoBehaviour
    {
        [SerializeField] private CharacterCardUI  cardPrefab;
        [SerializeField] private Transform        gridContainer;
        [SerializeField] private TMP_Text         gemBalanceText;

        private readonly List<CharacterCardUI> _pool = new();

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnCharacterUnlocked  += OnCharacterUnlocked;
            GameEvents.OnCurrencyChanged    += OnCurrencyChanged;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnCharacterUnlocked  -= OnCharacterUnlocked;
            GameEvents.OnCurrencyChanged    -= OnCurrencyChanged;
        }

        // ── Display ────────────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (CharacterManager.Instance == null) return;

            var characters = CharacterManager.Instance.AllCharacters;
            EnsurePoolSize(characters.Count);

            for (int i = 0; i < _pool.Count; i++)
            {
                bool active = i < characters.Count;
                _pool[i].gameObject.SetActive(active);
                if (active) _pool[i].Bind(characters[i]);
            }

            UpdateGemBalance();
        }

        private void UpdateGemBalance()
        {
            if (gemBalanceText == null || CurrencyManager.Instance == null) return;
            int balance = CurrencyManager.Instance.GetBalance(CurrencyType.Gems);
            gemBalanceText.text = $"{balance} Gems";
        }

        // ── Event handlers ─────────────────────────────────────────────────────────

        private void OnCharacterUnlocked(CharacterData _)
        {
            // Refresh all tiles so owned status updates
            foreach (var tile in _pool)
                if (tile.gameObject.activeSelf) tile.Refresh();
        }

        private void OnCurrencyChanged(CurrencyType type, int newAmount)
        {
            if (type == CurrencyType.Gems) UpdateGemBalance();
        }

        // ── Pool ───────────────────────────────────────────────────────────────────

        private void EnsurePoolSize(int required)
        {
            while (_pool.Count < required)
            {
                var tile = Instantiate(cardPrefab, gridContainer);
                tile.gameObject.SetActive(false);
                _pool.Add(tile);
            }
        }
    }
}
