using UnityEngine;
using System.Collections.Generic;
using TCG.Core;

namespace TCG.Cards
{
    /// <summary>
    /// Runtime instance of a card. Holds current (possibly modified) stats
    /// and tracks which zone/owner it belongs to.
    /// </summary>
    public class Card : MonoBehaviour
    {
        public CardData Data { get; private set; }
        public Player.PlayerState Owner { get; private set; }
        public GameZone Zone { get; private set; }

        // Runtime stats (can be buffed/debuffed)
        public int CurrentAttack { get; private set; }
        public int CurrentDefense { get; private set; }
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }

        public bool IsExhausted { get; private set; }
        public bool CanAttack => !IsExhausted && Zone == GameZone.Field && Data.IsCreature;
        public bool IsAlive => CurrentHealth > 0;

        private List<StatusEffect> _activeStatusEffects = new List<StatusEffect>();

        public void Initialize(CardData data, Player.PlayerState owner)
        {
            Data = data;
            Owner = owner;
            Zone = GameZone.Deck;

            CurrentAttack = data.baseAttack;
            CurrentDefense = data.baseDefense;
            CurrentHealth = data.baseHealth;
            MaxHealth = data.baseHealth;
            IsExhausted = false;
        }

        public void SetZone(GameZone zone)
        {
            Zone = zone;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;

            // Shield absorbs one hit
            if (_activeStatusEffects.Contains(StatusEffect.Shielded))
            {
                RemoveStatus(StatusEffect.Shielded);
                return;
            }

            int effectiveDamage = Mathf.Max(0, amount - CurrentDefense);
            CurrentHealth -= effectiveDamage;
            GameEvents.CreatureDamaged(this, effectiveDamage);

            if (CurrentHealth <= 0)
                Die();
        }

        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        }

        public void BuffAttack(int amount)
        {
            CurrentAttack = Mathf.Max(0, CurrentAttack + amount);
        }

        public void BuffDefense(int amount)
        {
            CurrentDefense = Mathf.Max(0, CurrentDefense + amount);
        }

        public void Exhaust()
        {
            IsExhausted = true;
        }

        public void Refresh()
        {
            IsExhausted = false;
        }

        public void ApplyStatus(StatusEffect status)
        {
            if (!_activeStatusEffects.Contains(status))
                _activeStatusEffects.Add(status);
        }

        public void RemoveStatus(StatusEffect status)
        {
            _activeStatusEffects.Remove(status);
        }

        public bool HasStatus(StatusEffect status) => _activeStatusEffects.Contains(status);

        private void Die()
        {
            CurrentHealth = 0;
            GameEvents.CreatureDied(this);

            foreach (var effect in Data.onDeathEffects)
                EffectResolver.Resolve(effect, Owner, this);

            SetZone(GameZone.Graveyard);
            Owner.Field.RemoveCard(this);
            Owner.Graveyard.AddCard(this);
            GameEvents.CardDestroyed(this, Owner);
        }

        public override string ToString()
        {
            return $"[{Data.cardName} ATK:{CurrentAttack} DEF:{CurrentDefense} HP:{CurrentHealth}/{MaxHealth}]";
        }
    }
}
