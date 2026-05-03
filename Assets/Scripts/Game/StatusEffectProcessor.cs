using System.Collections.Generic;
using TCG.Cards;
using TCG.Core;
using TCG.Player;

namespace TCG.Game
{
    /// <summary>
    /// Processes end-of-turn and start-of-turn timed effects:
    ///   - Poison damage per turn on creatures
    ///   - Regenerate reset for creatures with that keyword
    ///   - Fatigue damage when a player draws from an empty deck
    /// </summary>
    public static class StatusEffectProcessor
    {
        // Damage dealt per creature per turn while Poisoned
        public const int PoisonDamagePerTurn = 1;

        // Fatigue damage scales: turn 1 past empty = 1 dmg, turn 2 = 2, etc.
        private static int _player1FatigueDamage = 0;
        private static int _player2FatigueDamage = 0;

        public static void Reset()
        {
            _player1FatigueDamage = 0;
            _player2FatigueDamage = 0;
        }

        // ── End-of-turn processing ─────────────────────────────────────────

        /// <summary>
        /// Call at the END of a player's turn for their own field.
        /// Applies poison damage to all poisoned creatures they own.
        /// </summary>
        public static void ProcessEndOfTurn(PlayerState player)
        {
            ApplyPoisonTick(player.Field);
        }

        private static void ApplyPoisonTick(Field field)
        {
            var snapshot = new List<Card>(field.Creatures);
            foreach (var creature in snapshot)
            {
                if (!creature.HasStatus(StatusEffect.Poisoned)) continue;
                creature.TakeDamage(PoisonDamagePerTurn);
            }
        }

        // ── Start-of-turn processing ───────────────────────────────────────

        /// <summary>
        /// Call at the START of a player's turn.
        /// Resets Regenerate charges on creatures that have it.
        /// </summary>
        public static void ProcessStartOfTurn(PlayerState player)
        {
            ResetRegenerate(player.Field);
        }

        private static void ResetRegenerate(Field field)
        {
            foreach (var creature in field.Creatures)
            {
                if (creature.Data.hasRegenerate)
                    creature.RemoveStatus(StatusEffect.Shielded); // regenerate re-grants shield next hit
            }
        }

        // ── Fatigue ────────────────────────────────────────────────────────

        /// <summary>
        /// Call when a player attempts to draw but their deck is empty.
        /// Fatigue damage increases by 1 each time.
        /// Returns the damage dealt.
        /// </summary>
        public static int ApplyFatigue(PlayerState player, bool isPlayer1)
        {
            int dmg;
            if (isPlayer1)
            {
                _player1FatigueDamage++;
                dmg = _player1FatigueDamage;
            }
            else
            {
                _player2FatigueDamage++;
                dmg = _player2FatigueDamage;
            }

            player.TakeDamage(dmg);
            GameEvents.DrawAttempt(player, DrawResult.FatigueDamage, dmg);
            return dmg;
        }
    }
}
