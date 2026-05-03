using UnityEngine;
using System.Collections.Generic;
using TCG.Cards;
using TCG.Characters;
using TCG.Core;
using TCG.Game;

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

        public string PlayerId { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsLocalPlayer { get; private set; }

        public int Health { get; private set; }
        public int CurrentMana { get; private set; }
        public int MaxManaThisTurn { get; private set; }

        // Zones
        public Deck Deck { get; } = new Deck();
        public Hand Hand { get; } = new Hand();
        public Field Field { get; } = new Field();
        public ArtifactZone ArtifactZone { get; } = new ArtifactZone();
        public TrapZone TrapZone { get; } = new TrapZone();
        public Graveyard Graveyard { get; } = new Graveyard();

        // Character — optional
        public CharacterState Character { get; private set; }

        private bool _hasShield;

        public bool IsAlive => Health > 0;
        public int TurnNumber { get; private set; }

        // Whether this player is player1 (for fatigue tracking)
        public bool IsPlayer1 { get; private set; }

        public int CurrentEnergy => Character?.CurrentEnergy ?? 0;
        public int MaxEnergy => Character?.MaxEnergy ?? 0;

        public PlayerState(string id, string displayName, bool isLocal = false, bool isPlayer1 = true)
        {
            PlayerId = id;
            DisplayName = displayName;
            IsLocalPlayer = isLocal;
            IsPlayer1 = isPlayer1;
            Health = StartingHealth;
        }

        public void AssignCharacter(CharacterState character) => Character = character;

        // ── Turn lifecycle ─────────────────────────────────────────────────

        public void OnTurnStart()
        {
            TurnNumber++;
            MaxManaThisTurn = Mathf.Min(TurnNumber, MaxMana);
            CurrentMana = MaxManaThisTurn;
            GameEvents.ManaChanged(this, CurrentMana);

            // Haste creatures can attack the turn they arrive — no special handling needed
            // Regenerate creatures regain shield at turn start
            StatusEffectProcessor.ProcessStartOfTurn(this);

            // Artifact passive effects fire each turn
            ArtifactZone.OnTurnStart(this);

            // Check traps that trigger on turn start
            TrapZone.CheckTrigger(TrapTrigger.OnTurnStart, this);

            Character?.OnTurnStart();
        }

        public void OnTurnEnd()
        {
            // Poison ticks at end of turn
            StatusEffectProcessor.ProcessEndOfTurn(this);
        }

        // ── Mana ──────────────────────────────────────────────────────────

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
            if (_hasShield) { _hasShield = false; return; }
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

        // ── Draw ──────────────────────────────────────────────────────────

        /// <summary>
        /// Draws one card. Returns DrawResult so callers know what happened.
        /// Fatigue is applied automatically when deck is empty.
        /// </summary>
        public DrawResult DrawCard()
        {
            if (Deck.IsEmpty)
            {
                StatusEffectProcessor.ApplyFatigue(this, IsPlayer1);
                return DrawResult.FatigueDamage;
            }

            var card = Deck.DrawTop();
            if (!Hand.AddCard(card))
            {
                // Hand full — burned
                Graveyard.AddCard(card);
                GameEvents.DrawAttempt(this, DrawResult.HandFull);
                return DrawResult.HandFull;
            }

            GameEvents.CardDrawn(card, this);
            GameEvents.DrawAttempt(this, DrawResult.Success);
            return DrawResult.Success;
        }

        public void DrawStartingHand()
        {
            for (int i = 0; i < StartingHandSize; i++)
                DrawCard();
        }

        // ── Play card ─────────────────────────────────────────────────────

        /// <summary>
        /// Plays a card from hand. Handles all four card types correctly.
        /// BUG FIX: creatures now also trigger their onPlayEffects.
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

            switch (card.Data.cardType)
            {
                case CardType.Creature:
                    return PlayCreature(card, targetCreature);

                case CardType.Spell:
                    return PlaySpell(card, targetCreature);

                case CardType.Artifact:
                    return PlayArtifact(card);

                case CardType.Trap:
                    return SetTrap(card);
            }

            return false;
        }

        private bool PlayCreature(Card card, Card targetCreature)
        {
            if (!Field.AddCard(card)) return false;

            // Warlord bonus
            if (Character != null && Character.WarlordEnterBonus > 0)
                card.BuffAttack(Character.WarlordEnterBonus);

            // Haste: creatures without haste start exhausted (can't attack until next turn)
            if (!card.Data.hasHaste)
                card.Exhaust();

            GameEvents.CardPlayed(card, this);

            // ✅ BUG FIX — creature onPlayEffects now fire
            foreach (var effect in card.Data.onPlayEffects)
                EffectResolver.Resolve(effect, this, targetCreature);

            // Traps that watch for creature being played
            TrapZone.CheckTrigger(TrapTrigger.OnCreaturePlayed, this, card);

            return true;
        }

        private bool PlaySpell(Card card, Card targetCreature)
        {
            card.SetZone(GameZone.Graveyard);
            GameEvents.CardPlayed(card, this);

            foreach (var effect in card.Data.onPlayEffects)
                EffectResolver.Resolve(effect, this, targetCreature);

            Graveyard.AddCard(card);

            TrapZone.CheckTrigger(TrapTrigger.OnSpellPlayed, this, card);
            return true;
        }

        private bool PlayArtifact(Card card)
        {
            if (!ArtifactZone.AddArtifact(card)) return false;

            GameEvents.ArtifactPlayed(card, this);

            // On-play one-time effect still fires immediately
            if (!card.Data.artifactTriggersEachTurn)
            {
                foreach (var effect in card.Data.onPlayEffects)
                    EffectResolver.Resolve(effect, this, null);
            }

            return true;
        }

        private bool SetTrap(Card card)
        {
            if (!TrapZone.SetTrap(card)) return false;
            GameEvents.TrapSet(card, this);
            return true;
        }

        // ── Character ability ──────────────────────────────────────────────

        public bool UseCharacterAbility(int abilityIndex, Card targetCard = null)
        {
            if (Character == null || !Character.IsAlive) return false;
            if (!Character.TryConsumeAbility(abilityIndex)) return false;
            CharacterAbilityResolver.Resolve(Character, abilityIndex, targetCard);
            return true;
        }

        // ── Field management ───────────────────────────────────────────────

        public void RefreshField() => Field.RefreshAll();
    }
}
