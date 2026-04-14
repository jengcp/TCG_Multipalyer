using System;
using System.Collections;

namespace TCG.Match
{
    /// <summary>
    /// Abstraction over a player's decision-making. Implemented by
    /// <see cref="LocalPlayerController"/> (human via UI) and
    /// <see cref="AIPlayerController"/> (computer opponent).
    /// All selection methods are coroutines that yield until a choice is made,
    /// delivering the result via a callback so the match loop never blocks the main thread.
    /// </summary>
    public interface IPlayerController
    {
        /// <summary>Top-level hook called once per turn (currently a no-op; phases are driven by MatchManager).</summary>
        IEnumerator TakeTurn(PlayerState myState, MatchState matchState);

        /// <summary>
        /// Choose a card from <paramref name="state"/>.Hand to play.
        /// Invoke <paramref name="onSelected"/> with the chosen card, or null to pass/end main phase.
        /// </summary>
        IEnumerator SelectCardToPlay(PlayerState state, Action<CardInstance> onSelected);

        /// <summary>
        /// Choose an untapped creature on <paramref name="state"/>.Battlefield to attack with.
        /// Invoke <paramref name="onSelected"/> with the slot, or null to skip all attacks.
        /// </summary>
        IEnumerator SelectAttackSlot(PlayerState state, Action<BattlefieldSlot> onSelected);

        /// <summary>
        /// Choose a target on <paramref name="opponentState"/>.Battlefield to receive the attack.
        /// Invoke <paramref name="onSelected"/> with null to perform a direct player attack.
        /// </summary>
        IEnumerator SelectDefendSlot(PlayerState opponentState, Action<BattlefieldSlot> onSelected);
    }
}
