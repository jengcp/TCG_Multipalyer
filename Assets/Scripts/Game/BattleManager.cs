using UnityEngine;
using TCG.Cards;
using TCG.Core;
using TCG.Player;

namespace TCG.Game
{
    public class BattleManager
    {
        /// <summary>
        /// Resolves combat between two creatures (simultaneous damage).
        /// </summary>
        public void ResolveCombat(Card attacker, Card defender)
        {
            if (!attacker.CanAttack || !defender.IsAlive) return;

            GameEvents.AttackDeclared(attacker, defender);

            int attackerDmg = attacker.CurrentAttack;
            int defenderDmg = defender.CurrentAttack;

            if (attacker.Data.hasFirstStrike)
            {
                defender.TakeDamage(attackerDmg);
                if (defender.IsAlive)
                    attacker.TakeDamage(defenderDmg);
            }
            else if (defender.Data.hasFirstStrike)
            {
                attacker.TakeDamage(defenderDmg);
                if (attacker.IsAlive)
                    defender.TakeDamage(attackerDmg);
            }
            else
            {
                // Simultaneous
                defender.TakeDamage(attackerDmg);
                attacker.TakeDamage(defenderDmg);
            }

            // Lifelink — attacker heals its owner for damage dealt
            if (attacker.Data.hasLifelink && attacker.IsAlive)
                attacker.Owner.Heal(attackerDmg);

            attacker.Exhaust();
        }

        /// <summary>
        /// Attacker hits the opponent player directly.
        /// </summary>
        public void ResolvePlayerAttack(Card attacker, PlayerState opponent)
        {
            if (!attacker.CanAttack) return;

            GameEvents.AttackOnPlayer(attacker, opponent);
            opponent.TakeDamage(attacker.CurrentAttack);

            if (attacker.Data.hasLifelink)
                attacker.Owner.Heal(attacker.CurrentAttack);

            attacker.Exhaust();
        }
    }
}
