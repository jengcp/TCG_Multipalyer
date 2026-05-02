using UnityEngine;
using TCG.Core;
using TCG.Player;

namespace TCG.Cards
{
    /// <summary>
    /// Resolves a CardEffectData against a source owner and optional source card.
    /// </summary>
    public static class EffectResolver
    {
        public static void Resolve(CardEffectData effect, PlayerState caster, Card source = null)
        {
            var opponent = GameManager.Instance.GetOpponent(caster);

            switch (effect.effectType)
            {
                case EffectType.DealDamage:
                    ApplyDamage(effect, caster, opponent, source);
                    break;

                case EffectType.Heal:
                    ApplyHeal(effect, caster, opponent);
                    break;

                case EffectType.DrawCards:
                    DrawCards(effect, caster, opponent);
                    break;

                case EffectType.BuffAttack:
                    BuffCreatureAttack(effect, caster, source);
                    break;

                case EffectType.BuffDefense:
                    BuffCreatureDefense(effect, caster, source);
                    break;

                case EffectType.DestroyCreature:
                    DestroyCreature(effect, caster, opponent, source);
                    break;

                case EffectType.ReturnToHand:
                    ReturnToHand(effect, caster, opponent, source);
                    break;

                case EffectType.AddMana:
                    AddMana(effect, caster, opponent);
                    break;

                case EffectType.ApplyPoison:
                    ApplyStatus(effect, StatusEffect.Poisoned, caster, opponent, source);
                    break;

                case EffectType.Shield:
                    ApplyStatus(effect, StatusEffect.Shielded, caster, opponent, source);
                    break;

                case EffectType.Silence:
                    ApplyStatus(effect, StatusEffect.Silenced, caster, opponent, source);
                    break;
            }
        }

        private static void ApplyDamage(CardEffectData effect, PlayerState caster, PlayerState opponent, Card source)
        {
            switch (effect.targetType)
            {
                case TargetType.Opponent:
                    opponent.TakeDamage(effect.value);
                    break;
                case TargetType.Self:
                    caster.TakeDamage(effect.value);
                    break;
                case TargetType.AllEnemyCreatures:
                    foreach (var c in opponent.Field.Creatures)
                        c.TakeDamage(effect.value);
                    break;
                case TargetType.AllCreatures:
                    foreach (var c in caster.Field.Creatures) c.TakeDamage(effect.value);
                    foreach (var c in opponent.Field.Creatures) c.TakeDamage(effect.value);
                    break;
            }
        }

        private static void ApplyHeal(CardEffectData effect, PlayerState caster, PlayerState opponent)
        {
            switch (effect.targetType)
            {
                case TargetType.Self:
                    caster.Heal(effect.value);
                    break;
                case TargetType.Opponent:
                    opponent.Heal(effect.value);
                    break;
                case TargetType.AllFriendlyCreatures:
                    foreach (var c in caster.Field.Creatures) c.Heal(effect.value);
                    break;
            }
        }

        private static void DrawCards(CardEffectData effect, PlayerState caster, PlayerState opponent)
        {
            var target = effect.targetType == TargetType.Opponent ? opponent : caster;
            for (int i = 0; i < effect.value; i++)
                target.DrawCard();
        }

        private static void BuffCreatureAttack(CardEffectData effect, PlayerState caster, Card source)
        {
            switch (effect.targetType)
            {
                case TargetType.FriendlyCreature when source != null:
                    source.BuffAttack(effect.value);
                    break;
                case TargetType.AllFriendlyCreatures:
                    foreach (var c in caster.Field.Creatures) c.BuffAttack(effect.value);
                    break;
            }
        }

        private static void BuffCreatureDefense(CardEffectData effect, PlayerState caster, Card source)
        {
            switch (effect.targetType)
            {
                case TargetType.FriendlyCreature when source != null:
                    source.BuffDefense(effect.value);
                    break;
                case TargetType.AllFriendlyCreatures:
                    foreach (var c in caster.Field.Creatures) c.BuffDefense(effect.value);
                    break;
            }
        }

        private static void DestroyCreature(CardEffectData effect, PlayerState caster, PlayerState opponent, Card source)
        {
            // Targeted destruction is handled via targeting UI — here handle AoE variants
            if (effect.targetType == TargetType.AllEnemyCreatures)
            {
                var targets = new System.Collections.Generic.List<Card>(opponent.Field.Creatures);
                foreach (var c in targets) c.TakeDamage(int.MaxValue);
            }
            else if (effect.targetType == TargetType.AllCreatures)
            {
                var friends = new System.Collections.Generic.List<Card>(caster.Field.Creatures);
                var enemies = new System.Collections.Generic.List<Card>(opponent.Field.Creatures);
                foreach (var c in friends) c.TakeDamage(int.MaxValue);
                foreach (var c in enemies) c.TakeDamage(int.MaxValue);
            }
        }

        private static void ReturnToHand(CardEffectData effect, PlayerState caster, PlayerState opponent, Card source)
        {
            if (source == null) return;
            var owner = source.Owner;
            owner.Field.RemoveCard(source);
            source.SetZone(GameZone.Hand);
            owner.Hand.AddCard(source);
            GameEvents.CardReturnedToHand(source, owner);
        }

        private static void AddMana(CardEffectData effect, PlayerState caster, PlayerState opponent)
        {
            var target = effect.targetType == TargetType.Opponent ? opponent : caster;
            target.AddMana(effect.value);
        }

        private static void ApplyStatus(CardEffectData effect, StatusEffect status,
            PlayerState caster, PlayerState opponent, Card source)
        {
            switch (effect.targetType)
            {
                case TargetType.FriendlyCreature when source != null:
                    source.ApplyStatus(status);
                    break;
                case TargetType.EnemyCreature when source != null:
                    source.ApplyStatus(status);
                    break;
                case TargetType.AllFriendlyCreatures:
                    foreach (var c in caster.Field.Creatures) c.ApplyStatus(status);
                    break;
                case TargetType.AllEnemyCreatures:
                    foreach (var c in opponent.Field.Creatures) c.ApplyStatus(status);
                    break;
            }
        }
    }
}
