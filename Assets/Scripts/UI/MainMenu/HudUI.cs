using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Currency;
using TCG.Navigation;

namespace TCG.UI.MainMenu
{
    /// <summary>
    /// Persistent top-of-screen HUD showing currency balances and a contextual back button.
    ///
    /// The HUD is always active. The back button is hidden on the home panel and shown
    /// on every sub-panel so the player can navigate up the stack.
    ///
    /// Inspector setup:
    ///   • <see cref="goldText"/>, <see cref="gemsText"/>, <see cref="shardsText"/>
    ///       — TMP_Text labels that display each currency balance
    ///   • <see cref="backButton"/>  — button shown only on non-home panels; calls
    ///       <see cref="PanelNavigator.Back"/>
    ///   • <see cref="panelTitleText"/> — (optional) label that reflects the current panel name
    /// </summary>
    public class HudUI : MonoBehaviour
    {
        [Header("Currency")]
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text gemsText;
        [SerializeField] private TMP_Text shardsText;

        [Header("Navigation")]
        [SerializeField] private Button   backButton;
        [SerializeField] private TMP_Text panelTitleText;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            backButton?.onClick.AddListener(OnBackClicked);
        }

        private void OnDestroy()
        {
            backButton?.onClick.RemoveListener(OnBackClicked);
        }

        private void OnEnable()
        {
            GameEvents.OnCurrencyChanged += OnCurrencyChanged;
            PanelNavigator.OnPanelChanged  += OnPanelChanged;
            RefreshAll();
        }

        private void OnDisable()
        {
            GameEvents.OnCurrencyChanged -= OnCurrencyChanged;
            PanelNavigator.OnPanelChanged  -= OnPanelChanged;
        }

        // ── Display ────────────────────────────────────────────────────────────────

        private void RefreshAll()
        {
            RefreshCurrency(CurrencyType.Gold,   CurrencyManager.Instance?.GetBalance(CurrencyType.Gold)   ?? 0);
            RefreshCurrency(CurrencyType.Gems,   CurrencyManager.Instance?.GetBalance(CurrencyType.Gems)   ?? 0);
            RefreshCurrency(CurrencyType.Shards, CurrencyManager.Instance?.GetBalance(CurrencyType.Shards) ?? 0);
            RefreshBackButton();
        }

        private void RefreshCurrency(CurrencyType type, int amount)
        {
            switch (type)
            {
                case CurrencyType.Gold:   if (goldText   != null) goldText.text   = amount.ToString("N0"); break;
                case CurrencyType.Gems:   if (gemsText   != null) gemsText.text   = amount.ToString("N0"); break;
                case CurrencyType.Shards: if (shardsText != null) shardsText.text = amount.ToString("N0"); break;
            }
        }

        private void RefreshBackButton()
        {
            if (backButton == null) return;
            bool onHome = PanelNavigator.Instance == null ||
                          PanelNavigator.Instance.CurrentPanel == PanelNavigator.HomeKey ||
                          PanelNavigator.Instance.CurrentPanel == PanelNavigator.MatchKey;
            backButton.gameObject.SetActive(!onHome);
        }

        private void RefreshPanelTitle(string key)
        {
            if (panelTitleText == null) return;
            panelTitleText.text = PanelKeyToTitle(key);
        }

        private static string PanelKeyToTitle(string key) => key switch
        {
            PanelNavigator.HomeKey       => string.Empty,
            PanelNavigator.MatchKey      => string.Empty,
            PanelNavigator.CampaignKey   => "Campaign",
            PanelNavigator.RankedKey     => "Ranked",
            PanelNavigator.QuickMatchKey => "Quick Match",
            PanelNavigator.CollectionKey => "Collection",
            PanelNavigator.ShopKey       => "Shop",
            PanelNavigator.GachaKey      => "Card Packs",
            PanelNavigator.CharactersKey => "Characters",
            PanelNavigator.QuestsKey     => "Quests",
            _                            => key,
        };

        // ── Event Callbacks ───────────────────────────────────────────────────────

        private void OnCurrencyChanged(CurrencyType type, int newAmount) =>
            RefreshCurrency(type, newAmount);

        private void OnPanelChanged(string _, string newKey)
        {
            RefreshBackButton();
            RefreshPanelTitle(newKey);
        }

        private void OnBackClicked() => PanelNavigator.Instance?.Back();
    }
}
