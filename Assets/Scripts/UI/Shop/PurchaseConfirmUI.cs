using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Shop;

namespace TCG.UI.Shop
{
    /// <summary>
    /// Modal confirmation dialog shown before a purchase is finalised.
    /// Call Show() with the listing and callbacks; the panel manages its own visibility.
    /// </summary>
    public class PurchaseConfirmUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject     panel;
        [SerializeField] private Image          itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI confirmMessageText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button         confirmButton;
        [SerializeField] private Button         cancelButton;

        private Action _onConfirm;
        private Action _onCancel;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            confirmButton?.onClick.AddListener(OnConfirmClicked);
            cancelButton?.onClick.AddListener(OnCancelClicked);
            Hide();
        }

        // ─── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Displays the confirmation dialog for the given listing.
        /// <paramref name="onConfirm"/> is invoked when the player clicks Confirm.
        /// <paramref name="onCancel"/> is invoked when the player clicks Cancel or closes.
        /// </summary>
        public void Show(ShopItemListing listing, Action onConfirm, Action onCancel = null)
        {
            if (listing == null) return;

            _onConfirm = onConfirm;
            _onCancel  = onCancel;

            if (itemIcon != null)
                itemIcon.sprite = listing.item?.icon;

            if (itemNameText != null)
                itemNameText.text = listing.item?.displayName ?? "Unknown Item";

            if (confirmMessageText != null)
                confirmMessageText.text = listing.quantityPerPurchase > 1
                    ? $"Buy {listing.quantityPerPurchase}x {listing.item?.displayName}?"
                    : $"Buy {listing.item?.displayName}?";

            if (priceText != null)
                priceText.text = $"{listing.FinalPrice:N0} {listing.currency}";

            if (panel != null) panel.SetActive(true);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
            _onConfirm = null;
            _onCancel  = null;
        }

        // ─── Button Handlers ───────────────────────────────────────────────────

        private void OnConfirmClicked()
        {
            var cb = _onConfirm;
            Hide();
            cb?.Invoke();
        }

        private void OnCancelClicked()
        {
            var cb = _onCancel;
            Hide();
            cb?.Invoke();
        }
    }
}
