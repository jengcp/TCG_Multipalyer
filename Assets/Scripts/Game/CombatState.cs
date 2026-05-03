using System.Collections.Generic;
using TCG.Cards;
using TCG.Player;

namespace TCG.Game
{
    /// <summary>
    /// Snapshot of one full round of combat:
    /// who is attacking, who is blocking whom, and which attackers go unblocked.
    /// </summary>
    public class CombatState
    {
        public PlayerState Attacker { get; }
        public PlayerState Defender { get; }

        // Ordered list of declared attackers
        public List<Card> DeclaredAttackers { get; } = new List<Card>();

        // blocker → attacker assignment (one blocker blocks one attacker)
        private Dictionary<Card, Card> _blockerToAttacker = new Dictionary<Card, Card>();

        // attacker → list of blockers (multiple creatures can block the same attacker)
        private Dictionary<Card, List<Card>> _attackerToBlockers = new Dictionary<Card, List<Card>>();

        public CombatState(PlayerState attacker, PlayerState defender)
        {
            Attacker = attacker;
            Defender = defender;
        }

        // ── Attacker declaration ───────────────────────────────────────────

        public void AddAttacker(Card card)
        {
            if (!DeclaredAttackers.Contains(card) && card.CanAttack)
            {
                DeclaredAttackers.Add(card);
                _attackerToBlockers[card] = new List<Card>();
            }
        }

        public void RemoveAttacker(Card card)
        {
            DeclaredAttackers.Remove(card);
            _attackerToBlockers.Remove(card);
        }

        // ── Blocker assignment ─────────────────────────────────────────────

        /// <returns>False if blocker is already assigned or attacker not declared.</returns>
        public bool AssignBlocker(Card blocker, Card attacker)
        {
            if (!DeclaredAttackers.Contains(attacker)) return false;
            if (_blockerToAttacker.ContainsKey(blocker)) return false;
            if (!blocker.IsAlive || blocker.HasStatus(TCG.Core.StatusEffect.Exhausted)) return false;

            _blockerToAttacker[blocker] = attacker;
            _attackerToBlockers[attacker].Add(blocker);
            return true;
        }

        public void UnassignBlocker(Card blocker)
        {
            if (!_blockerToAttacker.TryGetValue(blocker, out var attacker)) return;
            _blockerToAttacker.Remove(blocker);
            _attackerToBlockers[attacker].Remove(blocker);
        }

        // ── Queries ────────────────────────────────────────────────────────

        public List<Card> GetBlockersFor(Card attacker)
        {
            return _attackerToBlockers.TryGetValue(attacker, out var list)
                ? new List<Card>(list)
                : new List<Card>();
        }

        public bool IsBlocked(Card attacker) =>
            _attackerToBlockers.TryGetValue(attacker, out var list) && list.Count > 0;

        public Card GetAttackerFor(Card blocker) =>
            _blockerToAttacker.TryGetValue(blocker, out var a) ? a : null;

        public List<Card> GetUnblockedAttackers()
        {
            var result = new List<Card>();
            foreach (var atk in DeclaredAttackers)
                if (!IsBlocked(atk)) result.Add(atk);
            return result;
        }

        public void Clear()
        {
            DeclaredAttackers.Clear();
            _blockerToAttacker.Clear();
            _attackerToBlockers.Clear();
        }
    }
}
