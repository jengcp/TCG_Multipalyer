using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TCG.Match;

namespace TCG.UI.Match
{
    /// <summary>
    /// Visual tile for one card in the local player's hand.
    /// Dims to 50% alpha when the card is unaffordable.
    /// Clicking calls <see cref="LocalPlayerController.NotifyCardSelected"/>.
    /// </summary>
    public class CardInHandUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image    cardArtwork;
        [SerializeField] private TMP_Text cardNameText;
        [SerializeField] private TMP_Text manaCostText;
        [SerializeField] private TMP_Text cardTypeText;
        [SerializeField] private CanvasGroup canvasGroup;

        private CardInstance         _card;
        private LocalPlayerController _controller;
        private bool                 _isAffordable;

        // ── Setup ──────────────────────────────────────────────────────────────────

        public void Bind(CardInstance card, int currentMana, LocalPlayerController controller)
        {
            _card        = card;
            _controller  = controller;
            _isAffordable = card.BaseData.manaCost <= currentMana;

            Refresh();
        }

        /// <summary>Updates affordability dim without re-binding.</summary>
        public void SetAffordable(bool affordable)
        {
            _isAffordable = affordable;
            UpdateAlpha();
        }

        // ── IPointerClickHandler ───────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_card == null || _controller == null) return;
            _controller.NotifyCardSelected(_card);
        }

        // ── Display ────────────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (_card == null) return;

            var data = _card.BaseData;

            if (cardArtwork != null && data.cardArtwork != null)
                cardArtwork.sprite = data.cardArtwork;

            if (cardNameText != null) cardNameText.text = data.name;
            if (manaCostText != null) manaCostText.text = data.manaCost.ToString();
            if (cardTypeText != null) cardTypeText.text = data.cardClass.ToString();

            UpdateAlpha();
        }

        private void UpdateAlpha()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = _isAffordable ? 1f : 0.5f;
        }
    }
}
