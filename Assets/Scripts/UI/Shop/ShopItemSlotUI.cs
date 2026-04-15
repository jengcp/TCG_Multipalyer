using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Shop;
using TCG.Items;

namespace TCG.UI.Shop
{
    /// <summary>
    /// Controls a single item tile in the shop grid.
    /// Bind via Inspector or instantiate from a prefab managed by ShopUI.
    /// </summary>
    public class ShopItemSlotUI : MonoBehaviour
    {
        [Header("Item Info")]
        [SerializeField] private Image      itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI quantityText;

        [Header("Pricing")]
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI originalPriceText; // shown with strikethrough when discounted
        [SerializeField] private Image           currencyIcon;
        [SerializeField] private Sprite[]        currencyIcons;    // index maps to CurrencyType enum

        [Header("Status Overlays")]
        [SerializeField] private GameObject soldOutOverlay;
        [SerializeField] private GameObject purchasedOverlay;
        [SerializeField] private GameObject badgeObject;
        [SerializeField] private TextMeshProUGUI badgeText;
        [SerializeField] private Image      discountBadge;
        [SerializeField] private TextMeshProUGUI discountText;

        [Header("Rarity Frame")]
        [SerializeField] private Image rarityFrame;
        [SerializeField] private Color[] rarityColors; // index maps to ItemRarity enum

        [Header("Interaction")]
        [SerializeField] private Button purchaseButton;

        private ShopItemListing _listing;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            purchaseButton?.onClick.AddListener(OnPurchaseClicked);
        }

        // ─── Public API ────────────────────────────────────────────────────────

        /// <summary>Populates all UI elements from the given listing data.</summary>
        public void Populate(ShopItemListing listing)
        {
            _listing = listing;
            if (_listing == null) { gameObject.SetActive(false); return; }

            var item = listing.item;

            // Icon & name
            if (itemIcon != null)       itemIcon.sprite = item?.icon;
            if (itemNameText != null)   itemNameText.text = item?.displayName ?? "Unknown";
            if (descriptionText != null) descriptionText.text = item?.description ?? string.Empty;

            // Quantity label (e.g. "x5" for bundles)
            if (quantityText != null)
            {
                quantityText.text = listing.quantityPerPurchase > 1 ? $"x{listing.quantityPerPurchase}" : string.Empty;
                quantityText.gameObject.SetActive(listing.quantityPerPurchase > 1);
            }

            // Rarity frame
            if (rarityFrame != null && item != null && rarityColors != null)
            {
                int rarityIndex = (int)item.rarity;
                rarityFrame.color = rarityIndex < rarityColors.Length ? rarityColors[rarityIndex] : Color.white;
            }

            // Price
            RefreshPrice(listing);

            // Badge
            bool hasBadge = !string.IsNullOrEmpty(listing.badgeText);
            if (badgeObject != null) badgeObject.SetActive(hasBadge);
            if (badgeText != null)   badgeText.text = listing.badgeText;

            // Availability
            RefreshAvailability(listing);
        }

        public void RefreshAvailability(ShopItemListing listing)
        {
            bool available = listing.IsAvailable;

            if (purchaseButton != null) purchaseButton.interactable = available;
            if (soldOutOverlay != null) soldOutOverlay.SetActive(!available && !listing.isPurchased);
            if (purchasedOverlay != null) purchasedOverlay.SetActive(listing.isPurchased);
        }

        // ─── Private Helpers ───────────────────────────────────────────────────

        private void RefreshPrice(ShopItemListing listing)
        {
            bool hasDiscount = listing.discountPercent > 0;

            if (priceText != null)
                priceText.text = listing.FinalPrice.ToString("N0");

            if (originalPriceText != null)
            {
                originalPriceText.gameObject.SetActive(hasDiscount);
                originalPriceText.text = listing.price.ToString("N0");
            }

            if (discountBadge != null) discountBadge.gameObject.SetActive(hasDiscount);
            if (discountText != null)  discountText.text = $"-{listing.discountPercent}%";

            if (currencyIcon != null && currencyIcons != null)
            {
                int idx = (int)listing.currency;
                currencyIcon.sprite = idx < currencyIcons.Length ? currencyIcons[idx] : null;
            }
        }

        private void OnPurchaseClicked()
        {
            if (_listing == null) return;

            var result = ShopManager.Instance?.TryPurchase(_listing);
            if (result == null) return;

            if (result.Value.IsSuccess)
                RefreshAvailability(_listing);
            else
                Debug.Log($"[ShopItemSlotUI] Purchase failed: {result.Value.Message}");
        }
    }
}
