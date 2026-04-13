using System.Collections.Generic;
using System.Linq;
using TCG.Items;

namespace TCG.Inventory
{
    /// <summary>
    /// A read-only view over the player's owned cards, with card-specific
    /// filtering (element, class, mana cost range) on top of the base filter.
    /// Does not own data — it queries PlayerInventory on demand.
    /// </summary>
    public class CardCollection
    {
        private readonly IInventory _source;

        public CardCollection(IInventory source)
        {
            _source = source;
        }

        // ─── Queries ───────────────────────────────────────────────────────────

        /// <summary>All owned cards as InventoryItems.</summary>
        public List<InventoryItem> GetAll()
            => _source.GetByType(ItemType.Card);

        /// <summary>All owned cards filtered by element.</summary>
        public List<InventoryItem> GetByElement(CardElement element)
            => GetAll()
               .Where(i => i.itemData is CardData c && c.element == element)
               .ToList();

        /// <summary>All owned cards filtered by card class (Creature, Spell, etc.).</summary>
        public List<InventoryItem> GetByClass(CardClass cardClass)
            => GetAll()
               .Where(i => i.itemData is CardData c && c.cardClass == cardClass)
               .ToList();

        /// <summary>Cards within an inclusive mana cost range.</summary>
        public List<InventoryItem> GetByManaCost(int min, int max)
            => GetAll()
               .Where(i => i.itemData is CardData c && c.manaCost >= min && c.manaCost <= max)
               .ToList();

        /// <summary>Cards filtered by rarity.</summary>
        public List<InventoryItem> GetByRarity(ItemRarity rarity)
            => GetAll()
               .Where(i => i.itemData.rarity == rarity)
               .ToList();

        /// <summary>Cards belonging to a specific set/expansion.</summary>
        public List<InventoryItem> GetBySet(string setName)
            => GetAll()
               .Where(i => i.itemData is CardData c &&
                           c.cardSet.Equals(setName, System.StringComparison.OrdinalIgnoreCase))
               .ToList();

        /// <summary>
        /// Returns cards that match all of the supplied card-level criteria.
        /// Pass null/default for criteria you don't care about.
        /// </summary>
        public List<InventoryItem> GetAdvanced(
            CardElement?  element   = null,
            CardClass?    cardClass = null,
            ItemRarity?   rarity    = null,
            int           minMana   = 0,
            int           maxMana   = int.MaxValue,
            string        nameContains = "")
        {
            return GetAll().Where(i =>
            {
                if (i.itemData is not CardData card) return false;
                if (element.HasValue   && card.element   != element.Value)   return false;
                if (cardClass.HasValue && card.cardClass != cardClass.Value)  return false;
                if (rarity.HasValue    && card.rarity    != rarity.Value)     return false;
                if (card.manaCost < minMana || card.manaCost > maxMana)       return false;
                if (!string.IsNullOrEmpty(nameContains) &&
                    !card.displayName.ToLowerInvariant().Contains(nameContains.ToLowerInvariant()))
                    return false;
                return true;
            }).ToList();
        }

        /// <summary>Total number of unique card types owned.</summary>
        public int UniqueCardCount => GetAll().Count;

        /// <summary>Total card copies owned across all types.</summary>
        public int TotalCardCount  => GetAll().Sum(i => i.quantity);

        /// <summary>How many copies of a specific card are owned.</summary>
        public int OwnedCopies(string cardId) => _source.GetQuantity(cardId);
    }
}
