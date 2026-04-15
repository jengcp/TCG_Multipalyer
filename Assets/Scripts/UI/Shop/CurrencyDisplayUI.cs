using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Currency;

namespace TCG.UI.Shop
{
    /// <summary>
    /// Displays a single currency's balance and animates changes.
    /// Place one per currency type in the shop HUD.
    /// </summary>
    public class CurrencyDisplayUI : MonoBehaviour
    {
        [Header("Target Currency")]
        [SerializeField] private CurrencyType currencyType;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private Image           currencyIcon;
        [SerializeField] private Animator        changeAnimator; // plays "Gain" or "Spend" clips

        private static readonly int GainTrigger  = Animator.StringToHash("Gain");
        private static readonly int SpendTrigger = Animator.StringToHash("Spend");

        private int _previousBalance;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnCurrencyChanged += OnCurrencyChanged;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnCurrencyChanged -= OnCurrencyChanged;
        }

        // ─── Handlers ─────────────────────────────────────────────────────────

        private void OnCurrencyChanged(CurrencyType type, int newAmount)
        {
            if (type != currencyType) return;

            if (changeAnimator != null)
            {
                if (newAmount > _previousBalance)
                    changeAnimator.SetTrigger(GainTrigger);
                else if (newAmount < _previousBalance)
                    changeAnimator.SetTrigger(SpendTrigger);
            }

            _previousBalance = newAmount;
            SetText(newAmount);
        }

        private void Refresh()
        {
            if (CurrencyManager.Instance == null) return;
            int balance = CurrencyManager.Instance.GetBalance(currencyType);
            _previousBalance = balance;
            SetText(balance);
        }

        private void SetText(int amount)
        {
            if (balanceText != null)
                balanceText.text = amount.ToString("N0");
        }
    }
}
