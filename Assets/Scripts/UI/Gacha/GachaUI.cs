using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Currency;
using TCG.Gacha;

namespace TCG.UI.Gacha
{
    /// <summary>
    /// Main gacha screen. Supports multiple pools (banners) with a tab/selector.
    /// Shows gem balance, pull costs, pity counter, single/multi pull buttons.
    ///
    /// Assign <see cref="pools"/> and connect buttons in the Inspector.
    /// </summary>
    public class GachaUI : MonoBehaviour
    {
        [Header("Pools / Banners")]
        [SerializeField] private GachaPoolData[] pools;
        [SerializeField] private Image           bannerImage;
        [SerializeField] private TMP_Text        poolNameText;
        [SerializeField] private Button          prevPoolButton;
        [SerializeField] private Button          nextPoolButton;

        [Header("Currency Display")]
        [SerializeField] private TMP_Text gemBalanceText;

        [Header("Pull Buttons")]
        [SerializeField] private Button    singlePullButton;
        [SerializeField] private TMP_Text  singleCostText;
        [SerializeField] private Button    multiPullButton;
        [SerializeField] private TMP_Text  multiCostText;

        [Header("Pity Counter")]
        [SerializeField] private TMP_Text  pityText;

        [Header("Result Panel")]
        [SerializeField] private GachaPullResultUI resultUI;

        // ── Runtime ────────────────────────────────────────────────────────────────

        private int _selectedPoolIndex;

        private GachaPoolData CurrentPool => (pools != null && pools.Length > 0)
            ? pools[_selectedPoolIndex] : null;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            singlePullButton?.onClick.AddListener(OnSinglePull);
            multiPullButton?.onClick.AddListener(OnMultiPull);
            prevPoolButton?.onClick.AddListener(PrevPool);
            nextPoolButton?.onClick.AddListener(NextPool);
        }

        private void OnEnable()
        {
            GameEvents.OnCurrencyChanged += OnCurrencyChanged;
            RefreshUI();
        }

        private void OnDisable()
        {
            GameEvents.OnCurrencyChanged -= OnCurrencyChanged;
        }

        private void OnDestroy()
        {
            singlePullButton?.onClick.RemoveListener(OnSinglePull);
            multiPullButton?.onClick.RemoveListener(OnMultiPull);
            prevPoolButton?.onClick.RemoveListener(PrevPool);
            nextPoolButton?.onClick.RemoveListener(NextPool);
        }

        // ── Pool navigation ────────────────────────────────────────────────────────

        private void PrevPool()
        {
            if (pools == null || pools.Length == 0) return;
            _selectedPoolIndex = (_selectedPoolIndex - 1 + pools.Length) % pools.Length;
            RefreshUI();
        }

        private void NextPool()
        {
            if (pools == null || pools.Length == 0) return;
            _selectedPoolIndex = (_selectedPoolIndex + 1) % pools.Length;
            RefreshUI();
        }

        // ── Pull handlers ──────────────────────────────────────────────────────────

        private void OnSinglePull()
        {
            var pool = CurrentPool;
            if (pool == null || GachaManager.Instance == null) return;

            var card = GachaManager.Instance.PullSingle(pool);
            if (card != null)
                resultUI?.Show(new System.Collections.Generic.List<TCG.Items.CardData> { card });

            RefreshUI();
        }

        private void OnMultiPull()
        {
            var pool = CurrentPool;
            if (pool == null || GachaManager.Instance == null) return;

            var cards = GachaManager.Instance.PullMulti(pool);
            if (cards != null && cards.Count > 0)
                resultUI?.Show(cards);

            RefreshUI();
        }

        // ── Display ────────────────────────────────────────────────────────────────

        private void RefreshUI()
        {
            var pool = CurrentPool;

            // Banner
            if (bannerImage != null && pool?.bannerArt != null)
                bannerImage.sprite = pool.bannerArt;

            if (poolNameText != null)
                poolNameText.text = pool?.poolName ?? "No Pools";

            // Nav buttons
            if (prevPoolButton != null) prevPoolButton.interactable = pools != null && pools.Length > 1;
            if (nextPoolButton != null) nextPoolButton.interactable = pools != null && pools.Length > 1;

            // Gem balance
            int balance = CurrencyManager.Instance?.GetBalance(TCG.Currency.CurrencyType.Gems) ?? 0;
            if (gemBalanceText != null) gemBalanceText.text = $"{balance} Gems";

            if (pool == null) return;

            // Costs
            if (singleCostText != null) singleCostText.text = $"{pool.singlePullCost} Gems";
            if (multiCostText  != null) multiCostText.text  = $"{pool.multiPullCost} Gems ({pool.multiPullCount}x)";

            // Button interactability
            bool canSingle = GachaManager.Instance?.CanAffordSingle(pool) ?? false;
            bool canMulti  = GachaManager.Instance?.CanAffordMulti(pool)  ?? false;

            if (singlePullButton != null) singlePullButton.interactable = canSingle;
            if (multiPullButton  != null) multiPullButton.interactable  = canMulti;

            // Pity counter
            if (pityText != null)
            {
                int until = GachaManager.Instance?.GetPullsUntilRarePity(pool) ?? pool.rarePityThreshold;
                pityText.text = $"Guaranteed Rare in {until} pulls";
            }
        }

        private void OnCurrencyChanged(TCG.Currency.CurrencyType type, int newAmount)
        {
            if (type == TCG.Currency.CurrencyType.Gems)
                RefreshUI();
        }
    }
}
