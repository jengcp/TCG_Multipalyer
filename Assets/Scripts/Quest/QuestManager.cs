using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TCG.Core;
using TCG.Currency;
using TCG.Inventory;
using TCG.Items;
using TCG.Save;

namespace TCG.Quest
{
    /// <summary>
    /// Singleton that owns all runtime quest state.
    /// Handles quest activation, rotation (daily/weekly refresh),
    /// prerequisite gating, reward granting, and persistence.
    ///
    /// <para>Usage:
    /// <list type="bullet">
    ///   <item>Assign <see cref="allQuests"/> in the Inspector.</item>
    ///   <item>Call <see cref="TryClaimQuest"/> from the UI claim button.</item>
    ///   <item>Subscribe to <see cref="GameEvents.OnQuestCompleted"/> for notifications.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Quest Roster")]
        [Tooltip("All QuestData assets that can appear in the game.")]
        [SerializeField] private List<QuestData> allQuests = new();

        [Header("Rotation Limits")]
        [Tooltip("Max concurrent Daily quests shown at once.")]
        [SerializeField] private int maxDailyQuests   = 3;
        [Tooltip("Max concurrent Weekly quests shown at once.")]
        [SerializeField] private int maxWeeklyQuests  = 2;

        // ── Runtime State ─────────────────────────────────────────────────────
        private readonly Dictionary<string, QuestProgress> _progressMap = new();

        // Cached lists for fast UI queries
        private List<QuestProgress> _dailyQuests   = new();
        private List<QuestProgress> _weeklyQuests  = new();
        private List<QuestProgress> _storyQuests   = new();
        private List<QuestProgress> _achievements  = new();

        private DateTime _nextDailyRefresh;
        private DateTime _nextWeeklyRefresh;

        public TimeSpan TimeUntilDailyRefresh  =>
            _nextDailyRefresh  > DateTime.UtcNow ? _nextDailyRefresh  - DateTime.UtcNow : TimeSpan.Zero;
        public TimeSpan TimeUntilWeeklyRefresh =>
            _nextWeeklyRefresh > DateTime.UtcNow ? _nextWeeklyRefresh - DateTime.UtcNow : TimeSpan.Zero;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadQuestState();
            RefreshRotations();
            EvaluatePrerequisites();
        }

        private void Update()
        {
            // Check refresh timers once per second
            if (Time.frameCount % 60 != 0) return;

            if (DateTime.UtcNow >= _nextDailyRefresh)
                RefreshCategory(QuestCategory.Daily, maxDailyQuests);

            if (DateTime.UtcNow >= _nextWeeklyRefresh)
                RefreshCategory(QuestCategory.Weekly, maxWeeklyQuests);

            // Expire overdue quests
            foreach (var qp in _progressMap.Values)
                if (qp.Status == QuestStatus.Active && qp.IsExpired)
                {
                    qp.ForceExpire();
                    GameEvents.RaiseQuestExpired(qp);
                }
        }

        // ─── Public API ────────────────────────────────────────────────────────

        public IReadOnlyList<QuestProgress> DailyQuests   => _dailyQuests.AsReadOnly();
        public IReadOnlyList<QuestProgress> WeeklyQuests  => _weeklyQuests.AsReadOnly();
        public IReadOnlyList<QuestProgress> StoryQuests   => _storyQuests.AsReadOnly();
        public IReadOnlyList<QuestProgress> Achievements  => _achievements.AsReadOnly();

        public QuestProgress GetProgress(string questId) =>
            _progressMap.TryGetValue(questId, out var qp) ? qp : null;

        /// <summary>
        /// Claims the reward of a completed quest.
        /// Returns false if the quest is not in Completed state.
        /// </summary>
        public bool TryClaimQuest(string questId)
        {
            var qp = GetProgress(questId);
            if (qp == null || !qp.TryClaim()) return false;

            GrantRewards(qp);
            GameEvents.RaiseQuestClaimed(qp);
            SaveQuestState();

            // Unlock quests whose prerequisites are now satisfied
            EvaluatePrerequisites();

            Debug.Log($"[QuestManager] Quest '{qp.Data.questName}' claimed.");
            return true;
        }

        /// <summary>
        /// Forces a daily rotation immediately (useful for debug / testing).
        /// </summary>
        public void ForceRefreshDaily()  => RefreshCategory(QuestCategory.Daily,  maxDailyQuests);
        public void ForceRefreshWeekly() => RefreshCategory(QuestCategory.Weekly, maxWeeklyQuests);

        // ─── Called by QuestTracker ────────────────────────────────────────────

        /// <summary>
        /// Advances all active quest objectives that match the given action context.
        /// <paramref name="targetValue"/> is used for "reach N" style objectives
        /// (e.g. own 50 unique cards) where progress is absolute, not incremental.
        /// Pass -1 to use incremental (<paramref name="amount"/>).
        /// </summary>
        internal void NotifyProgress(
            QuestObjectiveType type,
            string             specificItemId,
            CardElement        elementContext,
            CardClass          classContext,
            int                amount,
            int                targetValue)
        {
            bool anyChanged = false;

            foreach (var qp in _progressMap.Values)
            {
                if (qp.Status != QuestStatus.Active) continue;

                foreach (var obj in qp.Objectives)
                {
                    if (obj.IsDone)              continue;
                    if (obj.Data.objectiveType != type) continue;

                    // Apply optional filters
                    if (!PassesFilters(obj.Data, specificItemId, elementContext, classContext))
                        continue;

                    if (targetValue >= 0)
                        obj.SetCurrent(targetValue);
                    else
                        obj.Increment(amount);

                    anyChanged = true;
                }

                qp.Evaluate();

                if (qp.Status == QuestStatus.Completed)
                {
                    GameEvents.RaiseQuestCompleted(qp);
                    Debug.Log($"[QuestManager] Quest completed: '{qp.Data.questName}'");
                }
            }

            if (anyChanged)
                SaveQuestState();
        }

        // ─── Rotation ─────────────────────────────────────────────────────────

        private void RefreshRotations()
        {
            var save = SaveSystem.Load()?.quests;

            _nextDailyRefresh  = save?.nextDailyRefreshTicks  > 0
                ? new DateTime(save.nextDailyRefreshTicks,  DateTimeKind.Utc) : DateTime.UtcNow;
            _nextWeeklyRefresh = save?.nextWeeklyRefreshTicks > 0
                ? new DateTime(save.nextWeeklyRefreshTicks, DateTimeKind.Utc) : DateTime.UtcNow;

            if (DateTime.UtcNow >= _nextDailyRefresh)
                RefreshCategory(QuestCategory.Daily, maxDailyQuests);

            if (DateTime.UtcNow >= _nextWeeklyRefresh)
                RefreshCategory(QuestCategory.Weekly, maxWeeklyQuests);
        }

        private void RefreshCategory(QuestCategory category, int maxSlots)
        {
            // Expire currently active quests in this category
            foreach (var qp in _progressMap.Values)
            {
                if (qp.Data.category == category && qp.Status == QuestStatus.Active)
                    qp.ForceExpire();
            }

            // Pick new quests from the pool (exclude already-claimed or locked ones)
            var pool = allQuests
                .Where(qd => qd.category == category)
                .Where(qd => !_progressMap.TryGetValue(qd.questId, out var existing)
                             || existing.Status == QuestStatus.Expired
                             || existing.Status == QuestStatus.Claimed)
                .ToList();

            var chosen = PickRandom(pool, maxSlots);

            foreach (var qd in chosen)
            {
                var qp = new QuestProgress(qd);
                qp.Activate();
                _progressMap[qd.questId] = qp;
            }

            RebuildCachedLists();

            // Set next refresh time
            if (category == QuestCategory.Daily)
            {
                _nextDailyRefresh = DateTime.UtcNow.Date.AddDays(1); // midnight UTC
                GameEvents.RaiseQuestRotationRefreshed(category);
            }
            else if (category == QuestCategory.Weekly)
            {
                _nextWeeklyRefresh = DateTime.UtcNow.Date.AddDays(7 - (int)DateTime.UtcNow.DayOfWeek);
                GameEvents.RaiseQuestRotationRefreshed(category);
            }

            SaveQuestState();
        }

        // ─── Prerequisites ─────────────────────────────────────────────────────

        private void EvaluatePrerequisites()
        {
            foreach (var qd in allQuests)
            {
                if (qd.category != QuestCategory.Story &&
                    qd.category != QuestCategory.Achievement) continue;

                if (_progressMap.TryGetValue(qd.questId, out var existing) &&
                    existing.Status != QuestStatus.Locked) continue;

                bool prereqsMet = qd.prerequisiteQuestIds.All(pid =>
                    _progressMap.TryGetValue(pid, out var pre) &&
                    pre.Status == QuestStatus.Claimed);

                if (prereqsMet)
                {
                    var qp = _progressMap.TryGetValue(qd.questId, out var ex)
                        ? ex : new QuestProgress(qd, QuestStatus.Locked);

                    qp.Activate();
                    _progressMap[qd.questId] = qp;
                }
                else if (!_progressMap.ContainsKey(qd.questId))
                {
                    _progressMap[qd.questId] = new QuestProgress(qd, QuestStatus.Locked);
                }
            }

            RebuildCachedLists();
        }

        // ─── Rewards ──────────────────────────────────────────────────────────

        private void GrantRewards(QuestProgress qp)
        {
            foreach (var reward in qp.Data.rewards)
            {
                switch (reward.rewardType)
                {
                    case QuestRewardType.Currency:
                        CurrencyManager.Instance?.Add(reward.currencyType, reward.currencyAmount);
                        break;

                    case QuestRewardType.Item:
                        if (reward.itemReward != null)
                            PlayerInventory.Instance?.TryAddItem(reward.itemReward, reward.itemQuantity);
                        break;

                    case QuestRewardType.XP:
                        GameEvents.RaiseXPEarned(reward.xpAmount);
                        break;
                }
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static bool PassesFilters(QuestObjectiveData data,
            string specificItemId, CardElement element, CardClass cardClass)
        {
            if (!string.IsNullOrEmpty(data.specificItemId) &&
                data.specificItemId != specificItemId) return false;

            if (data.filterByElement   && data.elementFilter != element)   return false;
            if (data.filterByCardClass && data.classFilter   != cardClass) return false;

            return true;
        }

        private void RebuildCachedLists()
        {
            _dailyQuests  = _progressMap.Values.Where(q => q.Data.category == QuestCategory.Daily).ToList();
            _weeklyQuests = _progressMap.Values.Where(q => q.Data.category == QuestCategory.Weekly).ToList();
            _storyQuests  = _progressMap.Values.Where(q => q.Data.category == QuestCategory.Story).ToList();
            _achievements = _progressMap.Values.Where(q => q.Data.category == QuestCategory.Achievement).ToList();
        }

        private static List<T> PickRandom<T>(List<T> source, int count)
        {
            count = Mathf.Min(count, source.Count);
            var copy   = new List<T>(source);
            var result = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                int idx = UnityEngine.Random.Range(0, copy.Count);
                result.Add(copy[idx]);
                copy.RemoveAt(idx);
            }
            return result;
        }

        // ─── Persistence ──────────────────────────────────────────────────────

        private void LoadQuestState()
        {
            var save = SaveSystem.Load()?.quests;
            if (save?.entries == null) return;

            foreach (var entry in save.entries)
            {
                var qd = allQuests.FirstOrDefault(q => q.questId == entry.questId);
                if (qd == null) continue;

                var qp = new QuestProgress(
                    qd,
                    (QuestStatus)entry.status,
                    entry.objectiveCounts,
                    entry.activatedTicks);

                _progressMap[qd.questId] = qp;
            }

            RebuildCachedLists();
        }

        private void SaveQuestState()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.quests ??= new QuestsSaveData();

            save.quests.nextDailyRefreshTicks  = _nextDailyRefresh.Ticks;
            save.quests.nextWeeklyRefreshTicks = _nextWeeklyRefresh.Ticks;

            save.quests.entries = _progressMap.Values.Select(qp => new QuestProgressSaveData
            {
                questId          = qp.Data.questId,
                status           = (int)qp.Status,
                activatedTicks   = qp.ActivatedAt.Ticks,
                objectiveCounts  = qp.Objectives.Select(o => o.Current).ToList()
            }).ToList();

            SaveSystem.Save(save);
        }
    }
}
