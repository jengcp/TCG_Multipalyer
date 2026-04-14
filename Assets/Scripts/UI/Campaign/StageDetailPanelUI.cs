using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Campaign;
using TCG.Inventory.Deck;

namespace TCG.UI.Campaign
{
    /// <summary>
    /// Slide-up panel that shows a stage's info when the player taps a hex node.
    ///
    /// Displays: stage name, description, star criteria, card rewards per star,
    /// gemstone bonus, current star progress, and a Play button.
    ///
    /// Animates in/out using the "Open" Animator bool parameter.
    /// </summary>
    public class StageDetailPanelUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private Animator  animator;
        [SerializeField] private Button    closeButton;

        [Header("Stage Info")]
        [SerializeField] private TMP_Text  stageNameText;
        [SerializeField] private TMP_Text  descriptionText;

        [Header("Stars")]
        [SerializeField] private Image[]   starImages;        // 3 slots
        [SerializeField] private Sprite    starFilledSprite;
        [SerializeField] private Sprite    starEmptySprite;
        [SerializeField] private TMP_Text[] criteriaLabels;  // 3 text labels for criteria descriptions

        [Header("Rewards")]
        [SerializeField] private Transform  rewardContainer;
        [SerializeField] private Image      rewardCardPrefab; // shows card icon
        [SerializeField] private TMP_Text   gemRewardText;

        [Header("Play")]
        [SerializeField] private Button     playButton;
        [SerializeField] private TMP_Text   playButtonText;

        // ── Runtime ────────────────────────────────────────────────────────────────

        private CampaignStageData          _stage;
        private static readonly int        OpenParam = Animator.StringToHash("Open");

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            closeButton?.onClick.AddListener(Hide);
            playButton?.onClick.AddListener(OnPlayClicked);

            if (animator != null) animator.SetBool(OpenParam, false);
        }

        private void OnDestroy()
        {
            closeButton?.onClick.RemoveListener(Hide);
            playButton?.onClick.RemoveListener(OnPlayClicked);
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        public void Show(CampaignStageData stage)
        {
            _stage = stage;
            Refresh();
            if (animator != null) animator.SetBool(OpenParam, true);
        }

        public void Hide()
        {
            if (animator != null) animator.SetBool(OpenParam, false);
        }

        // ── Display ────────────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (_stage == null) return;

            if (stageNameText  != null) stageNameText.text  = _stage.stageName;
            if (descriptionText != null) descriptionText.text = _stage.description;

            // Star criteria descriptions
            for (int i = 0; i < 3; i++)
            {
                if (criteriaLabels == null || i >= criteriaLabels.Length || criteriaLabels[i] == null) continue;
                if (_stage.starCriteria != null && i < _stage.starCriteria.Length)
                    criteriaLabels[i].text = $"★{i + 1} {_stage.starCriteria[i].description}";
                else
                    criteriaLabels[i].text = string.Empty;
            }

            // Current star fill
            int earned = CampaignManager.Instance?.GetStars(_stage.stageId) ?? 0;
            for (int i = 0; i < starImages?.Length; i++)
            {
                if (starImages[i] == null) continue;
                starImages[i].sprite = i < earned ? starFilledSprite : starEmptySprite;
            }

            // Reward icons (one per star slot)
            ClearRewards();
            if (_stage.starCardRewards != null && rewardCardPrefab != null && rewardContainer != null)
            {
                foreach (var card in _stage.starCardRewards)
                {
                    if (card == null) continue;
                    var icon = Instantiate(rewardCardPrefab, rewardContainer);
                    if (card.icon != null) icon.sprite = card.icon;
                }
            }

            // Gem reward label
            if (gemRewardText != null)
                gemRewardText.text = _stage.gemstoneOnFullStars > 0
                    ? $"All 3 stars: +{_stage.gemstoneOnFullStars} Gems"
                    : string.Empty;

            // Play button state
            var status = CampaignManager.Instance?.GetStageStatus(_stage.stageId) ?? StageStatus.Locked;
            if (playButton != null) playButton.interactable = status != StageStatus.Locked;
            if (playButtonText != null)
                playButtonText.text = earned > 0 ? "Replay" : "Play";
        }

        private void ClearRewards()
        {
            if (rewardContainer == null) return;
            foreach (Transform child in rewardContainer)
                Destroy(child.gameObject);
        }

        // ── Play handler ───────────────────────────────────────────────────────────

        private void OnPlayClicked()
        {
            if (_stage == null || CampaignManager.Instance == null) return;
            Hide();
            CampaignManager.Instance.StartStage(_stage);
        }
    }
}
