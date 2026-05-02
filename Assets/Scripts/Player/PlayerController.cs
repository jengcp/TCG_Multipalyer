using UnityEngine;
using TCG.Cards;
using TCG.Core;

namespace TCG.Player
{
    /// <summary>
    /// MonoBehaviour that bridges local input → PlayerState actions.
    /// Attach to a Player GameObject in the scene.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public PlayerState State { get; private set; }

        private Card _selectedCard;
        private Card _selectedAttacker;
        private bool _isMyTurn;

        public void Initialize(PlayerState state)
        {
            State = state;
            _isMyTurn = false;

            GameEvents.OnTurnStarted += OnTurnStarted;
            GameEvents.OnTurnEnded += OnTurnEnded;
            GameEvents.OnPhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnTurnStarted -= OnTurnStarted;
            GameEvents.OnTurnEnded -= OnTurnEnded;
            GameEvents.OnPhaseChanged -= OnPhaseChanged;
        }

        private void OnTurnStarted(PlayerState p) => _isMyTurn = p == State;
        private void OnTurnEnded(PlayerState p) { if (p == State) _isMyTurn = false; }
        private void OnPhaseChanged(GamePhase phase) { /* react to phase if needed */ }

        // ── Input handlers called by UI ────────────────────────────────────

        public void SelectCard(Card card)
        {
            if (!_isMyTurn || card.Owner != State) return;
            _selectedCard = card;
        }

        public void TryPlaySelectedCard(Card targetCreature = null)
        {
            if (!_isMyTurn || _selectedCard == null) return;
            if (GameManager.Instance.CurrentPhase != GamePhase.MainPhase) return;

            State.PlayCard(_selectedCard, targetCreature);
            _selectedCard = null;
        }

        public void SelectAttacker(Card attacker)
        {
            if (!_isMyTurn) return;
            if (!attacker.CanAttack || attacker.Owner != State) return;
            _selectedAttacker = attacker;
        }

        public void DeclareAttackOnCreature(Card target)
        {
            if (!_isMyTurn || _selectedAttacker == null) return;
            if (GameManager.Instance.CurrentPhase != GamePhase.BattlePhase) return;

            GameManager.Instance.Battle.ResolveCombat(_selectedAttacker, target);
            _selectedAttacker = null;
        }

        public void DeclareAttackOnPlayer()
        {
            if (!_isMyTurn || _selectedAttacker == null) return;
            if (GameManager.Instance.CurrentPhase != GamePhase.BattlePhase) return;

            var opponent = GameManager.Instance.GetOpponent(State);
            var tauntCreatures = opponent.Field.GetTauntCreatures();
            if (tauntCreatures.Count > 0) return; // Must attack taunt first

            GameManager.Instance.Battle.ResolvePlayerAttack(_selectedAttacker, opponent);
            _selectedAttacker = null;
        }

        public void EndTurn()
        {
            if (!_isMyTurn) return;
            GameManager.Instance.Turns.EndCurrentTurn();
        }

        public void Surrender()
        {
            GameManager.Instance.DeclareResult(
                State == GameManager.Instance.Player1 ? GameResult.Player2Win : GameResult.Player1Win);
        }
    }
}
