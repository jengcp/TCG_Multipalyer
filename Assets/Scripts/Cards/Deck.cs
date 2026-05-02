using UnityEngine;
using System.Collections.Generic;
using TCG.Core;

namespace TCG.Cards
{
    public class Deck
    {
        private List<Card> _cards = new List<Card>();

        public int Count => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;

        public void AddCard(Card card)
        {
            card.SetZone(GameZone.Deck);
            _cards.Add(card);
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
                AddCard(card);
        }

        public Card DrawTop()
        {
            if (IsEmpty) return null;
            var card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }

        public Card PeekTop() => IsEmpty ? null : _cards[0];

        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        public void RemoveCard(Card card)
        {
            _cards.Remove(card);
        }

        public List<Card> GetAll() => new List<Card>(_cards);

        public void Clear() => _cards.Clear();
    }
}
