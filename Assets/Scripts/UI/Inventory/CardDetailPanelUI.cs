using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Inventory;
using TCG.Items;

namespace TCG.UI.Inventory
{
    /// <summary>
    /// Large card detail panel shown when a card is selected in the inventory.
    /// Subscribes to GameEvents.OnItemInspected and auto-populates.
    /// </summary>
    public class CardDetailPanelUI : MonoBehaviour
    {
        [Header("Panel Root")]
        [SerializeField] private GameObject panel;

        [Header("Common Fields")]
        [SerializeField] private Image           itemIcon;
        [SerializeField] private Image           rarityGem;
        [SerializeField] private Color[]         rarityColors;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI ownedCountText;

        [Header("Card-Specific")]
        [SerializeField] private GameObject      cardStatsRoot;
        [SerializeField] private Image           cardArtwork;
        [SerializeField] private TextMeshProUGUI elementText;
        [SerializeField] private TextMeshProUGUI cardClassText;
        [SerializeField] private TextMeshProUGUI manaCostText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI defenseText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private TextMeshProUGUI flavorText;
        [SerializeField] private TextMeshProUGUI cardSetText;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            closeButton?.onClick.AddListener(Hide);
            Hide();
        }

        private void OnEnable()  => GameEvents.OnItemInspected += OnItemInspected;
        private void OnDisable() => GameEvents.OnItemInspected -= OnItemInspected;

        // ─── Handlers ─────────────────────────────────────────────────────────

        private void OnItemInspected(InventoryItem item)
        {
            if (item == null || !item.IsValid()) { Hide(); return; }
            Populate(item);
            Show();
        }

        // ─── Public API ────────────────────────────────────────────────────────

        public void Show() { if (panel != null) panel.SetActive(true); }
        public void Hide() { if (panel != null) panel.SetActive(false); }

        // ─── Populate ─────────────────────────────────────────────────────────

        private void Populate(InventoryItem item)
        {
            var data = item.itemData;

            // Common
            if (itemIcon        != null) itemIcon.sprite = data.icon;
            if (nameText        != null) nameText.text   = data.displayName;
            if (rarityText      != null) rarityText.text = data.rarity.ToString();
            if (typeText        != null) typeText.text   = data.itemType.ToString();
            if (descriptionText != null) descriptionText.text = data.description;
            if (ownedCountText  != null) ownedCountText.text  = $"Owned: {item.quantity}";

            if (rarityGem != null && rarityColors != null)
            {
                int idx = (int)data.rarity;
                rarityGem.color = idx < rarityColors.Length ? rarityColors[idx] : Color.white;
            }

            // Card-specific section
            bool isCard = data is CardData;
            if (cardStatsRoot != null) cardStatsRoot.SetActive(isCard);

            if (isCard && data is CardData card)
            {
                if (cardArtwork   != null) cardArtwork.sprite = card.cardArtwork;
                if (elementText   != null) elementText.text   = card.element.ToString();
                if (cardClassText != null) cardClassText.text  = card.cardClass.ToString();
                if (manaCostText  != null) manaCostText.text   = card.manaCost.ToString();
                if (attackText    != null) attackText.text     = card.attackPower.ToString();
                if (defenseText   != null) defenseText.text    = card.defensePower.ToString();
                if (healthText    != null) healthText.text     = card.healthPoints.ToString();
                if (effectText    != null) effectText.text     = card.effectText;
                if (flavorText    != null) flavorText.text     = card.flavorText;
                if (cardSetText   != null) cardSetText.text    = card.cardSet;
            }
        }
    }
}
