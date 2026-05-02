using UnityEngine;
using System.Collections.Generic;
using TCG.Core;
using TCG.Player;

namespace TCG.Characters
{
    /// <summary>
    /// Runtime state for one player's character.
    /// Lives outside the battlefield — targeted by specific effects only.
    /// </summary>
    public class CharacterState
    {
        public CharacterData Data { get; private set; }
        public PlayerState Owner { get; private set; }

        // Health
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        // Energy (carries over between turns, unlike mana)
        public int CurrentEnergy { get; private set; }
        public int MaxEnergy => Data.maxEnergy;

        // Per-ability usage tracking
        private int[] _cooldownsRemaining;  // turns left per ability index
        private int[] _usesThisTurn;        // times used this turn per ability index
        private bool _resilientProcd;       // Resilient keyword one-time proc tracker

        public CharacterState(CharacterData data, PlayerState owner)
        {
            Data = data;
            Owner = owner;

            MaxHealth = data.maxHealth;
            CurrentHealth = MaxHealth;
            CurrentEnergy = data.startingEnergy;

            _cooldownsRemaining = new int[data.abilities.Count];
            _usesThisTurn = new int[data.abilities.Count];
        }

        // ── Turn lifecycle ─────────────────────────────────────────────────

        public void OnTurnStart()
        {
            GainEnergy(Data.energyPerTurn +
                       (Data.HasKeyword(CharacterKeyword.Energized) ? 1 : 0));

            // Tick down cooldowns
            for (int i = 0; i < _cooldownsRemaining.Length; i++)
            {
                if (_cooldownsRemaining[i] > 0)
                {
                    _cooldownsRemaining[i]--;
                    GameEvents.AbilityCooldownTicked(this, i, _cooldownsRemaining[i]);
                }
            }

            // Reset per-turn use counts
            for (int i = 0; i < _usesThisTurn.Length; i++)
                _usesThisTurn[i] = 0;
        }

        // ── Energy ────────────────────────────────────────────────────────

        public void GainEnergy(int amount)
        {
            if (amount <= 0) return;
            CurrentEnergy = Mathf.Min(CurrentEnergy + amount, MaxEnergy);
            GameEvents.EnergyChanged(this, CurrentEnergy);
        }

        public bool SpendEnergy(int amount)
        {
            if (CurrentEnergy < amount) return false;
            CurrentEnergy -= amount;
            GameEvents.EnergyChanged(this, CurrentEnergy);
            return true;
        }

        public void AddEnergy(int amount) => GainEnergy(amount);

        public void DrainEnergy(int amount)
        {
            CurrentEnergy = Mathf.Max(0, CurrentEnergy - amount);
            GameEvents.EnergyChanged(this, CurrentEnergy);
        }

        // ── Health ────────────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive) return;

            // Resilient keyword: survive the first lethal hit once per game
            if (!_resilientProcd && Data.HasKeyword(CharacterKeyword.Resilient)
                && amount >= CurrentHealth)
            {
                _resilientProcd = true;
                CurrentHealth = 1;
                GameEvents.CharacterDamaged(this, amount);
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            GameEvents.CharacterDamaged(this, amount);

            if (!IsAlive)
                GameEvents.CharacterDied(this);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || !IsAlive) return;
            int prev = CurrentHealth;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            GameEvents.CharacterHealed(this, CurrentHealth - prev);
        }

        // ── Ability access ─────────────────────────────────────────────────

        public AbilityState GetAbilityState(int index)
        {
            if (!IsAlive) return AbilityState.Disabled;
            if (index < 0 || index >= Data.abilities.Count) return AbilityState.Disabled;

            var ability = Data.abilities[index];

            if (_cooldownsRemaining[index] > 0) return AbilityState.OnCooldown;
            if (CurrentEnergy < ability.energyCost) return AbilityState.NotEnoughEnergy;

            // Rapid keyword: allows 2 uses/turn; others: 1
            int maxUsesPerTurn = Data.HasKeyword(CharacterKeyword.Rapid) ? 2 : 1;
            if (_usesThisTurn[index] >= maxUsesPerTurn) return AbilityState.OnCooldown;

            return AbilityState.Ready;
        }

        public IReadOnlyList<CharacterAbilityData> Abilities => Data.abilities;

        public int GetCooldownRemaining(int index) =>
            (index >= 0 && index < _cooldownsRemaining.Length) ? _cooldownsRemaining[index] : 0;

        /// <summary>
        /// Spends energy and starts cooldown. Returns false if not ready.
        /// Actual effect resolution is handled by CharacterAbilityResolver.
        /// </summary>
        public bool TryConsumeAbility(int index)
        {
            if (GetAbilityState(index) != AbilityState.Ready) return false;

            var ability = Data.abilities[index];
            if (!SpendEnergy(ability.energyCost)) return false;

            _cooldownsRemaining[index] = ability.cooldownTurns;
            _usesThisTurn[index]++;
            GameEvents.AbilityUsed(this, index);
            return true;
        }

        // ── Passive keyword queries (used by other systems) ────────────────

        public bool HasKeyword(CharacterKeyword kw) => Data.HasKeyword(kw);

        /// <summary>Mana discount for spells granted by the Arcane keyword.</summary>
        public int SpellManaCostReduction => (IsAlive && HasKeyword(CharacterKeyword.Arcane)) ? 1 : 0;

        /// <summary>ATK bonus applied when a friendly creature enters the field (Warlord).</summary>
        public int WarlordEnterBonus => (IsAlive && HasKeyword(CharacterKeyword.Warlord)) ? 1 : 0;
    }
}
