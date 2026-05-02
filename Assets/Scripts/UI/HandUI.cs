using UnityEngine;
using System.Collections.Generic;
using TCG.Cards;
using TCG.Core;
using TCG.Player;

namespace TCG.UI
{
    /// <summary>
    /// Manages the fan/row layout of a player's hand.
    /// </summary>
    public class HandUI : MonoBehaviour
    {
        [Header("References")]
        public CardUI cardUIPrefab;
        public Transform handContainer;
        public PlayerController playerController;

        [Header("Layout")]
        public float cardSpacing = 120f;
        public float hoverRaise = 30f;

        private List<CardUI> _cardUIs = new List<CardUI>();
        private CardUI _selectedCardUI;

        private void Start()
        {
            GameEvents.OnCardDrawn += OnCardDrawn;
            GameEvents.OnCardPlayed += OnCardRemoved;
            GameEvents.OnCardReturnedToHand += OnCardDrawn;
        }

        private void OnDestroy()
        {
            GameEvents.OnCardDrawn -= OnCardDrawn;
            GameEvents.OnCardPlayed -= OnCardRemoved;
            GameEvents.OnCardReturnedToHand -= OnCardDrawn;
        }

        private void OnCardDrawn(Card card, PlayerState player)
        {
            if (player != playerController.State) return;
            SpawnCardUI(card);
            LayoutHand();
        }

        private void OnCardRemoved(Card card, PlayerState player)
        {
            if (player != playerController.State) return;
            var ui = _cardUIs.Find(c => c.Card == card);
            if (ui == null) return;
            _cardUIs.Remove(ui);
            Destroy(ui.gameObject);
            LayoutHand();
        }

        private void SpawnCardUI(Card card)
        {
            var ui = Instantiate(cardUIPrefab, handContainer);
            ui.Bind(card);
            _cardUIs.Add(ui);
        }

        private void LayoutHand()
        {
            int count = _cardUIs.Count;
            float totalWidth = (count - 1) * cardSpacing;
            float startX = -totalWidth * 0.5f;

            for (int i = 0; i < count; i++)
            {
                var rt = _cardUIs[i].GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(startX + i * cardSpacing, 0);
            }
        }

        public void OnCardClicked(CardUI cardUI)
        {
            if (_selectedCardUI == cardUI)
            {
                // Second click on the same card = play it
                playerController.TryPlaySelectedCard();
                DeselectCard();
                return;
            }

            DeselectCard();
            _selectedCardUI = cardUI;
            cardUI.SetSelected(true);
            playerController.SelectCard(cardUI.Card);
        }

        private void DeselectCard()
        {
            _selectedCardUI?.SetSelected(false);
            _selectedCardUI = null;
        }

        public void RefreshAll()
        {
            foreach (var ui in _cardUIs) ui.Refresh();
        }
    }
}
