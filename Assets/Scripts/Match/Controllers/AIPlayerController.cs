using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TCG.Match
{
    /// <summary>
    /// Simple AI opponent. Introduces think delays to simulate human pacing.
    /// Strategy: plays the most expensive affordable creature; attacks with random
    /// untapped creatures; targets the opponent's weakest creature or goes direct.
    /// </summary>
    public class AIPlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private float thinkDelayMin = 0.5f;
        [SerializeField] private float thinkDelayMax = 1.5f;

        private float ThinkDelay => UnityEngine.Random.Range(thinkDelayMin, thinkDelayMax);

        // ── IPlayerController ─────────────────────────────────────────────────────

        public IEnumerator TakeTurn(PlayerState myState, MatchState matchState)
        {
            yield return null;
        }

        public IEnumerator SelectCardToPlay(PlayerState state, Action<CardInstance> onSelected)
        {
            yield return new WaitForSeconds(ThinkDelay);

            CardInstance chosen  = null;
            int          bestCost = -1;

            foreach (var card in state.Hand)
            {
                if (card.BaseData.manaCost <= state.CurrentMana &&
                    card.BaseData.manaCost > bestCost &&
                    state.FirstEmptySlot() != null)
                {
                    chosen   = card;
                    bestCost = card.BaseData.manaCost;
                }
            }

            onSelected?.Invoke(chosen); // null = pass (no card played this iteration)
        }

        public IEnumerator SelectAttackSlot(PlayerState state, Action<BattlefieldSlot> onSelected)
        {
            yield return new WaitForSeconds(ThinkDelay);

            var valid = state.OccupiedSlots().FindAll(s => !s.Occupant.IsTapped);

            if (valid.Count == 0) { onSelected?.Invoke(null); yield break; }

            onSelected?.Invoke(valid[UnityEngine.Random.Range(0, valid.Count)]);
        }

        public IEnumerator SelectDefendSlot(PlayerState opponentState, Action<BattlefieldSlot> onSelected)
        {
            yield return new WaitForSeconds(ThinkDelay);

            var occupied = opponentState.OccupiedSlots();

            // 30% chance to go direct; otherwise target the weakest creature
            if (occupied.Count == 0 || UnityEngine.Random.value < 0.3f)
            {
                onSelected?.Invoke(null);
                yield break;
            }

            occupied.Sort((a, b) =>
                a.Occupant.CurrentHealth.CompareTo(b.Occupant.CurrentHealth));

            onSelected?.Invoke(occupied[0]);
        }
    }
}
