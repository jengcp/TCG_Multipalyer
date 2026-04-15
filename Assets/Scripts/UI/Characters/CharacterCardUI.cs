using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Characters;

namespace TCG.UI.Characters
{
    /// <summary>
    /// A single character tile in the character shop grid.
    /// Shows portrait, name, gem cost, and owned/buy state.
    /// </summary>
    public class CharacterCardUI : MonoBehaviour
    {
        [SerializeField] private Image    portrait;
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Button   buyButton;
        [SerializeField] private TMP_Text buyButtonLabel;
        [SerializeField] private GameObject ownedBadge;     // "Owned" overlay shown when unlocked

        private CharacterData _data;

        // ── Setup ──────────────────────────────────────────────────────────────────

        public void Bind(CharacterData data)
        {
            _data = data;

            if (portrait != null && data.portrait != null)
                portrait.sprite = data.portrait;

            if (characterNameText != null)
                characterNameText.text = data.characterName;

            Refresh();

            buyButton?.onClick.RemoveAllListeners();
            buyButton?.onClick.AddListener(OnBuyClicked);
        }

        public void Refresh()
        {
            if (_data == null) return;

            bool owned = CharacterManager.Instance?.IsOwned(_data.characterId) ?? _data.isStarterCharacter;

            if (ownedBadge   != null) ownedBadge.SetActive(owned);
            if (buyButton    != null) buyButton.gameObject.SetActive(!owned);

            if (costText != null)
                costText.text = _data.isStarterCharacter ? "Free" : $"{_data.gemCost} Gems";

            if (buyButtonLabel != null)
                buyButtonLabel.text = $"Buy — {_data.gemCost} Gems";
        }

        // ── Handler ────────────────────────────────────────────────────────────────

        private void OnBuyClicked()
        {
            if (_data == null) return;
            bool success = CharacterManager.Instance?.TryPurchase(_data.characterId) ?? false;
            if (success) Refresh();
        }
    }
}
