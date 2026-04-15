using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Match;

namespace TCG.UI.Match
{
    /// <summary>
    /// Displays one player's health (slider + text), mana count, and deck size.
    /// Call <see cref="Refresh"/> each time PlayerState changes.
    /// </summary>
    public class PlayerStatusUI : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private Slider  healthSlider;
        [SerializeField] private TMP_Text healthText;

        [Header("Mana")]
        [SerializeField] private TMP_Text manaText;

        [Header("Deck")]
        [SerializeField] private TMP_Text deckCountText;

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Refreshes all values from the given PlayerState.</summary>
        public void Refresh(PlayerState state)
        {
            if (state == null) return;

            UpdateHealth(state.CurrentHealth);
            UpdateMana(state.CurrentMana, state.MaxMana);
            UpdateDeckCount(state.Deck.Count);
        }

        /// <summary>Animates only the health display (called from OnPlayerHealthChanged events).</summary>
        public void UpdateHealth(int currentHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = PlayerState.StartingHealth;
                healthSlider.value    = currentHealth;
            }

            if (healthText != null)
                healthText.text = currentHealth.ToString();
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        private void UpdateMana(int current, int max)
        {
            if (manaText != null)
                manaText.text = $"{current}/{max}";
        }

        private void UpdateDeckCount(int count)
        {
            if (deckCountText != null)
                deckCountText.text = count.ToString();
        }
    }
}
