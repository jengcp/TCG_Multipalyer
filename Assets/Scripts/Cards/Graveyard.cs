using System.Collections.Generic;
using TCG.Core;

namespace TCG.Cards
{
    public class Graveyard
    {
        private List<Card> _cards = new List<Card>();
        public IReadOnlyList<Card> Cards => _cards;
        public int Count => _cards.Count;

        public void AddCard(Card card)
        {
            card.SetZone(GameZone.Graveyard);
            _cards.Add(card);
        }

        public bool RemoveCard(Card card) => _cards.Remove(card);

        public Card PeekTop() => _cards.Count > 0 ? _cards[^1] : null;

        public void Clear() => _cards.Clear();
    }
}
