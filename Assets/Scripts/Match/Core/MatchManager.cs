using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCG.Core;
using TCG.Inventory.Deck;
using TCG.Items;

namespace TCG.Match
{
    /// <summary>
    /// Singleton MonoBehaviour that owns the full match lifecycle:
    /// deck building → dealing → turn loop → win condition → rewards.
    ///
    /// Call <see cref="StartMatch"/> to begin. Phases are driven by
    /// <see cref="TurnManager"/>; this class provides RunMainPhase and
    /// RunCombatPhase which TurnManager calls back into.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        // ── State ──────────────────────────────────────────────────────────────────
        private MatchState        _state;
        private IPlayerController _controllers0;
        private IPlayerController _controllers1;
        private LocalPlayerController _localController;
        private bool              _matchRunning;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes and starts a match between two decks with the given controllers.
        /// Pass a <see cref="LocalPlayerController"/> as p0Controller for the local human player.
        /// </summary>
        public void StartMatch(
            DeckData          p0Deck,
            DeckData          p1Deck,
            IPlayerController p0Controller,
            IPlayerController p1Controller)
        {
            _controllers0    = p0Controller;
            _controllers1    = p1Controller;
            _localController = p0Controller as LocalPlayerController;

            // Build player states
            var p0 = new PlayerState(0);
            var p1 = new PlayerState(1);
            _state = new MatchState(p0, p1);

            // Populate and shuffle decks
            LoadDeckIntoState(p0Deck, p0);
            LoadDeckIntoState(p1Deck, p1);

            // Deal opening hand (5 cards each)
            for (int i = 0; i < 5; i++)
            {
                Effects.CardEffectProcessor.DrawCardForPlayer(p0, _state);
                Effects.CardEffectProcessor.DrawCardForPlayer(p1, _state);
            }

            // Give player 0 their first turn's mana
            p0.RefreshMana();

            TurnManager.Instance.Initialize(_state);

            GameEvents.RaiseMatchStarted(_state);
            _matchRunning = true;

            StartCoroutine(MatchLoop());
        }

        /// <summary>
        /// Flushes all pending local player input (called by EndTurnButtonUI alongside AdvancePhase).
        /// </summary>
        public void FlushPendingInput()
        {
            _localController?.FlushPendingInput();
        }

        // ── Match Loop ─────────────────────────────────────────────────────────────

        private IEnumerator MatchLoop()
        {
            while (_matchRunning)
            {
                var controller = _state.CurrentPlayerIndex == 0 ? _controllers0 : _controllers1;
                yield return TurnManager.Instance.RunTurn(controller, this);

                if (CheckWinConditions()) yield break;
            }
        }

        // ── Phase runners (called by TurnManager) ──────────────────────────────────

        /// <summary>
        /// Main Phase loop: repeatedly ask the controller to pick a card until null (pass) or phase ends.
        /// </summary>
        public IEnumerator RunMainPhase(IPlayerController controller)
        {
            var player = _state.ActivePlayer;

            while (_state.Phase == MatchPhase.MainPhase)
            {
                CardInstance chosen = null;
                yield return controller.SelectCardToPlay(player, c => chosen = c);

                if (chosen == null) yield break; // player passed

                TryPlayCard(chosen, player);

                if (CheckWinConditions()) yield break;
            }
        }

        /// <summary>
        /// Combat Phase: for each occupied slot, ask controller to declare an attacker,
        /// then ask for a defender (or direct). Stops when the controller returns null.
        /// </summary>
        public IEnumerator RunCombatPhase(IPlayerController controller)
        {
            var attacker = _state.ActivePlayer;
            var defender = _state.OpponentPlayer;

            // We iterate by index because KillCreature may shrink OccupiedSlots mid-loop
            while (_state.Phase == MatchPhase.CombatPhase)
            {
                BattlefieldSlot attackSlot = null;
                yield return controller.SelectAttackSlot(attacker, s => attackSlot = s);

                if (attackSlot == null) yield break; // no more attacks

                var attackingCard = attackSlot.Occupant;
                if (attackingCard == null || attackingCard.IsTapped) continue;

                BattlefieldSlot defendSlot = null;
                yield return controller.SelectDefendSlot(defender, s => defendSlot = s);

                if (defendSlot != null && defendSlot.HasCard)
                {
                    CombatResolver.ResolveAttack(
                        attackingCard, defendSlot.Occupant,
                        attacker.PlayerIndex, defender.PlayerIndex, _state);
                }
                else
                {
                    CombatResolver.ResolveDirect(attackingCard, defender, attacker.PlayerIndex, _state);
                }

                if (CheckWinConditions()) yield break;
            }
        }

        // ── Win Condition ──────────────────────────────────────────────────────────

        /// <summary>
        /// Checks health and deck-empty conditions.
        /// Sets MatchOver + Result on the state, then calls EndMatch.
        /// Returns true if the match is over.
        /// </summary>
        public bool CheckWinConditions()
        {
            if (!_matchRunning) return true;

            var p0 = _state.Players[0];
            var p1 = _state.Players[1];

            bool p0Dead = !p0.IsAlive || (p0.Deck.Count == 0 && _state.Phase == MatchPhase.DrawPhase);
            bool p1Dead = !p1.IsAlive || (p1.Deck.Count == 0 && _state.Phase == MatchPhase.DrawPhase);

            if (!p0Dead && !p1Dead) return false;

            MatchResult result;
            if (p0Dead && p1Dead)
                result = MatchResult.Draw;
            else if (p1Dead)
                result = MatchResult.Victory;   // from local player (p0) perspective
            else
                result = MatchResult.Defeat;

            _state.SetPhase(MatchPhase.MatchOver);
            _state.SetResult(result);
            EndMatch();
            return true;
        }

        // ── Card Play ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to play a card from hand. Creatures go to the battlefield;
        /// spells/traps/equipment go directly to the graveyard.
        /// Effect processing happens for both types.
        /// </summary>
        private void TryPlayCard(CardInstance card, PlayerState player)
        {
            if (!player.Hand.Contains(card)) return;
            if (!player.TrySpendMana(card.BaseData.manaCost)) return;

            player.Hand.Remove(card);

            if (card.BaseData.cardClass == CardClass.Creature)
            {
                var slot = player.FirstEmptySlot();
                if (slot == null)
                {
                    // No room — refund mana and return
                    Debug.LogWarning($"[MatchManager] No empty slot for {card.BaseData.name}");
                    player.Hand.Add(card);
                        player.RestoreMana(card.BaseData.manaCost);
                    return;
                }

                slot.PlaceCard(card);
                card.SetStatus(CardInstanceStatus.OnBattlefield);
                GameEvents.RaiseCardPlacedOnBattlefield(card, player.PlayerIndex, slot.SlotIndex);
            }
            else
            {
                // Non-creature: resolve effect then send to graveyard
                card.SetStatus(CardInstanceStatus.InGraveyard);
                player.Graveyard.Add(card);
                GameEvents.RaiseCardSentToGraveyard(card, player.PlayerIndex);
            }

            // Fire effect processing for both creature and non-creature cards
            if (card.BaseData.effectData != null)
                Effects.CardEffectProcessor.ProcessEffects(card.BaseData.effectData, card, null, _state);

            GameEvents.RaiseCardPlayed(card.BaseData);
        }

        // ── End Match ──────────────────────────────────────────────────────────────

        private void EndMatch()
        {
            if (!_matchRunning) return;
            _matchRunning = false;

            // Fire quest integration event first (QuestTracker listens to OnMatchCompleted)
            // Use p0 as "local player" perspective for win/loss
            bool localWon = _state.Result == MatchResult.Victory;

            // Pick a representative card for quest tracking (first graveyard/battlefield card, or Neutral default)
            var p0 = _state.Players[0];
            var repCard = p0.Hand.Count > 0 ? p0.Hand[0]
                        : p0.Graveyard.Count > 0 ? p0.Graveyard[0] : null;

            var repElement = repCard?.BaseData.element ?? Items.CardElement.Neutral;
            var repClass   = repCard?.BaseData.cardClass ?? Items.CardClass.Creature;

            GameEvents.RaiseMatchCompleted(localWon, repElement, repClass);

            // Calculate rewards (grants gold internally and persists save)
            var rewards = MatchRewardCalculator.CalculateRewards(_state.Result, _state);

            GameEvents.RaiseMatchEnded(_state.Result, _state, rewards);
        }

        // ── Deck Building ──────────────────────────────────────────────────────────

        private void LoadDeckIntoState(DeckData deck, PlayerState player)
        {
            var list = new List<CardInstance>();

            foreach (var slot in deck.Slots)
                for (int i = 0; i < slot.Count; i++)
                    list.Add(new CardInstance(slot.Card));

            // Fisher-Yates shuffle
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            foreach (var card in list)
                player.Deck.Push(card);
        }
    }
}
