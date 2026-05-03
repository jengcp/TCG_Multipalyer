using UnityEngine;
using System.Collections.Generic;
using TCG.Cards;
using TCG.Core;
using TCG.Player;

namespace TCG.Game
{
    /// <summary>
    /// Manages the full combat flow:
    ///   DeclareAttackers → DeclareBlockers → ResolveCombat
    ///
    /// Also wraps the original single-card BattleManager logic for direct attacks.
    /// </summary>
    public class CombatManager
    {
        public BattleSubPhase SubPhase { get; private set; } = BattleSubPhase.Idle;
        public CombatState Current { get; private set; }

        private BattleManager _battle = new BattleManager();

        // ── Phase control ──────────────────────────────────────────────────

        public void BeginDeclareAttackers(PlayerState activePlayer, PlayerState defender)
        {
            Current = new CombatState(activePlayer, defender);
            SetSubPhase(BattleSubPhase.DeclareAttackers);
        }

        /// <summary>Active player toggles an attacker in/out.</summary>
        public void ToggleAttacker(Card card)
        {
            if (SubPhase != BattleSubPhase.DeclareAttackers) return;
            if (Current.DeclaredAttackers.Contains(card))
                Current.RemoveAttacker(card);
            else
                Current.AddAttacker(card);
        }

        /// <summary>
        /// Active player confirms their attacker list.
        /// Exhausts all declared attackers (except Vigilance).
        /// Fires OnAttackersDeclared and transitions to DeclareBlockers.
        /// </summary>
        public void ConfirmAttackers()
        {
            if (SubPhase != BattleSubPhase.DeclareAttackers) return;
            if (Current.DeclaredAttackers.Count == 0)
            {
                // No attackers — skip combat entirely
                SetSubPhase(BattleSubPhase.Idle);
                return;
            }

            // Exhaust attackers now (Vigilance skips exhaustion)
            foreach (var atk in Current.DeclaredAttackers)
            {
                if (!atk.Data.hasVigilance) atk.Exhaust();
                Current.Attacker.TrapZone?.CheckTrigger(TrapTrigger.OnCreatureAttacks,
                    Current.Defender, atk);
            }

            GameEvents.AttackersDeclared(new List<Card>(Current.DeclaredAttackers));
            SetSubPhase(BattleSubPhase.DeclareBlockers);
        }

        /// <summary>Defending player assigns a blocker to an attacker.</summary>
        public bool AssignBlocker(Card blocker, Card attacker)
        {
            if (SubPhase != BattleSubPhase.DeclareBlockers) return false;
            bool ok = Current.AssignBlocker(blocker, attacker);
            if (ok) GameEvents.BlockerAssigned(blocker, attacker);
            return ok;
        }

        public void UnassignBlocker(Card blocker)
        {
            if (SubPhase != BattleSubPhase.DeclareBlockers) return;
            Current.UnassignBlocker(blocker);
        }

        /// <summary>Defending player finalises blockers → resolve combat.</summary>
        public void ConfirmBlockers()
        {
            if (SubPhase != BattleSubPhase.DeclareBlockers) return;
            GameEvents.BlockersConfirmed();
            SetSubPhase(BattleSubPhase.ResolveCombat);
            ResolveCombat();
        }

        // ── Combat resolution ──────────────────────────────────────────────

        private void ResolveCombat()
        {
            var defender = Current.Defender;

            foreach (var attacker in Current.DeclaredAttackers)
            {
                if (!attacker.IsAlive) continue;

                var blockers = Current.GetBlockersFor(attacker);

                if (blockers.Count == 0)
                {
                    // Unblocked — hit player directly
                    _battle.ResolvePlayerAttack(attacker, defender);
                }
                else if (blockers.Count == 1)
                {
                    // Standard 1v1 block
                    _battle.ResolveCombat(attacker, blockers[0]);
                }
                else
                {
                    // Multiple blockers — attacker distributes damage, each blocker hits back
                    ResolveMultiBlock(attacker, blockers);
                }
            }

            Current.Clear();
            SetSubPhase(BattleSubPhase.Idle);
        }

        private void ResolveMultiBlock(Card attacker, List<Card> blockers)
        {
            int totalAttack = attacker.CurrentAttack;

            // Each blocker deals its full attack to the attacker simultaneously
            int totalBlockerDamage = 0;
            foreach (var b in blockers)
                totalBlockerDamage += b.CurrentAttack;

            // Attacker distributes damage across blockers (sequential, front-loaded)
            int remaining = totalAttack;
            foreach (var b in blockers)
            {
                int dmg = Mathf.Min(remaining, b.CurrentHealth);
                b.TakeDamage(dmg);
                remaining -= dmg;
                if (remaining <= 0) break;
            }

            attacker.TakeDamage(totalBlockerDamage);

            if (attacker.IsAlive && attacker.Data.hasLifelink)
                attacker.Owner.Heal(totalAttack - remaining);
        }

        // ── Direct single-card attacks (outside combat flow, e.g. abilities) ──

        public void DirectAttack(Card attacker, Card target) =>
            _battle.ResolveCombat(attacker, target);

        public void DirectPlayerAttack(Card attacker, PlayerState target) =>
            _battle.ResolvePlayerAttack(attacker, target);

        // ── Helpers ────────────────────────────────────────────────────────

        private void SetSubPhase(BattleSubPhase p)
        {
            SubPhase = p;
            GameEvents.BattleSubPhaseChanged(p);
        }
    }
}
