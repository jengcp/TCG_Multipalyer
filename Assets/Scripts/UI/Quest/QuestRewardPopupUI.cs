using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Quest;

namespace TCG.UI.Quest
{
    /// <summary>
    /// Modal popup shown after a quest reward is claimed.
    /// Call Show() with the claimed QuestProgress; it populates the reward list and fades in.
    /// </summary>
    public class QuestRewardPopupUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button     closeButton;

        [Header("Quest Info")]
        [SerializeField] private TextMeshProUGUI questNameText;

        [Header("Reward List")]
        [SerializeField] private Transform  rewardContainer;
        [SerializeField] private GameObject rewardItemPrefab; // TextMeshProUGUI + optional icon

        [Header("Animation")]
        [SerializeField] private Animator panelAnimator;
        private static readonly int ShowTrigger = Animator.StringToHash("Show");

        private void Awake()
        {
            closeButton?.onClick.AddListener(Hide);
            Hide();
        }

        public void Show(QuestProgress quest)
        {
            if (quest == null) return;

            if (questNameText != null)
                questNameText.text = quest.Data.questName;

            PopulateRewards(quest.Data.rewards);

            if (panel != null)      panel.SetActive(true);
            if (panelAnimator != null) panelAnimator.SetTrigger(ShowTrigger);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void PopulateRewards(List<QuestRewardData> rewards)
        {
            if (rewardContainer == null || rewardItemPrefab == null) return;

            foreach (Transform child in rewardContainer)
                Destroy(child.gameObject);

            foreach (var reward in rewards)
            {
                var go  = Instantiate(rewardItemPrefab, rewardContainer);
                var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = reward.ToString();

                var img = go.GetComponentInChildren<Image>();
                if (img != null && reward.rewardType == QuestRewardType.Item)
                    img.sprite = reward.itemReward?.icon;
            }
        }
    }
}
