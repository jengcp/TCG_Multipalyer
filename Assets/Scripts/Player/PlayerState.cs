using UnityEngine;
using System.Collections.Generic;
using TCG.Cards;
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

        public bool IsAlive => Health > 0;
        public int TurnNumber { get; private set; }

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

        // ── Card actions ──────────────────────────────────────────────────

        public bool DrawCard()
        {
            if (Deck.IsEmpty) return false;
            var card = Deck.DrawTop();
            if (!Hand.AddCard(card))
            {
                // Hand full — card is burned
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
        /// </summary>
        public bool PlayCard(Card card, Card targetCreature = null)
        {
            if (!Hand.ContainsCard(card)) return false;
            if (!SpendMana(card.Data.manaCost)) return false;

            Hand.RemoveCard(card);

            if (card.Data.IsCreature)
            {
                if (!Field.AddCard(card)) return false;
                GameEvents.CardPlayed(card, this);
            }
            else
            {
                // Spell / artifact / trap
                card.SetZone(GameZone.Graveyard);
                GameEvents.CardPlayed(card, this);

                foreach (var effect in card.Data.onPlayEffects)
                    EffectResolver.Resolve(effect, this, targetCreature);

                Graveyard.AddCard(card);
            }

            return true;
        }

        public void RefreshField() => Field.RefreshAll();
    }
}
