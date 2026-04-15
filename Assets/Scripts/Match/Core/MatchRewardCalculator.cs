using System;
using TCG.Core;
using TCG.Currency;
using TCG.Save;
using UnityEngine;

namespace TCG.Match
{
    /// <summary>
    /// Calculates end-of-match gold rewards, persists the match record, and grants currency.
    /// Called once by MatchManager.EndMatch — never called by UI.
    /// </summary>
    public static class MatchRewardCalculator
    {
        // Gold amounts
        private const int WinBase   = 100;
        private const int WinPerTurn = 10;
        private const int LossGold  = 25;
        private const int DrawGold  = 50;

        /// <summary>
        /// Calculates rewards based on result and turns played, grants gold via CurrencyManager,
        /// fires GameEvents.RaiseGoldEarned, and persists a MatchRecordEntry to save data.
        /// Returns the populated MatchRewards struct.
        /// </summary>
        public static MatchRewards CalculateRewards(
            MatchResult result,
            MatchState  state,
            int         localPlayerIndex = 0)
        {
            int turns    = state.TurnNumber;
            int goldEarned;

            switch (result)
            {
                case MatchResult.Victory:
                    goldEarned = WinBase + turns * WinPerTurn;
                    break;
                case MatchResult.Defeat:
                    goldEarned = LossGold;
                    break;
                default: // Draw
                    goldEarned = DrawGold;
                    break;
            }

            // Grant currency
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.Add(CurrencyType.Gold, goldEarned);
            else
                Debug.LogWarning("[MatchRewardCalculator] CurrencyManager.Instance is null — gold not granted.");

            GameEvents.RaiseGoldEarned(goldEarned);

            // Persist record
            PersistRecord(result, turns, goldEarned);

            return new MatchRewards { GoldEarned = goldEarned };
        }

        // ── Persistence ────────────────────────────────────────────────────────────

        private static void PersistRecord(MatchResult result, int turns, int goldEarned)
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();

            save.matchHistory ??= new MatchHistorySaveData();
            save.matchHistory.entries ??= new System.Collections.Generic.List<MatchRecordEntry>();

            save.matchHistory.entries.Add(new MatchRecordEntry
            {
                dateTicks  = DateTime.UtcNow.Ticks,
                result     = (int)result,
                turnsPlayed = turns,
                goldEarned = goldEarned
            });

            SaveSystem.Save(save);
        }
    }

    /// <summary>Lightweight struct carrying rewards from a completed match.</summary>
    public struct MatchRewards
    {
        public int GoldEarned;
    }
}
