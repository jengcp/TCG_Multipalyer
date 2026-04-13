using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TCG.Quest
{
    /// <summary>
    /// Runtime progress for one objective inside an active quest.
    /// </summary>
    public class ObjectiveProgress
    {
        public QuestObjectiveData Data    { get; }
        public int                Current { get; private set; }
        public int                Target  => Data.targetCount;
        public bool               IsDone  => Current >= Target;
        public float              Ratio   => Target <= 0 ? 1f : Mathf.Clamp01((float)Current / Target);

        public ObjectiveProgress(QuestObjectiveData data, int savedCurrent = 0)
        {
            Data    = data;
            Current = savedCurrent;
        }

        /// <summary>Adds <paramref name="amount"/> to the current count, capped at Target.</summary>
        public void Increment(int amount = 1)
        {
            Current = Mathf.Min(Current + amount, Target);
        }

        public void SetCurrent(int value) => Current = Mathf.Clamp(value, 0, Target);

        public override string ToString() =>
            $"{Data.description}: {Current}/{Target}";
    }

    /// <summary>
    /// Runtime state of one quest for the active player session.
    /// Created by QuestManager and updated by QuestTracker.
    /// </summary>
    public class QuestProgress
    {
        public QuestData                  Data       { get; }
        public QuestStatus                Status     { get; private set; }
        public List<ObjectiveProgress>    Objectives { get; }
        public DateTime                   ActivatedAt { get; private set; }
        public DateTime                   ExpiresAt   { get; private set; }

        public bool IsExpired  => Data.expiryHours > 0 && DateTime.UtcNow >= ExpiresAt;
        public bool AllDone    => Objectives.All(o => o.IsDone);

        public QuestProgress(QuestData data, QuestStatus savedStatus = QuestStatus.Active,
            List<int> savedCounts = null, long activatedTicks = 0)
        {
            Data    = data;
            Status  = savedStatus;

            Objectives = data.objectives
                .Select((obj, idx) => new ObjectiveProgress(
                    obj,
                    savedCounts != null && idx < savedCounts.Count ? savedCounts[idx] : 0))
                .ToList();

            ActivatedAt = activatedTicks > 0
                ? new DateTime(activatedTicks, DateTimeKind.Utc)
                : DateTime.UtcNow;

            ExpiresAt = data.expiryHours > 0
                ? ActivatedAt.AddHours(data.expiryHours)
                : DateTime.MaxValue;
        }

        // ── State Machine ─────────────────────────────────────────────────────

        public void Activate()
        {
            if (Status == QuestStatus.Locked)
            {
                Status      = QuestStatus.Active;
                ActivatedAt = DateTime.UtcNow;
                if (Data.expiryHours > 0)
                    ExpiresAt = ActivatedAt.AddHours(Data.expiryHours);
            }
        }

        /// <summary>
        /// Checks completion / expiry and transitions Status accordingly.
        /// Called by QuestTracker after every objective increment.
        /// </summary>
        public void Evaluate()
        {
            if (Status != QuestStatus.Active) return;

            if (IsExpired)
            {
                Status = QuestStatus.Expired;
                return;
            }

            if (AllDone)
                Status = QuestStatus.Completed;
        }

        public bool TryClaim()
        {
            if (Status != QuestStatus.Completed) return false;
            Status = QuestStatus.Claimed;
            return true;
        }

        public void ForceExpire() => Status = QuestStatus.Expired;

        // ── Convenience ──────────────────────────────────────────────────────

        public TimeSpan TimeRemaining =>
            Data.expiryHours > 0 && Status == QuestStatus.Active
                ? (ExpiresAt > DateTime.UtcNow ? ExpiresAt - DateTime.UtcNow : TimeSpan.Zero)
                : TimeSpan.MaxValue;

        public override string ToString() =>
            $"[{Status}] {Data.questName}  " +
            string.Join(", ", Objectives.Select(o => o.ToString()));
    }
}
