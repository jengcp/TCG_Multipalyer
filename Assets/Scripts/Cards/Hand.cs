using System.Collections.Generic;
using TCG.Core;

namespace TCG.Cards
{
    public class Hand
    {
        public const int MaxHandSize = 10;

        private List<Card> _cards = new List<Card>();
        public IReadOnlyList<Card> Cards => _cards;
        public int Count => _cards.Count;
        public bool IsFull => _cards.Count >= MaxHandSize;

        public bool AddCard(Card card)
        {
            if (IsFull) return false;
            card.SetZone(GameZone.Hand);
            _cards.Add(card);
            return true;
        }

        public bool RemoveCard(Card card)
        {
            return _cards.Remove(card);
        }

        public bool ContainsCard(Card card) => _cards.Contains(card);

        public void Clear() => _cards.Clear();
    }
}
