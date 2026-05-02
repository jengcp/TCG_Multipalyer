using System.Collections.Generic;
using TCG.Core;

namespace TCG.Cards
{
    public class Field
    {
        public const int MaxFieldSize = 7;

        private List<Card> _creatures = new List<Card>();
        public IReadOnlyList<Card> Creatures => _creatures;
        public int Count => _creatures.Count;
        public bool IsFull => _creatures.Count >= MaxFieldSize;

        public bool AddCard(Card card)
        {
            if (IsFull || !card.Data.IsCreature) return false;
            card.SetZone(GameZone.Field);
            _creatures.Add(card);
            return true;
        }

        public bool RemoveCard(Card card)
        {
            return _creatures.Remove(card);
        }

        public bool ContainsCard(Card card) => _creatures.Contains(card);

        public List<Card> GetAttackers()
        {
            var attackers = new List<Card>();
            foreach (var c in _creatures)
                if (c.CanAttack) attackers.Add(c);
            return attackers;
        }

        public List<Card> GetTauntCreatures()
        {
            var taunt = new List<Card>();
            foreach (var c in _creatures)
                if (c.Data.hasTaunt && c.IsAlive) taunt.Add(c);
            return taunt;
        }

        public void RefreshAll()
        {
            foreach (var c in _creatures)
                c.Refresh();
        }

        public void Clear() => _creatures.Clear();
    }
}
