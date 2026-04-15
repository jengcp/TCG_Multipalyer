using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TCG.Core;
using TCG.Inventory;

namespace TCG.UI.Inventory
{
    /// <summary>
    /// Controls a single slot tile in the inventory grid.
    /// Supports click-to-inspect and drag-to-move (via Unity's IPointerHandler interfaces).
    /// </summary>
    public class InventorySlotUI : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Visuals")]
        [SerializeField] private Image           itemIcon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image           rarityBorder;
        [SerializeField] private Color[]         rarityColors;   // indexed by ItemRarity
        [SerializeField] private GameObject      emptyOverlay;
        [SerializeField] private GameObject      selectionHighlight;

        [Header("Tooltip")]
        [SerializeField] private InventoryItemTooltipUI tooltip;

        private InventoryItem _item;
        private bool          _selected;

        // ─── Public API ────────────────────────────────────────────────────────

        public InventoryItem Item => _item;

        public void Populate(InventoryItem item)
        {
            _item = item;
            bool hasItem = item != null && item.IsValid();

            if (emptyOverlay != null) emptyOverlay.SetActive(!hasItem);
            if (itemIcon     != null) itemIcon.sprite = hasItem ? item.itemData.icon : null;
            if (itemIcon     != null) itemIcon.enabled = hasItem;

            if (quantityText != null)
            {
                quantityText.text = hasItem && item.itemData.isStackable && item.quantity > 1
                    ? item.quantity.ToString()
                    : string.Empty;
            }

            if (rarityBorder != null && hasItem && rarityColors != null)
            {
                int idx = (int)item.itemData.rarity;
                rarityBorder.color = idx < rarityColors.Length ? rarityColors[idx] : Color.white;
            }

            SetSelected(false);
        }

        public void Clear() => Populate(null);

        public void SetSelected(bool selected)
        {
            _selected = selected;
            if (selectionHighlight != null) selectionHighlight.SetActive(selected);
        }

        // ─── Pointer Events ────────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData data)
        {
            if (_item == null || !_item.IsValid()) return;

            SetSelected(!_selected);
            if (_selected)
                GameEvents.RaiseItemInspected(_item);
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if (_item == null || !_item.IsValid()) return;
            tooltip?.Show(_item, transform.position);
        }

        public void OnPointerExit(PointerEventData data)
        {
            tooltip?.Hide();
        }
    }
}
