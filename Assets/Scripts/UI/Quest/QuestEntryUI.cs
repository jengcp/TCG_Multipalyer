using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Quest;

namespace TCG.UI.Quest
{
    /// <summary>
    /// Represents one quest row in the quest panel.
    /// Shows quest name, all objectives, rewards summary, expiry timer, and a claim button.
    /// </summary>
    public class QuestEntryUI : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Image           questIcon;
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image           statusBackground;
        [SerializeField] private Color           activeColor    = Color.white;
        [SerializeField] private Color           completedColor = Color.green;
        [SerializeField] private Color           claimedColor   = Color.gray;
        [SerializeField] private Color           expiredColor   = Color.red;

        [Header("Objectives")]
        [SerializeField] private Transform          objectiveContainer;
        [SerializeField] private GameObject         objectiveRowPrefab;

        [Header("Rewards")]
        [SerializeField] private TextMeshProUGUI    rewardsSummaryText;

        [Header("Timer")]
        [SerializeField] private GameObject         timerRoot;
        [SerializeField] private TextMeshProUGUI    timerText;

        [Header("Claim")]
        [SerializeField] private Button             claimButton;
        [SerializeField] private TextMeshProUGUI    claimButtonText;

        [Header("Reward Popup")]
        [SerializeField] private QuestRewardPopupUI rewardPopup;

        private QuestProgress                _quest;
        private List<QuestObjectiveRowUI>    _objectivePool = new();
        private float                        _timerUpdateInterval = 1f;
        private float                        _timerTimer;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            claimButton?.onClick.AddListener(OnClaimClicked);
        }

        private void Update()
        {
            if (_quest == null || _quest.Data.expiryHours <= 0) return;

            _timerTimer -= Time.deltaTime;
            if (_timerTimer <= 0f)
            {
                _timerTimer = _timerUpdateInterval;
                RefreshTimer();
            }
        }

        // ─── Public API ────────────────────────────────────────────────────────

        public void Populate(QuestProgress quest)
        {
            _quest = quest;
            if (quest == null) { gameObject.SetActive(false); return; }

            // Icon & text
            if (questIcon            != null) questIcon.sprite          = quest.Data.questIcon;
            if (questNameText        != null) questNameText.text        = quest.Data.questName;
            if (questDescriptionText != null) questDescriptionText.text = quest.Data.questDescription;

            RefreshStatus();
            PopulateObjectives();
            RefreshRewardsSummary();
            RefreshTimer();
        }

        // ─── Private Helpers ───────────────────────────────────────────────────

        private void RefreshStatus()
        {
            if (_quest == null) return;

            string label = _quest.Status.ToString();
            Color  bg    = _quest.Status switch
            {
                QuestStatus.Active    => activeColor,
                QuestStatus.Completed => completedColor,
                QuestStatus.Claimed   => claimedColor,
                QuestStatus.Expired   => expiredColor,
                _                     => activeColor
            };

            if (statusText != null) statusText.text = label;
            if (statusBackground != null) statusBackground.color = bg;

            // Claim button
            bool canClaim = _quest.Status == QuestStatus.Completed;
            if (claimButton != null) claimButton.interactable = canClaim;
            if (claimButtonText != null)
                claimButtonText.text = _quest.Status == QuestStatus.Claimed ? "Claimed" : "Claim";
        }

        private void PopulateObjectives()
        {
            if (objectiveContainer == null || objectiveRowPrefab == null) return;

            var objectives = _quest.Objectives;

            while (_objectivePool.Count < objectives.Count)
            {
                var go  = Instantiate(objectiveRowPrefab, objectiveContainer);
                var row = go.GetComponent<QuestObjectiveRowUI>();
                if (row != null) _objectivePool.Add(row);
            }

            for (int i = 0; i < _objectivePool.Count; i++)
            {
                if (i < objectives.Count)
                {
                    _objectivePool[i].gameObject.SetActive(true);
                    _objectivePool[i].Populate(objectives[i]);
                }
                else
                {
                    _objectivePool[i].gameObject.SetActive(false);
                }
            }
        }

        private void RefreshRewardsSummary()
        {
            if (rewardsSummaryText == null) return;

            var parts = new List<string>();
            foreach (var r in _quest.Data.rewards)
                parts.Add(r.ToString());

            rewardsSummaryText.text = parts.Count > 0
                ? "Rewards: " + string.Join("  |  ", parts)
                : string.Empty;
        }

        private void RefreshTimer()
        {
            bool hasExpiry = _quest.Data.expiryHours > 0 && _quest.Status == QuestStatus.Active;
            if (timerRoot != null) timerRoot.SetActive(hasExpiry);
            if (!hasExpiry || timerText == null) return;

            var remaining = _quest.TimeRemaining;
            timerText.text = remaining.TotalHours >= 1
                ? $"Expires: {(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}"
                : $"Expires: {remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }

        private void OnClaimClicked()
        {
            if (_quest == null) return;
            bool claimed = QuestManager.Instance?.TryClaimQuest(_quest.Data.questId) ?? false;
            if (claimed)
            {
                RefreshStatus();
                rewardPopup?.Show(_quest);
            }
        }
    }
}
