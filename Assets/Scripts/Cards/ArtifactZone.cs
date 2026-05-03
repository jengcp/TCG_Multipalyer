using System.Collections.Generic;
using TCG.Core;
using TCG.Player;

namespace TCG.Cards
{
    /// <summary>
    /// Holds artifacts that are permanently on the field.
    /// Artifacts apply passive effects each turn via OnTurnStart().
    /// </summary>
    public class ArtifactZone
    {
        public const int MaxArtifacts = 5;

        private List<Card> _artifacts = new List<Card>();
        public IReadOnlyList<Card> Artifacts => _artifacts;
        public int Count => _artifacts.Count;
        public bool IsFull => _artifacts.Count >= MaxArtifacts;

        public bool AddArtifact(Card card)
        {
            if (IsFull || !card.Data.IsArtifact) return false;
            card.SetZone(GameZone.ArtifactZone);
            _artifacts.Add(card);
            return true;
        }

        public bool RemoveArtifact(Card card) => _artifacts.Remove(card);

        public bool Contains(Card card) => _artifacts.Contains(card);

        /// <summary>
        /// Called each turn start — resolves any persistent/on-turn effects.
        /// </summary>
        public void OnTurnStart(PlayerState owner)
        {
            foreach (var artifact in _artifacts)
            {
                if (!artifact.IsAlive) continue;
                foreach (var effect in artifact.Data.onPlayEffects)
                    EffectResolver.Resolve(effect, owner, artifact);
            }
        }

        public void Clear() => _artifacts.Clear();
    }
}
