using System;
using System.Collections.Generic;
using TCG.Cards;
using TCG.Characters;
using TCG.Player;

namespace TCG.Core
{
    /// <summary>
    /// Central event bus — subscribe/unsubscribe to decouple systems.
    /// </summary>
    public static class GameEvents
    {
        // Turn flow
        public static event Action<GamePhase> OnPhaseChanged;
        public static event Action<PlayerState> OnTurnStarted;
        public static event Action<PlayerState> OnTurnEnded;

        // Card lifecycle
        public static event Action<Card, PlayerState> OnCardDrawn;
        public static event Action<Card, PlayerState> OnCardPlayed;
        public static event Action<Card, PlayerState> OnCardDestroyed;
        public static event Action<Card, PlayerState> OnCardReturnedToHand;

        // Combat
        public static event Action<Card, Card> OnAttackDeclared;
        public static event Action<Card, PlayerState> OnAttackOnPlayer;
        public static event Action<Card, int> OnCreatureDamaged;
        public static event Action<Card> OnCreatureDied;

        // Combat flow (blocker system)
        public static event Action<BattleSubPhase> OnBattleSubPhaseChanged;
        public static event Action<List<Card>> OnAttackersDeclared;           // final list
        public static event Action<Card, Card> OnBlockerAssigned;             // blocker, attacker
        public static event Action OnBlockersConfirmed;
        public static event Action<PlayerState, DrawResult, int> OnDrawAttempt; // player, result, fatigueDmg

        // Trap / Artifact
        public static event Action<Card, PlayerState> OnTrapSet;
        public static event Action<Card, PlayerState, TrapTrigger> OnTrapTriggered;
        public static event Action<Card, PlayerState> OnArtifactPlayed;

        // Player
        public static event Action<PlayerState, int> OnPlayerDamaged;
        public static event Action<PlayerState, int> OnPlayerHealed;
        public static event Action<PlayerState, int> OnManaChanged;

        // Character & Energy
        public static event Action<CharacterState, int> OnCharacterDamaged;
        public static event Action<CharacterState, int> OnCharacterHealed;
        public static event Action<CharacterState> OnCharacterDied;
        public static event Action<CharacterState, int> OnEnergyChanged;       // character, newEnergy
        public static event Action<CharacterState, int> OnAbilityUsed;         // character, abilityIndex
        public static event Action<CharacterState, int, int> OnAbilityCooldownTicked; // character, index, turnsLeft

        // Game
        public static event Action<GameResult> OnGameEnded;

        // Invocations
        public static void PhaseChanged(GamePhase phase) => OnPhaseChanged?.Invoke(phase);
        public static void TurnStarted(PlayerState p) => OnTurnStarted?.Invoke(p);
        public static void TurnEnded(PlayerState p) => OnTurnEnded?.Invoke(p);

        public static void CardDrawn(Card c, PlayerState p) => OnCardDrawn?.Invoke(c, p);
        public static void CardPlayed(Card c, PlayerState p) => OnCardPlayed?.Invoke(c, p);
        public static void CardDestroyed(Card c, PlayerState p) => OnCardDestroyed?.Invoke(c, p);
        public static void CardReturnedToHand(Card c, PlayerState p) => OnCardReturnedToHand?.Invoke(c, p);

        public static void AttackDeclared(Card attacker, Card target) => OnAttackDeclared?.Invoke(attacker, target);
        public static void AttackOnPlayer(Card attacker, PlayerState defender) => OnAttackOnPlayer?.Invoke(attacker, defender);
        public static void CreatureDamaged(Card c, int dmg) => OnCreatureDamaged?.Invoke(c, dmg);
        public static void CreatureDied(Card c) => OnCreatureDied?.Invoke(c);

        public static void BattleSubPhaseChanged(BattleSubPhase p) => OnBattleSubPhaseChanged?.Invoke(p);
        public static void AttackersDeclared(List<Card> list) => OnAttackersDeclared?.Invoke(list);
        public static void BlockerAssigned(Card blocker, Card attacker) => OnBlockerAssigned?.Invoke(blocker, attacker);
        public static void BlockersConfirmed() => OnBlockersConfirmed?.Invoke();
        public static void DrawAttempt(PlayerState p, DrawResult r, int dmg = 0) => OnDrawAttempt?.Invoke(p, r, dmg);

        public static void TrapSet(Card c, PlayerState p) => OnTrapSet?.Invoke(c, p);
        public static void TrapTriggered(Card c, PlayerState p, TrapTrigger t) => OnTrapTriggered?.Invoke(c, p, t);
        public static void ArtifactPlayed(Card c, PlayerState p) => OnArtifactPlayed?.Invoke(c, p);

        public static void PlayerDamaged(PlayerState p, int dmg) => OnPlayerDamaged?.Invoke(p, dmg);
        public static void PlayerHealed(PlayerState p, int amt) => OnPlayerHealed?.Invoke(p, amt);
        public static void ManaChanged(PlayerState p, int newMana) => OnManaChanged?.Invoke(p, newMana);

        public static void CharacterDamaged(CharacterState c, int dmg) => OnCharacterDamaged?.Invoke(c, dmg);
        public static void CharacterHealed(CharacterState c, int amt) => OnCharacterHealed?.Invoke(c, amt);
        public static void CharacterDied(CharacterState c) => OnCharacterDied?.Invoke(c);
        public static void EnergyChanged(CharacterState c, int e) => OnEnergyChanged?.Invoke(c, e);
        public static void AbilityUsed(CharacterState c, int idx) => OnAbilityUsed?.Invoke(c, idx);
        public static void AbilityCooldownTicked(CharacterState c, int idx, int left) => OnAbilityCooldownTicked?.Invoke(c, idx, left);

        public static void GameEnded(GameResult result) => OnGameEnded?.Invoke(result);

        public static void ClearAll()
        {
            OnPhaseChanged = null;
            OnTurnStarted = null;
            OnTurnEnded = null;
            OnCardDrawn = null;
            OnCardPlayed = null;
            OnCardDestroyed = null;
            OnCardReturnedToHand = null;
            OnAttackDeclared = null;
            OnAttackOnPlayer = null;
            OnCreatureDamaged = null;
            OnCreatureDied = null;
            OnBattleSubPhaseChanged = null;
            OnAttackersDeclared = null;
            OnBlockerAssigned = null;
            OnBlockersConfirmed = null;
            OnDrawAttempt = null;
            OnTrapSet = null;
            OnTrapTriggered = null;
            OnArtifactPlayed = null;
            OnPlayerDamaged = null;
            OnPlayerHealed = null;
            OnManaChanged = null;
            OnCharacterDamaged = null;
            OnCharacterHealed = null;
            OnCharacterDied = null;
            OnEnergyChanged = null;
            OnAbilityUsed = null;
            OnAbilityCooldownTicked = null;
            OnGameEnded = null;
        }
    }
}
