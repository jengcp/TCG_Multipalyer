using TCG.Core;
using TCG.Player;

namespace TCG.Game
{
    public class TurnManager
    {
        public GamePhase CurrentPhase { get; private set; } = GamePhase.NotStarted;
        public PlayerState ActivePlayer { get; private set; }
        public PlayerState DefendingPlayer => ActivePlayer == _player1 ? _player2 : _player1;

        private PlayerState _player1;
        private PlayerState _player2;
        private CombatManager _combat;

        public CombatManager Combat => _combat;

        public void Initialize(PlayerState p1, PlayerState p2, CombatManager combat)
        {
            _player1 = p1;
            _player2 = p2;
            _combat = combat;
        }

        public void StartFirstTurn()
        {
            ActivePlayer = _player1;
            BeginTurn();
        }

        // ── Turn flow ──────────────────────────────────────────────────────

        private void BeginTurn()
        {
            ActivePlayer.OnTurnStart();
            ActivePlayer.RefreshField();

            GameEvents.TurnStarted(ActivePlayer);
            EnterPhase(GamePhase.DrawPhase);

            // Draw phase auto-resolves
            ActivePlayer.DrawCard();
            EnterPhase(GamePhase.MainPhase);
            // Player stays in MainPhase until they call EndMainPhase()
        }

        /// <summary>Player finishes placing cards — move to Battle Phase.</summary>
        public void EndMainPhase()
        {
            if (CurrentPhase != GamePhase.MainPhase) return;
            EnterPhase(GamePhase.BattlePhase);
            _combat.BeginDeclareAttackers(ActivePlayer, DefendingPlayer);
        }

        // ── Battle Phase hooks (called by combat system) ───────────────────

        /// <summary>Active player toggles a creature as attacker.</summary>
        public void ToggleAttacker(TCG.Cards.Card card)
        {
            if (CurrentPhase != GamePhase.BattlePhase) return;
            _combat.ToggleAttacker(card);
        }

        /// <summary>Active player locks in their attacker choices.</summary>
        public void ConfirmAttackers()
        {
            if (CurrentPhase != GamePhase.BattlePhase) return;
            _combat.ConfirmAttackers();

            // If no attackers were declared, CombatManager already set Idle — go to EndPhase
            if (_combat.SubPhase == BattleSubPhase.Idle)
                BeginEndPhase();
        }

        /// <summary>Defending player assigns a blocker to an attacker.</summary>
        public bool AssignBlocker(TCG.Cards.Card blocker, TCG.Cards.Card attacker)
        {
            if (CurrentPhase != GamePhase.BattlePhase) return false;
            return _combat.AssignBlocker(blocker, attacker);
        }

        /// <summary>Defending player confirms blockers → combat resolves automatically.</summary>
        public void ConfirmBlockers()
        {
            if (CurrentPhase != GamePhase.BattlePhase) return;
            _combat.ConfirmBlockers();
            // After resolution CombatManager goes Idle — proceed to End Phase
            BeginEndPhase();
        }

        /// <summary>Skip directly to End Phase without attacking (pass priority).</summary>
        public void SkipBattlePhase()
        {
            if (CurrentPhase != GamePhase.BattlePhase) return;
            BeginEndPhase();
        }

        // ── End Phase ─────────────────────────────────────────────────────

        private void BeginEndPhase()
        {
            EnterPhase(GamePhase.EndPhase);

            // End-of-turn effects: poison ticks, etc.
            ActivePlayer.OnTurnEnd();

            EndCurrentTurn();
        }

        public void EndCurrentTurn()
        {
            if (CurrentPhase == GamePhase.GameOver) return;

            GameEvents.TurnEnded(ActivePlayer);
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
