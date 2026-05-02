using UnityEngine;
using TCG.Core;
using TCG.Player;

namespace TCG.Game
{
    public class TurnManager
    {
        public GamePhase CurrentPhase { get; private set; } = GamePhase.NotStarted;
        public PlayerState ActivePlayer { get; private set; }

        private PlayerState _player1;
        private PlayerState _player2;

        public void Initialize(PlayerState p1, PlayerState p2)
        {
            _player1 = p1;
            _player2 = p2;
        }

        public void StartFirstTurn()
        {
            ActivePlayer = _player1;
            BeginTurn();
        }

        private void BeginTurn()
        {
            ActivePlayer.OnTurnStart(); // handles mana ramp + character energy gain + cooldown ticks
            ActivePlayer.RefreshField();

            GameEvents.TurnStarted(ActivePlayer);
            EnterPhase(GamePhase.DrawPhase);
            ProcessDrawPhase();
        }

        private void ProcessDrawPhase()
        {
            ActivePlayer.DrawCard();
            EnterPhase(GamePhase.MainPhase);
            // Player controls when to move to BattlePhase via EndMainPhase()
        }

        public void EndMainPhase()
        {
            if (CurrentPhase != GamePhase.MainPhase) return;
            EnterPhase(GamePhase.BattlePhase);
            // Player attacks; resolved per attack action
        }

        public void EndBattlePhase()
        {
            if (CurrentPhase != GamePhase.BattlePhase) return;
            EnterPhase(GamePhase.EndPhase);
            EndCurrentTurn();
        }

        public void EndCurrentTurn()
        {
            if (CurrentPhase == GamePhase.GameOver) return;

            GameEvents.TurnEnded(ActivePlayer);

            // Swap active player
            ActivePlayer = ActivePlayer == _player1 ? _player2 : _player1;
            BeginTurn();
        }

        private void EnterPhase(GamePhase phase)
        {
            CurrentPhase = phase;
            GameEvents.PhaseChanged(phase);
        }

        public void SetGameOver()
        {
            CurrentPhase = GamePhase.GameOver;
            GameEvents.PhaseChanged(GamePhase.GameOver);
        }
    }
}
