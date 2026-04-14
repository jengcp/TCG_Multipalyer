using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Match;
using TCG.Core;

namespace TCG.UI.Match
{
    /// <summary>
    /// Visual representation of one creature currently on the battlefield.
    /// Subscribes to OnDamageDealt and OnAttackDeclared to refresh its own card only.
    /// </summary>
    public class CardInPlayUI : MonoBehaviour
    {
        [SerializeField] private Image    cardArtwork;
        [SerializeField] private TMP_Text cardNameText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image    tappedOverlay;   // semi-transparent tint when tapped

        private CardInstance _card;

        // ── Setup ──────────────────────────────────────────────────────────────────

        public void Bind(CardInstance card)
        {
            if (_card != null) Unsubscribe();

            _card = card;
            Refresh();
            Subscribe();
        }

        public void Unbind()
        {
            Unsubscribe();
            _card = null;
        }

        // ── Event subscriptions ────────────────────────────────────────────────────

        private void Subscribe()
        {
            GameEvents.OnDamageDealt      += OnDamageDealt;
            GameEvents.OnAttackDeclared   += OnAttackDeclared;
            GameEvents.OnCreatureDied     += OnCreatureDied;
        }

        private void Unsubscribe()
        {
            GameEvents.OnDamageDealt      -= OnDamageDealt;
            GameEvents.OnAttackDeclared   -= OnAttackDeclared;
            GameEvents.OnCreatureDied     -= OnCreatureDied;
        }

        private void OnDestroy() => Unsubscribe();

        // ── Event handlers ─────────────────────────────────────────────────────────

        private void OnDamageDealt(int damage, CardInstance target, int targetPlayerIdx)
        {
            if (target != _card) return;
            Refresh();
        }

        private void OnAttackDeclared(CardInstance attacker, int attackerPlayerIdx)
        {
            if (attacker != _card) return;
            UpdateTappedState();
        }

        private void OnCreatureDied(CardInstance card, int playerIdx)
        {
            if (card != _card) return;
            Unsubscribe();
        }

        // ── Display ────────────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (_card == null) return;

            var data = _card.BaseData;

            if (cardArtwork != null && data.cardArtwork != null)
                cardArtwork.sprite = data.cardArtwork;

            if (cardNameText != null)
                cardNameText.text = data.name;

            if (attackText  != null) attackText.text  = _card.CurrentAttack.ToString();
            if (defenseText != null) defenseText.text = _card.CurrentDefense.ToString();
            if (healthText  != null) healthText.text  = _card.CurrentHealth.ToString();

            UpdateTappedState();
        }

        private void UpdateTappedState()
        {
            if (tappedOverlay != null)
                tappedOverlay.enabled = _card != null && _card.IsTapped;
        }
    }
}
