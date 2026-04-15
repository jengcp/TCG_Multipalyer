using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Ranked;

namespace TCG.UI.Ranked
{
    /// <summary>
    /// Post-match popup that animates the RP delta and shows the resulting rank.
    ///
    /// Subscribes to <see cref="GameEvents.OnRankedMatchResolved"/> and activates itself
    /// automatically. Dismissed by the player pressing <see cref="continueButton"/>.
    ///
    /// Inspector setup:
    ///   • <see cref="rpDeltaText"/>   — e.g. "+25 RP" or "−20 RP"
    ///   • <see cref="rankAfterBadge"/> — shows the new rank after applying the delta
    ///   • <see cref="promotionBanner"/> — GameObject shown when the player promoted (RANK UP!)
    ///   • <see cref="demotionBanner"/>  — GameObject shown when the player demoted (RANK DOWN)
    ///   • <see cref="continueButton"/>  — dismisses the popup
    ///   • <see cref="gainColor"/>  — color for positive RP delta
    ///   • <see cref="lossColor"/>  — color for negative RP delta
    /// </summary>
    public class RpChangePopupUI : MonoBehaviour
    {
        [Header("RP Delta")]
        [SerializeField] private TMP_Text rpDeltaText;
        [SerializeField] private Color    gainColor = new Color(0.3f, 1f, 0.4f);
        [SerializeField] private Color    lossColor = new Color(1f, 0.35f, 0.35f);

        [Header("Rank After")]
        [SerializeField] private RankBadgeUI rankAfterBadge;

        [Header("Rank Change Banners")]
        [SerializeField] private GameObject promotionBanner;
        [SerializeField] private GameObject demotionBanner;

        [Header("Animation")]
        [Tooltip("Duration of the scale-bounce animation on the RP delta text.")]
        [SerializeField] private float animDuration = 0.4f;

        [Header("Continue")]
        [SerializeField] private Button continueButton;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            gameObject.SetActive(false);
            continueButton?.onClick.AddListener(OnContinueClicked);
        }

        private void OnEnable()  => GameEvents.OnRankedMatchResolved += OnRankedMatchResolved;
        private void OnDisable() => GameEvents.OnRankedMatchResolved -= OnRankedMatchResolved;

        // ── Event Handler ─────────────────────────────────────────────────────────

        private void OnRankedMatchResolved(RankedMatchOutcome outcome)
        {
            gameObject.SetActive(true);
            PopulateFields(outcome);
            StartCoroutine(AnimateDeltaText());
        }

        // ── Display ────────────────────────────────────────────────────────────────

        private void PopulateFields(RankedMatchOutcome outcome)
        {
            // RP delta text
            if (rpDeltaText != null)
            {
                string sign  = outcome.RpDelta >= 0 ? "+" : string.Empty;
                rpDeltaText.text  = $"{sign}{outcome.RpDelta} RP";
                rpDeltaText.color = outcome.RpDelta >= 0 ? gainColor : lossColor;
            }

            // New rank badge
            rankAfterBadge?.Bind(outcome.NewTier, outcome.NewDivision, outcome.NewRP);

            // Promotion / demotion banners
            promotionBanner?.SetActive(outcome.Promoted);
            demotionBanner?.SetActive(outcome.Demoted);
        }

        // ── Animation ─────────────────────────────────────────────────────────────

        private IEnumerator AnimateDeltaText()
        {
            if (rpDeltaText == null) yield break;

            var transform = rpDeltaText.transform;
            float half    = animDuration * 0.5f;
            float elapsed = 0f;

            // Scale up
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / half;
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.35f, t);
                yield return null;
            }

            elapsed = 0f;

            // Scale back down
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / half;
                transform.localScale = Vector3.Lerp(Vector3.one * 1.35f, Vector3.one, t);
                yield return null;
            }

            transform.localScale = Vector3.one;
        }

        // ── Continue ──────────────────────────────────────────────────────────────

        private void OnContinueClicked()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }
}
