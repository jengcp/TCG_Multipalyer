using System;
using System.Collections.Generic;

namespace TCG.Match
{
    /// <summary>
    /// Full in-match state for one player: health, mana, hand, deck, battlefield, graveyard.
    /// Plain C# class — no MonoBehaviour. Mutated only by MatchManager / TurnManager / CombatResolver.
    /// </summary>
    public class PlayerState
    {
        // ── Constants ─────────────────────────────────────────────────────────────
        public const int MaxBattlefieldSlots = 5;
        public const int MaxHandSize         = 10;
        public const int MaxManaCap          = 10;  // Named MaxManaCap to avoid collision with MaxMana property
        public const int StartingHealth      = 30;

        // ── Identity ──────────────────────────────────────────────────────────────
        /// <summary>0 = local player, 1 = opponent.</summary>
        public int PlayerIndex { get; }

        // ── Health ────────────────────────────────────────────────────────────────
        public int  CurrentHealth { get; private set; }
        public bool IsAlive       => CurrentHealth > 0;

        // ── Mana ─────────────────────────────────────────────────────────────────
        /// <summary>Grows by 1 each turn the player starts, capped at MaxManaCap.</summary>
        public int MaxMana     { get; private set; }
        public int CurrentMana { get; private set; }

        // ── Zones ─────────────────────────────────────────────────────────────────
        public List<CardInstance>    Hand        { get; } = new();
        public Stack<CardInstance>   Deck        { get; } = new();
        public List<BattlefieldSlot> Battlefield { get; } = new();
        public List<CardInstance>    Graveyard   { get; } = new();

        // ── Constructor ───────────────────────────────────────────────────────────

        public PlayerState(int playerIndex)
        {
            PlayerIndex   = playerIndex;
            CurrentHealth = StartingHealth;
            MaxMana       = 0;
            CurrentMana   = 0;

            for (int i = 0; i < MaxBattlefieldSlots; i++)
                Battlefield.Add(new BattlefieldSlot(i));
        }

        // ── Health ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Modifies health by <paramref name="delta"/> (negative = damage, positive = heal).
        /// Clamps to [0, StartingHealth]. Returns the actual delta applied.
        /// </summary>
        public int ModifyHealth(int delta)
        {
            int previous  = CurrentHealth;
            CurrentHealth = Math.Clamp(CurrentHealth + delta, 0, StartingHealth);
            return CurrentHealth - previous;
        }

        // ── Mana ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Called at the start of this player's turn: increments MaxMana by 1 (capped)
        /// then fills CurrentMana to MaxMana.
        /// </summary>
        public void RefreshMana()
        {
            MaxMana    = Math.Min(MaxMana + 1, MaxManaCap);
            CurrentMana = MaxMana;
        }

        /// <summary>Spends mana. Returns false without spending if insufficient.</summary>
        public bool TrySpendMana(int amount)
        {
            if (CurrentMana < amount) return false;
            CurrentMana -= amount;
            return true;
        }

        /// <summary>Returns mana to the pool (e.g. when a play is aborted). Capped at MaxMana.</summary>
        public void RestoreMana(int amount)
        {
            CurrentMana = Math.Min(CurrentMana + amount, MaxMana);
        }

        // ── Battlefield helpers ───────────────────────────────────────────────────

        /// <summary>Returns the first empty battlefield slot, or null if the board is full.</summary>
        public BattlefieldSlot FirstEmptySlot()
            => Battlefield.Find(s => s.IsEmpty);

        /// <summary>Returns all slots that currently have a card.</summary>
        public List<BattlefieldSlot> OccupiedSlots()
            => Battlefield.FindAll(s => s.HasCard);
    }
}
