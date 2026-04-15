using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Quest;

namespace TCG.UI.Quest
{
    /// <summary>
    /// Root quest panel with four category tabs (Daily / Weekly / Story / Achievements).
    /// Pools QuestEntryUI tiles and refreshes on quest events.
    /// </summary>
    public class QuestUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject questPanel;
        [SerializeField] private Button     openButton;
        [SerializeField] private Button     closeButton;

        [Header("Tabs")]
        [SerializeField] private Button tabDaily;
        [SerializeField] private Button tabWeekly;
        [SerializeField] private Button tabStory;
        [SerializeField] private Button tabAchievements;

        [Header("Quest List")]
        [SerializeField] private Transform  questContainer;
        [SerializeField] private GameObject questEntryPrefab;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Empty State")]
        [SerializeField] private GameObject      emptyStateRoot;
        [SerializeField] private TextMeshProUGUI emptyStateText;

        [Header("Refresh Timer")]
        [SerializeField] private TextMeshProUGUI refreshTimerText;
        [SerializeField] private GameObject      refreshTimerRoot;

        [Header("Reward Popup (shared)")]
        [SerializeField] private QuestRewardPopupUI rewardPopup;

        private readonly List<QuestEntryUI> _entryPool  = new();
        private QuestCategory               _activeTab  = QuestCategory.Daily;
        private float                       _timerTick;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            openButton?.onClick.AddListener(Open);
            closeButton?.onClick.AddListener(Close);

            tabDaily?.onClick.AddListener(() => SwitchTab(QuestCategory.Daily));
            tabWeekly?.onClick.AddListener(() => SwitchTab(QuestCategory.Weekly));
            tabStory?.onClick.AddListener(() => SwitchTab(QuestCategory.Story));
            tabAchievements?.onClick.AddListener(() => SwitchTab(QuestCategory.Achievement));

            Close();
        }

        private void OnEnable()
        {
            GameEvents.OnQuestCompleted         += _ => Refresh();
            GameEvents.OnQuestClaimed           += _ => Refresh();
            GameEvents.OnQuestExpired           += _ => Refresh();
            GameEvents.OnQuestRotationRefreshed += _ => Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnQuestCompleted         -= _ => Refresh();
            GameEvents.OnQuestClaimed           -= _ => Refresh();
            GameEvents.OnQuestExpired           -= _ => Refresh();
            GameEvents.OnQuestRotationRefreshed -= _ => Refresh();
        }

        private void Update()
        {
            _timerTick -= Time.deltaTime;
            if (_timerTick <= 0f)
            {
                _timerTick = 1f;
                RefreshRotationTimer();
            }
        }

        // ─── Public API ────────────────────────────────────────────────────────

        public void Open()
        {
            if (questPanel != null) questPanel.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (questPanel != null) questPanel.SetActive(false);
        }

        public void SwitchTab(QuestCategory category)
        {
            _activeTab = category;
            Refresh();
            HighlightTab(category);
        }

        // ─── Private Helpers ───────────────────────────────────────────────────

        private void Refresh()
        {
            var qm = QuestManager.Instance;
            if (qm == null) return;

            IReadOnlyList<QuestProgress> quests = _activeTab switch
            {
                QuestCategory.Daily       => qm.DailyQuests,
                QuestCategory.Weekly      => qm.WeeklyQuests,
                QuestCategory.Story       => qm.StoryQuests,
                QuestCategory.Achievement => qm.Achievements,
                _                         => qm.DailyQuests
            };

            PopulateList(quests);
            RefreshRotationTimer();

            if (scrollRect != null)
                scrollRect.normalizedPosition = new Vector2(0f, 1f);
        }

        private void PopulateList(IReadOnlyList<QuestProgress> quests)
        {
            // Grow pool
            while (_entryPool.Count < quests.Count)
            {
                var go    = Instantiate(questEntryPrefab, questContainer);
                var entry = go.GetComponent<QuestEntryUI>();
                if (entry != null) _entryPool.Add(entry);
            }

            for (int i = 0; i < _entryPool.Count; i++)
            {
                if (i < quests.Count)
                {
                    _entryPool[i].gameObject.SetActive(true);
                    _entryPool[i].Populate(quests[i]);
                }
                else
                {
                    _entryPool[i].gameObject.SetActive(false);
                }
            }

            bool isEmpty = quests.Count == 0;
            if (emptyStateRoot != null) emptyStateRoot.SetActive(isEmpty);
            if (emptyStateText != null && isEmpty)
                emptyStateText.text = "No quests available right now.";
        }

        private void RefreshRotationTimer()
        {
            var qm = QuestManager.Instance;
            if (qm == null || refreshTimerRoot == null) return;

            bool isTimedCategory = _activeTab == QuestCategory.Daily ||
                                   _activeTab == QuestCategory.Weekly;

            refreshTimerRoot.SetActive(isTimedCategory);
            if (!isTimedCategory || refreshTimerText == null) return;

            System.TimeSpan remaining = _activeTab == QuestCategory.Daily
                ? qm.TimeUntilDailyRefresh
                : qm.TimeUntilWeeklyRefresh;

            refreshTimerText.text = remaining.TotalSeconds > 0
                ? $"Refreshes in: {(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}"
                : "Refreshing...";
        }

        private void HighlightTab(QuestCategory category)
        {
            float alpha = 0.5f;
            SetTabAlpha(tabDaily,        category == QuestCategory.Daily        ? 1f : alpha);
            SetTabAlpha(tabWeekly,       category == QuestCategory.Weekly       ? 1f : alpha);
            SetTabAlpha(tabStory,        category == QuestCategory.Story        ? 1f : alpha);
            SetTabAlpha(tabAchievements, category == QuestCategory.Achievement  ? 1f : alpha);
        }

        private static void SetTabAlpha(Button tab, float alpha)
        {
            if (tab == null) return;
            var img = tab.GetComponent<Image>();
            if (img == null) return;
            var c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}
