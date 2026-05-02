using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TCG.Cards;
using TCG.Core;

namespace TCG.UI
{
    /// <summary>
    /// Visual representation of a single card. Attach to a card prefab that
    /// has an Image (artwork), TextMeshPro labels, and a Button component.
    /// </summary>
    public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        public Image artworkImage;
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI manaCostText;
        public TextMeshProUGUI attackText;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI descriptionText;
        public Image rarityBorder;
        public Image elementIcon;
        public GameObject exhaustedOverlay;
        public GameObject selectionHighlight;

        [Header("Rarity Colors")]
        public Color commonColor = Color.white;
        public Color uncommonColor = Color.green;
        public Color rareColor = Color.blue;
        public Color legendaryColor = Color.yellow;

        public Card Card { get; private set; }

        private bool _isSelected;

        public void Bind(Card card)
        {
            Card = card;
            Refresh();
        }

        public void Refresh()
        {
            if (Card == null || Card.Data == null) return;

            var data = Card.Data;

            cardNameText.text = data.cardName;
            manaCostText.text = data.manaCost.ToString();
            artworkImage.sprite = data.artwork;

            if (data.IsCreature)
            {
                attackText.text = Card.CurrentAttack.ToString();
                healthText.text = Card.CurrentHealth.ToString();
                attackText.gameObject.SetActive(true);
                healthText.gameObject.SetActive(true);
            }
            else
            {
                attackText.gameObject.SetActive(false);
                healthText.gameObject.SetActive(false);
            }

            descriptionText.text = data.flavorText;

            rarityBorder.color = data.rarity switch
            {
                CardRarity.Common => commonColor,
                CardRarity.Uncommon => uncommonColor,
                CardRarity.Rare => rareColor,
                CardRarity.Legendary => legendaryColor,
                _ => commonColor
            };

            exhaustedOverlay.SetActive(Card.IsExhausted);
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            selectionHighlight.SetActive(selected);
        }

        // ── Input ──────────────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            HandUI parentHand = GetComponentInParent<HandUI>();
            parentHand?.OnCardClicked(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.localScale = Vector3.one * 1.08f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
        }
    }
}
