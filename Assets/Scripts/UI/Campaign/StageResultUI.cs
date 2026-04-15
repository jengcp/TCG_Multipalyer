using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TCG.Campaign;
using TCG.Core;

namespace TCG.UI.Campaign
{
    /// <summary>
    /// Post-match result screen for campaign stages.
    /// Shows match outcome, animated star reveal, card rewards, and gem bonus.
    ///
    /// Subscribes to <see cref="GameEvents.OnCampaignStageCompleted"/>.
    /// Continues back to the campaign map scene.
    /// </summary>
    public class StageResultUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;

        [Header("Outcome")]
        [SerializeField] private TMP_Text   outcomeText;
        [SerializeField] private Color      winColor  = new Color(0.3f, 0.85f, 0.35f);
        [SerializeField] private Color      lossColor = new Color(0.85f, 0.3f, 0.3f);

        [Header("Stars")]
        [SerializeField] private Image[]  starImages;       // 3 star slots
        [SerializeField] private Sprite   starFilledSprite;
        [SerializeField] private Sprite   starEmptySprite;
        [SerializeField] private float    starRevealDelay = 0.5f;

        [Header("Card Rewards")]
        [SerializeField] private Transform rewardCardContainer;
        [SerializeField] private Image     rewardCardPrefab;   // shows card artwork

        [Header("Gem Reward")]
        [SerializeField] private GameObject gemRewardGroup;
        [SerializeField] private TMP_Text   gemRewardText;

        [Header("Continue")]
        [SerializeField] private Button    continueButton;
        [SerializeField] private string    mapSceneName = "CampaignMap";

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            panel?.SetActive(false);
            continueButton?.onClick.AddListener(OnContinueClicked);
        }

        private void OnEnable()  => GameEvents.OnCampaignStageCompleted += OnStageCompleted;
        private void OnDisable() => GameEvents.OnCampaignStageCompleted -= OnStageCompleted;

        private void OnDestroy() => continueButton?.onClick.RemoveListener(OnContinueClicked);

        // ── Event handler ──────────────────────────────────────────────────────────

        private void OnStageCompleted(CampaignStageResult result)
        {
            panel?.SetActive(true);
            StartCoroutine(AnimateResult(result));
        }

        // ── Animation ─────────────────────────────────────────────────────────────

        private IEnumerator AnimateResult(CampaignStageResult result)
        {
            // Outcome text
            if (outcomeText != null)
            {
                outcomeText.text  = result.MatchWon ? "Victory!" : "Defeat";
                outcomeText.color = result.MatchWon ? winColor : lossColor;
            }

            // Stars: show empty first, then fill one by one
            for (int i = 0; i < starImages?.Length; i++)
                if (starImages[i] != null) starImages[i].sprite = starEmptySprite;

            yield return new WaitForSeconds(0.3f);

            for (int i = 0; i < result.StarsEarned && i < starImages?.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].sprite = starFilledSprite;
                    // Punch scale animation (simple scale bounce)
                    StartCoroutine(PunchScale(starImages[i].transform));
                }
                yield return new WaitForSeconds(starRevealDelay);
            }

            // Card rewards
            ClearRewards();
            if (result.NewCardRewards != null && rewardCardPrefab != null && rewardCardContainer != null)
            {
                foreach (var card in result.NewCardRewards)
                {
                    if (card == null) continue;
                    var icon = Instantiate(rewardCardPrefab, rewardCardContainer);
                    if (card.cardArtwork != null) icon.sprite = card.cardArtwork;
                    else if (card.icon != null)   icon.sprite = card.icon;
                }
            }

            // Gem reward
            if (gemRewardGroup != null)
                gemRewardGroup.SetActive(result.FullStarBonusEarned && result.GemsEarned > 0);

            if (gemRewardText != null && result.FullStarBonusEarned)
                gemRewardText.text = $"+{result.GemsEarned} Gems";

            // Activate continue button
            if (continueButton != null) continueButton.interactable = true;
        }

        private IEnumerator PunchScale(Transform t)
        {
            Vector3 original = t.localScale;
            t.localScale     = original * 1.4f;
            float elapsed    = 0f;
            float duration   = 0.2f;

            while (elapsed < duration)
            {
                t.localScale = Vector3.Lerp(original * 1.4f, original, elapsed / duration);
                elapsed     += Time.deltaTime;
                yield return null;
            }

            t.localScale = original;
        }

        private void ClearRewards()
        {
            if (rewardCardContainer == null) return;
            foreach (Transform child in rewardCardContainer)
                Destroy(child.gameObject);
        }

        // ── Continue ───────────────────────────────────────────────────────────────

        private void OnContinueClicked()
        {
            SceneManager.LoadScene(mapSceneName);
        }
    }
}
