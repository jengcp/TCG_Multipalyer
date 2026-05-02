using UnityEngine;
using System.Collections.Generic;
using TCG.Cards;
using TCG.Core;
using TCG.Player;

namespace TCG.Characters
{
    /// <summary>
    /// Resolves the effect of a CharacterAbilityData after CharacterState has
    /// already consumed the energy and set the cooldown.
    /// </summary>
    public static class CharacterAbilityResolver
    {
        /// <param name="caster">The character using the ability.</param>
        /// <param name="abilityIndex">Index into caster.Data.abilities.</param>
        /// <param name="targetCard">Optional creature target (for single-target effects).</param>
        public static void Resolve(CharacterState caster, int abilityIndex, Card targetCard = null)
        {
            if (abilityIndex < 0 || abilityIndex >= caster.Abilities.Count) return;

            var ability = caster.Abilities[abilityIndex];
            var owner = caster.Owner;
            var opponent = GameManager.Instance.GetOpponent(owner);
            var opponentCharacter = opponent.Character;

            switch (ability.effectType)
            {
                case CharacterEffectType.DealDamageToPlayer:
                    DealDamageToPlayer(ability, owner, opponent, targetCard);
                    break;

                case CharacterEffectType.DealDamageToAllCreatures:
                    DealDamageToAllCreatures(ability, owner, opponent);
                    break;

                case CharacterEffectType.HealPlayer:
                    owner.Heal(ability.value);
                    break;

                case CharacterEffectType.HealCharacter:
                    caster.Heal(ability.value);
                    break;

                case CharacterEffectType.DrawCards:
                    for (int i = 0; i < ability.value; i++) owner.DrawCard();
                    break;

                case CharacterEffectType.BuffAllFriendlyCreatures:
                    BuffAllFriendly(ability, owner);
                    break;

                case CharacterEffectType.SummonToken:
                    SummonToken(ability, owner);
                    break;

                case CharacterEffectType.StealCreature:
                    StealCreature(ability, owner, opponent, targetCard);
                    break;

                case CharacterEffectType.ResurrectCreature:
                    ResurrectCreature(ability, owner);
                    break;

                case CharacterEffectType.AddEnergy:
                    caster.AddEnergy(ability.value);
                    break;

                case CharacterEffectType.DrainOpponentEnergy:
                    if (opponentCharacter != null)
                        opponentCharacter.DrainEnergy(ability.value);
                    break;

                case CharacterEffectType.DoubleManaThisTurn:
                    owner.AddMana(owner.CurrentMana); // double by adding current again
                    break;

                case CharacterEffectType.ShieldPlayerOnce:
                    owner.ApplyShield();
                    break;

                case CharacterEffectType.DestroyAllCreatures:
                    DestroyAllCreatures(owner, opponent);
                    break;

                case CharacterEffectType.GiveCreatureKeyword:
                    if (targetCard != null)
                        targetCard.ApplyStatus(ability.keywordToGrant);
                    break;
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static void DealDamageToPlayer(CharacterAbilityData ability,
            PlayerState owner, PlayerState opponent, Card targetCard)
        {
            switch (ability.targetType)
            {
                case TargetType.Opponent:
                    opponent.TakeDamage(ability.value);
                    break;
                case TargetType.EnemyCreature when targetCard != null:
                    targetCard.TakeDamage(ability.value);
                    break;
                case TargetType.Self:
                    owner.TakeDamage(ability.value);
                    break;
            }
        }

        private static void DealDamageToAllCreatures(CharacterAbilityData ability,
            PlayerState owner, PlayerState opponent)
        {
            var friendlies = new List<Card>(owner.Field.Creatures);
            var enemies = new List<Card>(opponent.Field.Creatures);

            switch (ability.targetType)
            {
                case TargetType.AllEnemyCreatures:
                    foreach (var c in enemies) c.TakeDamage(ability.value);
                    break;
                case TargetType.AllCreatures:
                    foreach (var c in friendlies) c.TakeDamage(ability.value);
                    foreach (var c in enemies) c.TakeDamage(ability.value);
                    break;
                case TargetType.AllFriendlyCreatures:
                    foreach (var c in friendlies) c.TakeDamage(ability.value);
                    break;
            }
        }

        private static void BuffAllFriendly(CharacterAbilityData ability, PlayerState owner)
        {
            foreach (var c in owner.Field.Creatures)
            {
                c.BuffAttack(ability.value);
                c.BuffDefense(ability.value);
            }
        }

        private static void SummonToken(CharacterAbilityData ability, PlayerState owner)
        {
            if (ability.tokenData == null || owner.Field.IsFull) return;

            var tokenGO = Object.Instantiate(GameManager.Instance.CardPrefab,
                GameManager.Instance.transform);
            tokenGO.Initialize(ability.tokenData, owner);
            tokenGO.SetZone(GameZone.Field);
            owner.Field.AddCard(tokenGO);
            GameEvents.CardPlayed(tokenGO, owner);
        }

        private static void StealCreature(CharacterAbilityData ability,
            PlayerState owner, PlayerState opponent, Card targetCard)
        {
            if (targetCard == null || !opponent.Field.ContainsCard(targetCard)) return;
            if (owner.Field.IsFull) return;

            opponent.Field.RemoveCard(targetCard);
            // Re-assign ownership is not directly supported in this architecture
            // so we move the card to the new field and mark it as exhausted
            owner.Field.AddCard(targetCard);
            targetCard.Exhaust();
        }

        private static void ResurrectCreature(CharacterAbilityData ability, PlayerState owner)
        {
            if (owner.Graveyard.Count == 0 || owner.Field.IsFull) return;

            // Resurrect the most recently dead creature
            Card target = null;
            var graves = owner.Graveyard.Cards;
            for (int i = graves.Count - 1; i >= 0; i--)
            {
                if (graves[i].Data.IsCreature) { target = graves[i]; break; }
            }
            if (target == null) return;

            owner.Graveyard.RemoveCard(target);
            target.Heal(target.Data.baseHealth); // restore to full
            owner.Field.AddCard(target);
            GameEvents.CardPlayed(target, owner);
        }

        private static void DestroyAllCreatures(PlayerState owner, PlayerState opponent)
        {
            var all = new List<Card>(owner.Field.Creatures);
            all.AddRange(opponent.Field.Creatures);
            foreach (var c in all) c.TakeDamage(int.MaxValue);
        }
    }
}
