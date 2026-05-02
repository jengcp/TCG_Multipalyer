using UnityEngine;
using System.Collections.Generic;
using TCG.Cards;
using TCG.Characters;
using TCG.Core;

namespace TCG.Player
{
    /// <summary>
    /// Owns and tracks all runtime state for one player.
    /// </summary>
    public class PlayerState
    {
        public const int StartingHealth = 30;
        public const int StartingHandSize = 4;
        public const int MaxMana = 10;
        public const int DrawsPerTurn = 1;

        public string PlayerId { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsLocalPlayer { get; private set; }

        public int Health { get; private set; }
        public int CurrentMana { get; private set; }
        public int MaxManaThisTurn { get; private set; }

        public Deck Deck { get; } = new Deck();
        public Hand Hand { get; } = new Hand();
        public Field Field { get; } = new Field();
        public Graveyard Graveyard { get; } = new Graveyard();

        // Character — may be null if none was chosen
        public CharacterState Character { get; private set; }

        // Shield — absorbs the next hit of player damage
        private bool _hasShield;

        public bool IsAlive => Health > 0;
        public int TurnNumber { get; private set; }

        // Energy convenience passthrough (delegates to Character when present)
        public int CurrentEnergy => Character?.CurrentEnergy ?? 0;
        public int MaxEnergy => Character?.MaxEnergy ?? 0;

        public PlayerState(string id, string displayName, bool isLocal = false)
        {
            PlayerId = id;
            DisplayName = displayName;
            IsLocalPlayer = isLocal;
            Health = StartingHealth;
            CurrentMana = 0;
            MaxManaThisTurn = 0;
            TurnNumber = 0;
        }

        public void AssignCharacter(CharacterState character)
        {
            Character = character;
        }

        // ── Mana ──────────────────────────────────────────────────────────

        public void StartTurnMana()
        {
            TurnNumber++;
            MaxManaThisTurn = Mathf.Min(TurnNumber, MaxMana);
            CurrentMana = MaxManaThisTurn;
            GameEvents.ManaChanged(this, CurrentMana);
        }

        public bool SpendMana(int amount)
        {
            if (CurrentMana < amount) return false;
            CurrentMana -= amount;
            GameEvents.ManaChanged(this, CurrentMana);
            return true;
        }

        public void AddMana(int amount)
        {
            CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
            GameEvents.ManaChanged(this, CurrentMana);
        }

        // ── Health ────────────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;

            if (_hasShield)
            {
                _hasShield = false;
                return;
            }

            Health = Mathf.Max(0, Health - amount);
            GameEvents.PlayerDamaged(this, amount);
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;
            int prev = Health;
            Health = Mathf.Min(Health + amount, StartingHealth);
            GameEvents.PlayerHealed(this, Health - prev);
        }

        public void ApplyShield() => _hasShield = true;

        // ── Turn start ────────────────────────────────────────────────────

        public void OnTurnStart()
        {
            StartTurnMana();
            Character?.OnTurnStart();
        }

        // ── Card actions ──────────────────────────────────────────────────

        public bool DrawCard()
        {
            if (Deck.IsEmpty) return false;
            var card = Deck.DrawTop();
            if (!Hand.AddCard(card))
            {
                Graveyard.AddCard(card);
                return false;
            }
            GameEvents.CardDrawn(card, this);
            return true;
        }

        public void DrawStartingHand()
        {
            for (int i = 0; i < StartingHandSize; i++)
                DrawCard();
        }

        /// <summary>
        /// Returns true if the card was successfully played from hand.
        /// Applies Arcane mana discount when character is alive.
        /// Applies Warlord ATK bonus when character is alive.
        /// </summary>
        public bool PlayCard(Card card, Card targetCreature = null)
        {
            if (!Hand.ContainsCard(card)) return false;

            int effectiveCost = card.Data.manaCost;

            // Arcane keyword: spells cost 1 less
            if (!card.Data.IsCreature && Character != null)
                effectiveCost = Mathf.Max(0, effectiveCost - Character.SpellManaCostReduction);

            if (!SpendMana(effectiveCost)) return false;

            Hand.RemoveCard(card);

            if (card.Data.IsCreature)
            {
                if (!Field.AddCard(card)) return false;

                // Warlord keyword: creatures enter with +1 ATK
                if (Character != null && Character.WarlordEnterBonus > 0)
                    card.BuffAttack(Character.WarlordEnterBonus);

                GameEvents.CardPlayed(card, this);
            }
            else
            {
                card.SetZone(GameZone.Graveyard);
                GameEvents.CardPlayed(card, this);

                foreach (var effect in card.Data.onPlayEffects)
                    EffectResolver.Resolve(effect, this, targetCreature);

                Graveyard.AddCard(card);
            }

            return true;
        }

        /// <summary>
        /// Uses a character ability by index. Returns false if not ready or no character.
        /// </summary>
        public bool UseCharacterAbility(int abilityIndex, Card targetCard = null)
        {
            if (Character == null || !Character.IsAlive) return false;
            if (!Character.TryConsumeAbility(abilityIndex)) return false;

            CharacterAbilityResolver.Resolve(Character, abilityIndex, targetCard);
            return true;
        }

        public void RefreshField() => Field.RefreshAll();
    }
}
