using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Inventory;
using TCG.Items;

namespace TCG.UI.Inventory
{
    /// <summary>
    /// Floating tooltip that follows the cursor and shows item details.
    /// Attach to a Canvas-root GameObject; call Show/Hide from InventorySlotUI.
    /// </summary>
    public class InventoryItemTooltipUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject      panel;
        [SerializeField] private Image           itemIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI quantityText;

        [Header("Card-specific (hidden for non-cards)")]
        [SerializeField] private GameObject      cardStatsGroup;
        [SerializeField] private TextMeshProUGUI manaCostText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI defenseText;
        [SerializeField] private TextMeshProUGUI effectText;

        [Header("Offset from pointer")]
        [SerializeField] private Vector2 offset = new Vector2(16f, -16f);

        private RectTransform _rect;
        private Canvas        _canvas;

        private void Awake()
        {
            _rect   = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            Hide();
        }

        // ─── Public API ────────────────────────────────────────────────────────

        public void Show(InventoryItem item, Vector3 worldPosition)
        {
            if (item == null || !item.IsValid()) return;

            Populate(item);

            if (panel != null) panel.SetActive(true);
            PositionNear(worldPosition);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void Update()
        {
            if (panel != null && panel.activeSelf)
                PositionNear(Input.mousePosition);
        }

        // ─── Private Helpers ───────────────────────────────────────────────────

        private void Populate(InventoryItem item)
        {
            var data = item.itemData;

            if (itemIcon        != null) itemIcon.sprite = data.icon;
            if (nameText        != null) nameText.text   = data.displayName;
            if (rarityText      != null) rarityText.text = data.rarity.ToString();
            if (typeText        != null) typeText.text   = data.itemType.ToString();
            if (descriptionText != null) descriptionText.text = data.description;
            if (quantityText    != null) quantityText.text    = $"Owned: {item.quantity}";

            bool isCard = data is CardData;
            if (cardStatsGroup != null) cardStatsGroup.SetActive(isCard);

            if (isCard && data is CardData card)
            {
                if (manaCostText != null) manaCostText.text = $"Cost: {card.manaCost}";
                if (attackText   != null) attackText.text   = $"ATK: {card.attackPower}";
                if (defenseText  != null) defenseText.text  = $"DEF: {card.defensePower}";
                if (effectText   != null) effectText.text   = card.effectText;
            }
        }

        private void PositionNear(Vector3 screenPosition)
        {
            if (_rect == null || _canvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                screenPosition,
                _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
                out Vector2 localPoint);

            _rect.anchoredPosition = localPoint + offset;
        }
    }
}
