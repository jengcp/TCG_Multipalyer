using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Quest;

namespace TCG.UI.Quest
{
    /// <summary>
    /// HUD element that shows a toast notification when a quest completes
    /// and maintains a badge count for unclaimed completed quests.
    /// </summary>
    public class QuestNotificationUI : MonoBehaviour
    {
        [Header("Badge")]
        [SerializeField] private GameObject      badgeRoot;
        [SerializeField] private TextMeshProUGUI badgeCountText;

        [Header("Toast")]
        [SerializeField] private GameObject      toastRoot;
        [SerializeField] private TextMeshProUGUI toastText;
        [SerializeField] private float           toastDuration = 3f;

        private readonly Queue<QuestProgress> _toastQueue = new();
        private bool _toastShowing;
        private int  _unclaimedCount;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnQuestCompleted += OnQuestCompleted;
            GameEvents.OnQuestClaimed   += OnQuestClaimed;
            GameEvents.OnQuestRotationRefreshed += _ => RefreshBadge();
        }

        private void OnDisable()
        {
            GameEvents.OnQuestCompleted -= OnQuestCompleted;
            GameEvents.OnQuestClaimed   -= OnQuestClaimed;
            GameEvents.OnQuestRotationRefreshed -= _ => RefreshBadge();
        }

        private void Start() => RefreshBadge();

        // ─── Handlers ─────────────────────────────────────────────────────────

        private void OnQuestCompleted(QuestProgress quest)
        {
            _unclaimedCount++;
            RefreshBadge();
            _toastQueue.Enqueue(quest);
            if (!_toastShowing) StartCoroutine(ShowNextToast());
        }

        private void OnQuestClaimed(QuestProgress _)
        {
            _unclaimedCount = Mathf.Max(0, _unclaimedCount - 1);
            RefreshBadge();
        }

        // ─── Badge ─────────────────────────────────────────────────────────────

        private void RefreshBadge()
        {
            // Count all completed (unclaimed) quests from the manager
            int count = 0;
            var qm = QuestManager.Instance;
            if (qm != null)
            {
                count += CountCompleted(qm.DailyQuests);
                count += CountCompleted(qm.WeeklyQuests);
                count += CountCompleted(qm.StoryQuests);
                count += CountCompleted(qm.Achievements);
            }
            _unclaimedCount = count;

            if (badgeRoot != null) badgeRoot.SetActive(_unclaimedCount > 0);
            if (badgeCountText != null) badgeCountText.text = _unclaimedCount.ToString();
        }

        private static int CountCompleted(IReadOnlyList<QuestProgress> list)
        {
            int n = 0;
            foreach (var q in list)
                if (q.Status == QuestStatus.Completed) n++;
            return n;
        }

        // ─── Toast ─────────────────────────────────────────────────────────────

        private IEnumerator ShowNextToast()
        {
            while (_toastQueue.Count > 0)
            {
                _toastShowing = true;
                var quest = _toastQueue.Dequeue();

                if (toastRoot != null)  toastRoot.SetActive(true);
                if (toastText != null)  toastText.text = $"Quest Complete!\n{quest.Data.questName}";

                yield return new WaitForSeconds(toastDuration);

                if (toastRoot != null) toastRoot.SetActive(false);
            }
            _toastShowing = false;
        }
    }
}
