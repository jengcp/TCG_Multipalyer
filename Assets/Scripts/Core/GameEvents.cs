using System;
using TCG.Cards;
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
        public static event Action<Card, Card> OnAttackDeclared;      // attacker, target card (nullable)
        public static event Action<Card, PlayerState> OnAttackOnPlayer; // attacker, defending player
        public static event Action<Card, int> OnCreatureDamaged;
        public static event Action<Card> OnCreatureDied;

        // Player
        public static event Action<PlayerState, int> OnPlayerDamaged;
        public static event Action<PlayerState, int> OnPlayerHealed;
        public static event Action<PlayerState, int> OnManaChanged;

        // Game
        public static event Action<GameResult> OnGameEnded;

        // Invocations (called by game systems)
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

        public static void PlayerDamaged(PlayerState p, int dmg) => OnPlayerDamaged?.Invoke(p, dmg);
        public static void PlayerHealed(PlayerState p, int amt) => OnPlayerHealed?.Invoke(p, amt);
        public static void ManaChanged(PlayerState p, int newMana) => OnManaChanged?.Invoke(p, newMana);

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
            OnPlayerDamaged = null;
            OnPlayerHealed = null;
            OnManaChanged = null;
            OnGameEnded = null;
        }
    }
}
