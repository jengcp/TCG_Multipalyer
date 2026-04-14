using TCG.Core;

namespace TCG.Match
{
    /// <summary>
    /// Static class that resolves combat between creatures and direct attacks on players.
    /// Simultaneous damage model: both sides deal damage before death is checked.
    /// </summary>
    public static class CombatResolver
    {
        /// <summary>
        /// Resolves combat between two creatures.
        /// Both sides deal damage simultaneously; then deaths are checked.
        /// Attacker is tapped after the exchange.
        /// </summary>
        public static void ResolveAttack(
            CardInstance attacker,
            CardInstance defender,
            int          attackerOwnerIdx,
            int          defenderOwnerIdx,
            MatchState   state)
        {
            GameEvents.RaiseAttackDeclared(attacker, attackerOwnerIdx);

            // Simultaneous damage — compute actual hits before applying deaths
            int dmgToDefender = defender.TakeDamage(attacker.CurrentAttack);
            int dmgToAttacker = attacker.TakeDamage(defender.CurrentAttack);

            GameEvents.RaiseDamageDealt(dmgToDefender, defender, defenderOwnerIdx);
            GameEvents.RaiseDamageDealt(dmgToAttacker, attacker, attackerOwnerIdx);

            attacker.Tap();

            // Check deaths after both creatures have taken damage
            if (!defender.IsAlive)
                Effects.CardEffectProcessor.KillCreature(defender, defenderOwnerIdx, state);

            if (!attacker.IsAlive)
                Effects.CardEffectProcessor.KillCreature(attacker, attackerOwnerIdx, state);
        }

        /// <summary>
        /// Resolves a direct attack against the opposing player (no defending creature).
        /// Attacker deals raw ATK damage — no card defense applies to player health.
        /// Attacker is tapped after the attack.
        /// </summary>
        public static void ResolveDirect(
            CardInstance attacker,
            PlayerState  defenderPlayer,
            int          attackerOwnerIdx,
            MatchState   state)
        {
            GameEvents.RaiseAttackDeclared(attacker, attackerOwnerIdx);

            int rawDamage = attacker.CurrentAttack;
            int delta     = defenderPlayer.ModifyHealth(-rawDamage);

            GameEvents.RaiseDamageDealt(rawDamage, null, defenderPlayer.PlayerIndex);
            GameEvents.RaisePlayerHealthChanged(
                defenderPlayer.PlayerIndex, defenderPlayer.CurrentHealth, delta);

            attacker.Tap();
        }
    }
}
