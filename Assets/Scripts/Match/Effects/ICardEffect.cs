namespace TCG.Match.Effects
{
    /// <summary>
    /// Implemented by any class that represents a discrete card effect.
    /// CardEffectProcessor is the primary consumer; this interface allows
    /// custom effect logic beyond the built-in CardEffectType enum.
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// Execute this effect.
        /// <paramref name="source"/> — the card that was played.
        /// <paramref name="target"/> — may be null for non-targeted or player-level effects.
        /// </summary>
        void Execute(CardInstance source, CardInstance target, MatchState state);
    }
}
