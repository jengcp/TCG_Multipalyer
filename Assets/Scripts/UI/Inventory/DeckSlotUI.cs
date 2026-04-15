using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Inventory.Deck;
using TCG.Items;

namespace TCG.UI.Inventory
{
    /// <summary>
    /// Represents one card entry (name + count) inside the deck builder's deck list.
    /// </summary>
    public class DeckSlotUI : MonoBehaviour
    {
        public event Action<string> OnAddClicked;    // cardId
        public event Action<string> OnRemoveClicked; // cardId

        [SerializeField] private Image           cardIcon;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI maxCopiesText;
        [SerializeField] private Image           rarityBar;
        [SerializeField] private Color[]         rarityColors;
        [SerializeField] private Button          addButton;
        [SerializeField] private Button          removeButton;

        private DeckCardSlot _slot;

        private void Awake()
        {
            addButton?.onClick.AddListener(() => OnAddClicked?.Invoke(_slot?.Card?.itemId));
            removeButton?.onClick.AddListener(() => OnRemoveClicked?.Invoke(_slot?.Card?.itemId));
        }

        public void Populate(DeckCardSlot slot)
        {
            _slot = slot;
            if (slot == null) { gameObject.SetActive(false); return; }

            var card = slot.Card;
            if (cardIcon     != null) cardIcon.sprite    = card.icon;
            if (cardNameText != null) cardNameText.text  = card.displayName;
            if (countText    != null) countText.text      = $"x{slot.Count}";
            if (maxCopiesText != null)
                maxCopiesText.text = $"/{card.maxCopiesInDeck}";

            if (rarityBar != null && rarityColors != null)
            {
                int idx = (int)card.rarity;
                rarityBar.color = idx < rarityColors.Length ? rarityColors[idx] : Color.white;
            }

            // Disable add when at copy limit
            if (addButton != null)
                addButton.interactable = slot.Count < card.maxCopiesInDeck;
        }
    }
}
