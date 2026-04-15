using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Items;

namespace TCG.Inventory.Deck
{
    /// <summary>
    /// Runtime representation of a constructed deck.
    /// Cards are stored as (CardData, count) pairs.
    /// Serialized to/from <see cref="TCG.Save.DeckSaveData"/>.
    /// </summary>
    public class DeckData
    {
        public string DeckId          { get; }
        public string DeckName        { get; private set; }
        public DateTime CreatedAt     { get; }
        public DateTime LastModified  { get; private set; }

        private readonly List<DeckCardSlot> _slots = new();

        public IReadOnlyList<DeckCardSlot> Slots => _slots.AsReadOnly();
        public int TotalCards => _slots.Sum(s => s.Count);
        public int UniqueCards => _slots.Count;

        public DeckData(string name = "New Deck", string id = null)
        {
            DeckId       = id ?? Guid.NewGuid().ToString("N");
            DeckName     = name;
            CreatedAt    = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
        }

        // ─── Mutation ──────────────────────────────────────────────────────────

        public void Rename(string newName)
        {
            DeckName     = newName;
            LastModified = DateTime.UtcNow;
        }

        /// <summary>Adds one copy of <paramref name="card"/> to the deck.</summary>
        public bool TryAddCard(CardData card)
        {
            if (card == null) return false;

            var existing = _slots.FirstOrDefault(s => s.Card.itemId == card.itemId);
            if (existing != null)
            {
                if (existing.Count >= card.maxCopiesInDeck) return false;
                existing.Increment();
            }
            else
            {
                _slots.Add(new DeckCardSlot(card, 1));
            }

            LastModified = DateTime.UtcNow;
            return true;
        }

        /// <summary>Removes one copy of the card with <paramref name="cardId"/>.</summary>
        public bool TryRemoveCard(string cardId)
        {
            var slot = _slots.FirstOrDefault(s => s.Card.itemId == cardId);
            if (slot == null) return false;

            slot.Decrement();
            if (slot.Count <= 0) _slots.Remove(slot);

            LastModified = DateTime.UtcNow;
            return true;
        }

        public void ClearDeck()
        {
            _slots.Clear();
            LastModified = DateTime.UtcNow;
        }

        // ─── Query ─────────────────────────────────────────────────────────────

        public int GetCount(string cardId)
        {
            var slot = _slots.FirstOrDefault(s => s.Card.itemId == cardId);
            return slot?.Count ?? 0;
        }

        public bool Contains(string cardId) => _slots.Any(s => s.Card.itemId == cardId);
    }

    /// <summary>One card type + how many copies are in the deck.</summary>
    public class DeckCardSlot
    {
        public CardData Card  { get; }
        public int      Count { get; private set; }

        public DeckCardSlot(CardData card, int count)
        {
            Card  = card;
            Count = count;
        }

        public void Increment() => Count++;
        public void Decrement() => Count = Math.Max(0, Count - 1);
    }
}
