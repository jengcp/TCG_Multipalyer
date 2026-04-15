using System;
using UnityEngine;
using TMPro;
using TCG.Core;
using TCG.Shop;

namespace TCG.UI.Shop
{
    /// <summary>
    /// Displays the countdown until the next shop rotation.
    /// Updates once per second.
    /// </summary>
    public class ShopTimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject      timerRoot;

        private float _updateInterval = 1f;
        private float _timer;

        private void OnEnable()
        {
            GameEvents.OnShopRefreshed += OnShopRefreshed;
            RefreshDisplay();
        }

        private void OnDisable()
        {
            GameEvents.OnShopRefreshed -= OnShopRefreshed;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = _updateInterval;
                RefreshDisplay();
            }
        }

        private void OnShopRefreshed() => RefreshDisplay();

        private void RefreshDisplay()
        {
            if (ShopManager.Instance == null) return;

            TimeSpan remaining = ShopManager.Instance.TimeUntilRefresh();

            bool showTimer = remaining.TotalSeconds > 0;
            if (timerRoot != null) timerRoot.SetActive(showTimer);
            if (!showTimer || timerText == null) return;

            if (remaining.TotalHours >= 1)
                timerText.text = $"{(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            else
                timerText.text = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }
}
