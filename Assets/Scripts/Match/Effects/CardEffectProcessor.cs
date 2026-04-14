using System.Collections.Generic;
using UnityEngine;
using TCG.Core;

namespace TCG.Match.Effects
{
    /// <summary>
    /// Static class that executes card effects defined in <see cref="CardEffectData"/>.
    /// Also exposes zone-manipulation helpers used by CombatResolver.
    /// </summary>
    public static class CardEffectProcessor
    {
        // ── Main entry point ───────────────────────────────────────────────────────

        /// <summary>
        /// Processes every effect in <paramref name="effectData"/> for the given played card.
        /// Silenced cards skip all effects. Pass <paramref name="explicitTarget"/> = null for
        /// non-targeted effects; targeted effects (TargetCreature) must supply one.
        /// </summary>
        public static void ProcessEffects(
            CardEffectData effectData,
            CardInstance   source,
            CardInstance   explicitTarget,
            MatchState     state)
        {
            if (effectData == null || effectData.effects == null) return;
            if (source.IsSilenced) return;

            foreach (var entry in effectData.effects)
            {
                // AllCreatures: loop every occupied slot for both players
                if (entry.targetType == EffectTargetType.AllCreatures)
                {
                    foreach (var player in state.Players)
                        foreach (var slot in player.Battlefield)
                            if (slot.HasCard)
                                ExecuteEffect(entry.effectType, entry.magnitude,
                                    source, slot.Occupant, state);
                    continue;
                }

                CardInstance resolved = ResolveTarget(
                    entry.targetType, source, explicitTarget, state);

                ExecuteEffect(entry.effectType, entry.magnitude, source, resolved, state);
            }
        }

        // ── Per-type logic ─────────────────────────────────────────────────────────

        private static void ExecuteEffect(
            CardEffectType type,
            int            magnitude,
            CardInstance   source,
            CardInstance   target,
            MatchState     state)
        {
            switch (type)
            {
                case CardEffectType.Damage:
                    if (target != null)
                    {
                        int pi  = GetOwnerIndex(target, state);
                        int dmg = target.TakeDamage(magnitude);
                        GameEvents.RaiseDamageDealt(dmg, target, pi);
                        if (!target.IsAlive && pi >= 0)
                            KillCreature(target, pi, state);
                    }
                    break;

                case CardEffectType.Heal:
                    if (target != null)
                    {
                        target.Heal(magnitude);
                    }
                    else
                    {
                        // Heal the owning player
                        var owner = GetOwnerState(source, state);
                        if (owner != null)
                        {
                            int delta = owner.ModifyHealth(magnitude);
                            GameEvents.RaisePlayerHealthChanged(
                                owner.PlayerIndex, owner.CurrentHealth, delta);
                        }
                    }
                    break;

                case CardEffectType.DrawCard:
                    var caster = GetOwnerState(source, state);
                    if (caster != null)
                        for (int i = 0; i < magnitude; i++)
                            DrawCardForPlayer(caster, state);
                    break;

                case CardEffectType.BuffAttack:
                    target?.ApplyAttackBuff(magnitude);
                    break;

                case CardEffectType.BuffDefense:
                    target?.ApplyDefenseBuff(magnitude);
                    break;

                case CardEffectType.DestroyCreature:
                    if (target != null)
                    {
                        int pi = GetOwnerIndex(target, state);
                        if (pi >= 0) KillCreature(target, pi, state);
                    }
                    break;

                case CardEffectType.ReturnToHand:
                    if (target != null) ReturnCreatureToHand(target, state);
                    break;

                case CardEffectType.Silence:
                    target?.Silence();
                    break;
            }
        }

        // ── Target resolution ──────────────────────────────────────────────────────

        private static CardInstance ResolveTarget(
            EffectTargetType type,
            CardInstance     source,
            CardInstance     explicitTarget,
            MatchState       state)
        {
            return type switch
            {
                EffectTargetType.Self           => source,
                EffectTargetType.TargetCreature => explicitTarget,
                EffectTargetType.RandomCreature => PickRandomCreature(state),
                _                               => null   // Opponent and AllCreatures handled by callers
            };
        }

        // ── Public zone helpers (also used by CombatResolver) ─────────────────────

        /// <summary>Draws the top card of the player's deck into their hand.</summary>
        public static void DrawCardForPlayer(PlayerState player, MatchState state)
        {
            if (player.Deck.Count == 0)
            {
                Debug.Log($"[CardEffectProcessor] Player {player.PlayerIndex} has no cards to draw.");
                return; // Deck-empty win condition checked by MatchManager
            }

            var card = player.Deck.Pop();
            card.SetStatus(CardInstanceStatus.InHand);
            player.Hand.Add(card);
            GameEvents.RaiseCardDrawn(card, player.PlayerIndex);
        }

        /// <summary>Moves a creature from the battlefield to the graveyard.</summary>
        public static void KillCreature(CardInstance card, int playerIndex, MatchState state)
        {
            var owner = state.Players[playerIndex];
            var slot  = owner.Battlefield.Find(s => s.Occupant == card);
            slot?.RemoveCard();

            card.SetStatus(CardInstanceStatus.InGraveyard);
            owner.Graveyard.Add(card);

            GameEvents.RaiseCreatureDied(card, playerIndex);
            GameEvents.RaiseCardSentToGraveyard(card, playerIndex);
        }

        /// <summary>Returns a creature from the battlefield back to its owner's hand and resets buffs.</summary>
        public static void ReturnCreatureToHand(CardInstance card, MatchState state)
        {
            for (int i = 0; i < state.Players.Length; i++)
            {
                var slot = state.Players[i].Battlefield.Find(s => s.Occupant == card);
                if (slot == null) continue;

                slot.RemoveCard();
                card.SetStatus(CardInstanceStatus.InHand);
                card.ResetModifiers();
                state.Players[i].Hand.Add(card);

                GameEvents.RaiseCardReturnedToHand(card, i);
                return;
            }
        }

        // ── Utility ───────────────────────────────────────────────────────────────

        private static int GetOwnerIndex(CardInstance card, MatchState state)
        {
            for (int i = 0; i < state.Players.Length; i++)
                if (state.Players[i].Battlefield.Exists(s => s.Occupant == card))
                    return i;
            return -1;
        }

        private static PlayerState GetOwnerState(CardInstance card, MatchState state)
        {
            foreach (var p in state.Players)
                if (p.Hand.Contains(card) ||
                    p.Battlefield.Exists(s => s.Occupant == card))
                    return p;
            return null;
        }

        private static CardInstance PickRandomCreature(MatchState state)
        {
            var all = new List<CardInstance>();
            foreach (var p in state.Players)
                foreach (var s in p.Battlefield)
                    if (s.HasCard) all.Add(s.Occupant);

            return all.Count == 0 ? null : all[Random.Range(0, all.Count)];
        }
    }
}
