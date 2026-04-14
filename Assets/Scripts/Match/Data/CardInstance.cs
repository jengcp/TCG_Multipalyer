using System;
using TCG.Items;

namespace TCG.Match
{
    /// <summary>
    /// Runtime wrapper around a CardData ScriptableObject.
    /// Tracks mutable in-match state: current health, modifiers, tapped/silenced flags.
    /// One instance is created per physical copy of a card in a shuffled deck.
    /// </summary>
    public class CardInstance
    {
        // ── Identity ──────────────────────────────────────────────────────────────
        public string   InstanceId { get; }
        public CardData BaseData   { get; }

        // ── Computed stats ────────────────────────────────────────────────────────
        /// <summary>Effective attack = base + buff modifiers.</summary>
        public int CurrentAttack  => BaseData.attackPower  + AttackModifier;
        /// <summary>Effective defense = base + buff modifiers. Reduces incoming damage.</summary>
        public int CurrentDefense => BaseData.defensePower + DefenseModifier;

        // ── Mutable state ─────────────────────────────────────────────────────────
        public int  CurrentHealth  { get; private set; }
        public int  AttackModifier { get; private set; }
        public int  DefenseModifier { get; private set; }

        public bool             IsTapped   { get; private set; }
        public bool             IsSilenced { get; private set; }
        public CardInstanceStatus Status   { get; private set; }

        public bool IsAlive => CurrentHealth > 0;

        // ── Constructor ───────────────────────────────────────────────────────────

        public CardInstance(CardData baseData)
        {
            if (baseData == null) throw new ArgumentNullException(nameof(baseData));
            InstanceId    = Guid.NewGuid().ToString("N");
            BaseData      = baseData;
            CurrentHealth = baseData.healthPoints;
            Status        = CardInstanceStatus.InDeck;
        }

        // ── Zone status ───────────────────────────────────────────────────────────

        public void SetStatus(CardInstanceStatus status) => Status = status;

        // ── Combat flags ──────────────────────────────────────────────────────────

        public void Tap()          => IsTapped   = true;
        public void Untap()        => IsTapped   = false;
        public void Silence()      => IsSilenced = true;
        public void RemoveSilence() => IsSilenced = false;

        // ── Health ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies raw incoming damage after subtracting this card's CurrentDefense.
        /// Returns the actual damage dealt (never negative).
        /// </summary>
        public int TakeDamage(int rawDamage)
        {
            int actual = Math.Max(0, rawDamage - CurrentDefense);
            CurrentHealth = Math.Max(0, CurrentHealth - actual);
            return actual;
        }

        /// <summary>Heals up to the card's base healthPoints maximum.</summary>
        public void Heal(int amount)
        {
            CurrentHealth = Math.Min(BaseData.healthPoints, CurrentHealth + amount);
        }

        // ── Modifiers ─────────────────────────────────────────────────────────────

        public void ApplyAttackBuff(int amount)  => AttackModifier  += amount;
        public void ApplyDefenseBuff(int amount) => DefenseModifier += amount;

        /// <summary>Removes all buff/debuff modifiers (used when returning to hand).</summary>
        public void ResetModifiers()
        {
            AttackModifier  = 0;
            DefenseModifier = 0;
        }

        public override string ToString() =>
            $"{BaseData.displayName} [{CurrentAttack}/{CurrentDefense}/{CurrentHealth}] {Status}";
    }
}
