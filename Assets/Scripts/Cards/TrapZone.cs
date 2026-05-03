using UnityEngine;
using System.Collections.Generic;
using TCG.Core;
using TCG.Player;

namespace TCG.Cards
{
    /// <summary>
    /// Holds face-down trap cards.
    /// Call CheckTrigger() at each game event — matching traps activate automatically.
    /// </summary>
    public class TrapZone
    {
        public const int MaxTraps = 5;

        private List<Card> _traps = new List<Card>();
        public IReadOnlyList<Card> Traps => _traps;
        public int Count => _traps.Count;
        public bool IsFull => _traps.Count >= MaxTraps;

        public bool SetTrap(Card card)
        {
            if (IsFull || !card.Data.IsTrap) return false;
            card.SetZone(GameZone.TrapZone);
            _traps.Add(card);
            GameEvents.TrapSet(card, card.Owner);
            return true;
        }

        public bool RemoveTrap(Card card) => _traps.Remove(card);

        /// <summary>
        /// Checks all set traps against a trigger condition.
        /// Matching traps reveal and resolve their effects, then go to graveyard.
        /// </summary>
        /// <param name="trigger">The event that just occurred.</param>
        /// <param name="sourceCard">The card that caused the event (nullable).</param>
        public void CheckTrigger(TrapTrigger trigger, PlayerState owner, Card sourceCard = null)
        {
            // Iterate a copy because triggering modifies the list
            var toCheck = new List<Card>(_traps);

            foreach (var trap in toCheck)
            {
                if (!MatchesTrigger(trap, trigger)) continue;

                _traps.Remove(trap);
                GameEvents.TrapTriggered(trap, owner, trigger);

                // Resolve all on-play effects as the trap activates
                foreach (var effect in trap.Data.onPlayEffects)
                    EffectResolver.Resolve(effect, owner, sourceCard);

                trap.SetZone(GameZone.Graveyard);
                owner.Graveyard.AddCard(trap);
                GameEvents.CardDestroyed(trap, owner);
            }
        }

        private static bool MatchesTrigger(Card trap, TrapTrigger trigger)
        {
            return trap.Data.trapTrigger == trigger;
        }

        public void Clear() => _traps.Clear();
    }
}
