using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TCG.Items;

namespace TCG.UI.Gacha
{
    /// <summary>
    /// Displays the cards obtained from a gacha pull.
    /// Cards fly in one by one (or all at once for a 10-pull).
    /// Call <see cref="Show"/> with the pull results.
    /// </summary>
    public class GachaPullResultUI : MonoBehaviour
    {
        [SerializeField] private GameObject      panel;
        [SerializeField] private GachaPullCardUI cardPrefab;
        [SerializeField] private Transform       cardContainer;
        [SerializeField] private Button          collectButton;

        [Header("Animation")]
        [SerializeField] private float cardRevealInterval = 0.15f;

        private readonly List<GachaPullCardUI> _pool = new();

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            panel?.SetActive(false);
            collectButton?.onClick.AddListener(Hide);
        }

        private void OnDestroy() => collectButton?.onClick.RemoveListener(Hide);

        // ── Public API ─────────────────────────────────────────────────────────────

        public void Show(List<CardData> cards)
        {
            panel?.SetActive(true);
            collectButton?.gameObject.SetActive(false);
            StartCoroutine(RevealCards(cards));
        }

        public void Hide()
        {
            panel?.SetActive(false);
            foreach (var tile in _pool)
                tile.gameObject.SetActive(false);
        }

        // ── Animation ─────────────────────────────────────────────────────────────

        private IEnumerator RevealCards(List<CardData> cards)
        {
            EnsurePoolSize(cards.Count);

            // Hide all tiles first
            for (int i = 0; i < _pool.Count; i++)
                _pool[i].gameObject.SetActive(false);

            // Reveal with delay
            for (int i = 0; i < cards.Count; i++)
            {
                _pool[i].Bind(cards[i]);
                _pool[i].gameObject.SetActive(true);

                // Brief scale-in punch
                StartCoroutine(ScaleIn(_pool[i].transform));

                yield return new WaitForSeconds(cardRevealInterval);
            }

            // Show collect button after all revealed
            if (collectButton != null)
                collectButton.gameObject.SetActive(true);
        }

        private IEnumerator ScaleIn(Transform t)
        {
            t.localScale = Vector3.zero;
            float elapsed  = 0f;
            float duration = 0.18f;

            while (elapsed < duration)
            {
                float scale = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                t.localScale = Vector3.one * scale;
                elapsed     += Time.deltaTime;
                yield return null;
            }

            t.localScale = Vector3.one;
        }

        private void EnsurePoolSize(int required)
        {
            while (_pool.Count < required)
            {
                var tile = Instantiate(cardPrefab, cardContainer);
                tile.gameObject.SetActive(false);
                _pool.Add(tile);
            }
        }
    }
}
