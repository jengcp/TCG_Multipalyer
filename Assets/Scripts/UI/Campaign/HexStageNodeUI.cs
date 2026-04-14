using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TCG.Campaign;

namespace TCG.UI.Campaign
{
    /// <summary>
    /// Visual representation of one hexagonal stage node on the campaign map.
    ///
    /// The hex shape is achieved through an Image component using a hexagon sprite
    /// (assign a hex-shaped sprite in the Inspector) or a polygon-masked panel.
    ///
    /// Layout: hex background → stage icon → star row (3 stars) → name label → lock overlay.
    /// Clicking an unlocked node opens <see cref="StageDetailPanelUI"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class HexStageNodeUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Visuals")]
        [SerializeField] private Image    hexBackground;
        [SerializeField] private Image    stageIcon;
        [SerializeField] private TMP_Text stageNameText;

        [Header("Stars")]
        [SerializeField] private Image[]  starImages;        // length 3
        [SerializeField] private Sprite   starFilledSprite;
        [SerializeField] private Sprite   starEmptySprite;

        [Header("State Overlays")]
        [SerializeField] private GameObject lockOverlay;     // active when Locked
        [SerializeField] private GameObject completeBadge;   // active when all 3 stars

        [Header("Colors")]
        [SerializeField] private Color lockedColor    = new Color(0.35f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color availableColor = new Color(0.9f,  0.85f, 0.5f,  1f);
        [SerializeField] private Color completedColor = new Color(0.45f, 0.85f, 0.45f, 1f);

        // ── Runtime ────────────────────────────────────────────────────────────────

        private CampaignStageData     _stage;
        private StageDetailPanelUI    _detailPanel;

        // ── Setup ──────────────────────────────────────────────────────────────────

        public void Bind(CampaignStageData stage, StageDetailPanelUI detailPanel)
        {
            _stage       = stage;
            _detailPanel = detailPanel;

            Refresh();
        }

        public void Refresh()
        {
            if (_stage == null) return;

            var status = CampaignManager.Instance?.GetStageStatus(_stage.stageId) ?? StageStatus.Locked;
            int stars  = CampaignManager.Instance?.GetStars(_stage.stageId) ?? 0;

            // Stage name
            if (stageNameText != null)
                stageNameText.text = _stage.stageName;

            // Stage icon
            if (stageIcon != null && _stage.stageIcon != null)
                stageIcon.sprite = _stage.stageIcon;

            // Background tint by status
            if (hexBackground != null)
                hexBackground.color = status switch
                {
                    StageStatus.Locked    => lockedColor,
                    StageStatus.Completed => completedColor,
                    _                     => availableColor
                };

            // Star display
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                starImages[i].sprite = i < stars ? starFilledSprite : starEmptySprite;
            }

            // Overlays
            lockOverlay?.SetActive(status == StageStatus.Locked);
            completeBadge?.SetActive(stars >= 3);
        }

        // ── IPointerClickHandler ───────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_stage == null) return;

            var status = CampaignManager.Instance?.GetStageStatus(_stage.stageId) ?? StageStatus.Locked;
            if (status == StageStatus.Locked) return;

            _detailPanel?.Show(_stage);
        }
    }
}
