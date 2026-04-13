using UnityEngine;
using TCG.Core;
using TCG.Items;

namespace TCG.Quest
{
    /// <summary>
    /// Editor / QA helper: call these methods from the Inspector context menu
    /// or hook them to UI buttons to simulate game events during testing.
    /// Remove or strip this from release builds.
    /// </summary>
    public class QuestDebugHelper : MonoBehaviour
    {
        [Header("Simulate Match")]
        [SerializeField] private bool        simulateWin    = true;
        [SerializeField] private CardElement matchElement   = CardElement.Fire;
        [SerializeField] private CardClass   matchClass     = CardClass.Creature;

        [Header("Simulate Gold")]
        [SerializeField] private int goldEarnedAmount = 100;

        [Header("Simulate Pack Open")]
        [SerializeField] private string packItemId = "starter_pack";

        [ContextMenu("Simulate: Match Completed")]
        public void SimulateMatchCompleted()
            => GameEvents.RaiseMatchCompleted(simulateWin, matchElement, matchClass);

        [ContextMenu("Simulate: Card Played")]
        public void SimulateCardPlayed()
            => GameEvents.RaiseCardPlayed(null);

        [ContextMenu("Simulate: Day Login")]
        public void SimulateDayLogin()
            => GameEvents.RaiseDayLogin();

        [ContextMenu("Simulate: Gold Earned")]
        public void SimulateGoldEarned()
            => GameEvents.RaiseGoldEarned(goldEarnedAmount);

        [ContextMenu("Simulate: Force Daily Refresh")]
        public void ForceDailyRefresh()
            => QuestManager.Instance?.ForceRefreshDaily();

        [ContextMenu("Simulate: Force Weekly Refresh")]
        public void ForceWeeklyRefresh()
            => QuestManager.Instance?.ForceRefreshWeekly();

        [ContextMenu("Simulate: Day Login")]
        private void Login() => SimulateDayLogin();
    }
}
