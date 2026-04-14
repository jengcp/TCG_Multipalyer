using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Items;

namespace TCG.UI.Gacha
{
    /// <summary>
    /// Shows one card pulled from a gacha result.
    /// The frame / background tints by rarity.
    /// </summary>
    public class GachaPullCardUI : MonoBehaviour
    {
        [SerializeField] private Image    cardArtwork;
        [SerializeField] private Image    rarityFrame;
        [SerializeField] private TMP_Text cardNameText;
        [SerializeField] private TMP_Text rarityText;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor    = Color.white;
        [SerializeField] private Color uncommonColor  = new Color(0.4f, 0.8f, 0.4f);
        [SerializeField] private Color rareColor      = new Color(0.4f, 0.6f, 1.0f);
        [SerializeField] private Color epicColor      = new Color(0.7f, 0.3f, 1.0f);
        [SerializeField] private Color legendaryColor = new Color(1.0f, 0.75f, 0.1f);

        // ── Setup ──────────────────────────────────────────────────────────────────

        public void Bind(CardData card)
        {
            if (card == null) return;

            if (cardArtwork != null)
                cardArtwork.sprite = card.cardArtwork != null ? card.cardArtwork : card.icon;

            if (cardNameText != null)
                cardNameText.text = card.displayName;

            if (rarityText != null)
                rarityText.text = card.rarity.ToString();

            var col = RarityToColor(card.rarity);
            if (rarityFrame != null)  rarityFrame.color = col;
            if (rarityText  != null)  rarityText.color  = col;
        }

        // ── Helper ─────────────────────────────────────────────────────────────────

        private Color RarityToColor(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Uncommon  => uncommonColor,
            ItemRarity.Rare      => rareColor,
            ItemRarity.Epic      => epicColor,
            ItemRarity.Legendary => legendaryColor,
            _                    => commonColor
        };
    }
}
