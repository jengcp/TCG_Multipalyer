using UnityEngine;
using System.Collections.Generic;
using TCG.Cards;
using TCG.Core;
using TCG.Player;

namespace TCG.UI
{
    /// <summary>
    /// Manages cards on the battlefield for one player.
    /// </summary>
    public class FieldUI : MonoBehaviour
    {
        [Header("References")]
        public CardUI cardUIPrefab;
        public Transform fieldContainer;
        public PlayerController playerController;
        public bool isOpponentField;

        [Header("Layout")]
        public float cardSpacing = 140f;

        private List<CardUI> _cardUIs = new List<CardUI>();
        private CardUI _selectedAttackerUI;

        private void Start()
        {
            GameEvents.OnCardPlayed += OnCardPlayed;
            GameEvents.OnCardDestroyed += OnCardDestroyed;
            GameEvents.OnCreatureDamaged += OnCreatureDamaged;
        }

        private void OnDestroy()
        {
            GameEvents.OnCardPlayed -= OnCardPlayed;
            GameEvents.OnCardDestroyed -= OnCardDestroyed;
            GameEvents.OnCreatureDamaged -= OnCreatureDamaged;
        }

        private void OnCardPlayed(Card card, PlayerState player)
        {
            bool isOwnerField = (player == playerController.State) != isOpponentField;
            if (!isOwnerField || !card.Data.IsCreature) return;
            SpawnCardUI(card);
            LayoutField();
        }

        private void OnCardDestroyed(Card card, PlayerState player)
        {
            var ui = _cardUIs.Find(c => c.Card == card);
            if (ui == null) return;
            _cardUIs.Remove(ui);
            Destroy(ui.gameObject);
            LayoutField();
        }

        private void OnCreatureDamaged(Card card, int damage)
        {
            var ui = _cardUIs.Find(c => c.Card == card);
            ui?.Refresh();
        }

        private void SpawnCardUI(Card card)
        {
            var ui = Instantiate(cardUIPrefab, fieldContainer);
            ui.Bind(card);

            if (isOpponentField)
                ui.GetComponent<UnityEngine.UI.Button>()?.onClick.AddListener(() => OnOpponentCardClicked(ui));
            else
                ui.GetComponent<UnityEngine.UI.Button>()?.onClick.AddListener(() => OnFriendlyCardClicked(ui));

            _cardUIs.Add(ui);
        }

        private void LayoutField()
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

        private void OnFriendlyCardClicked(CardUI ui)
        {
            var gameUI = FindObjectOfType<GameUI>();

            // If a character ability targeting flow is active, resolve it
            if (gameUI != null && gameUI.IsTargeting)
            {
                gameUI.ConfirmTarget(ui.Card);
                return;
            }

            DeselectAttacker();
            _selectedAttackerUI = ui;
            ui.SetSelected(true);
            playerController.SelectAttacker(ui.Card);
        }

        private void OnOpponentCardClicked(CardUI ui)
        {
            var gameUI = FindObjectOfType<GameUI>();

            // Ability targeting can also aim at enemy creatures
            if (gameUI != null && gameUI.IsTargeting)
            {
                gameUI.ConfirmTarget(ui.Card);
                return;
            }

            playerController.DeclareAttackOnCreature(ui.Card);
        }

        private void DeselectAttacker()
        {
            _selectedAttackerUI?.SetSelected(false);
            _selectedAttackerUI = null;
        }

        public void RefreshAll()
        {
            foreach (var ui in _cardUIs) ui.Refresh();
        }
    }
}
