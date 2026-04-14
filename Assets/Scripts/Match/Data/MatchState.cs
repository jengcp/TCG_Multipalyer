namespace TCG.Match
{
    /// <summary>
    /// Full snapshot of an ongoing match — both players, current turn, phase, and result.
    /// Plain C# class. Mutators are internal so only Match-namespace managers can change state.
    /// </summary>
    public class MatchState
    {
        public PlayerState[] Players           { get; }  // always length 2
        public int           CurrentPlayerIndex { get; private set; }
        public int           TurnNumber         { get; private set; }
        public MatchPhase    Phase              { get; private set; }
        public MatchResult   Result             { get; private set; }

        public PlayerState ActivePlayer   => Players[CurrentPlayerIndex];
        public PlayerState OpponentPlayer => Players[1 - CurrentPlayerIndex];

        public MatchState(PlayerState p0, PlayerState p1)
        {
            Players            = new[] { p0, p1 };
            CurrentPlayerIndex = 0;
            TurnNumber         = 0;
            Phase              = MatchPhase.Setup;
        }

        internal void SetPhase(MatchPhase phase)        => Phase              = phase;
        internal void SetCurrentPlayer(int index)       => CurrentPlayerIndex = index;
        internal void IncrementTurn()                   => TurnNumber++;
        internal void SetResult(MatchResult result)     => Result             = result;
    }
}
