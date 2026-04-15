using System.Collections.Generic;

namespace TCG.Inventory.Deck
{
    public enum DeckValidationResult
    {
        Valid,
        TooFewCards,
        TooManyCards,
        TooManyCopies,
        NotEnoughOwned
    }

    public class DeckValidationReport
    {
        public DeckValidationResult Result    { get; }
        public string               Message   { get; }
        public bool                 IsValid   => Result == DeckValidationResult.Valid;
        public List<string>         Warnings  { get; } = new();

        public DeckValidationReport(DeckValidationResult result, string message)
        {
            Result  = result;
            Message = message;
        }

        public override string ToString() => $"[{Result}] {Message}";
    }

    /// <summary>
    /// Validates a <see cref="DeckData"/> against standard TCG rules.
    /// Rules are configurable via the constructor.
    /// </summary>
    public class DeckValidator
    {
        public int MinDeckSize { get; }
        public int MaxDeckSize { get; }

        public DeckValidator(int minDeckSize = 20, int maxDeckSize = 60)
        {
            MinDeckSize = minDeckSize;
            MaxDeckSize = maxDeckSize;
        }

        /// <summary>
        /// Validates <paramref name="deck"/> and optionally checks card ownership
        /// against <paramref name="inventory"/> (pass null to skip ownership check).
        /// </summary>
        public DeckValidationReport Validate(DeckData deck, IInventory inventory = null)
        {
            if (deck == null)
                return new DeckValidationReport(DeckValidationResult.TooFewCards, "Deck is null.");

            int total = deck.TotalCards;

            if (total < MinDeckSize)
                return new DeckValidationReport(DeckValidationResult.TooFewCards,
                    $"Deck has {total} cards; minimum is {MinDeckSize}.");

            if (total > MaxDeckSize)
                return new DeckValidationReport(DeckValidationResult.TooManyCards,
                    $"Deck has {total} cards; maximum is {MaxDeckSize}.");

            var report = new DeckValidationReport(DeckValidationResult.Valid, "Deck is valid.");

            foreach (var slot in deck.Slots)
            {
                // Copy limit check (defined per-card in CardData.maxCopiesInDeck)
                if (slot.Count > slot.Card.maxCopiesInDeck)
                {
                    return new DeckValidationReport(DeckValidationResult.TooManyCopies,
                        $"'{slot.Card.displayName}' exceeds copy limit " +
                        $"({slot.Count}/{slot.Card.maxCopiesInDeck}).");
                }

                // Ownership check
                if (inventory != null)
                {
                    int owned = inventory.GetQuantity(slot.Card.itemId);
                    if (owned < slot.Count)
                    {
                        return new DeckValidationReport(DeckValidationResult.NotEnoughOwned,
                            $"'{slot.Card.displayName}': need {slot.Count} copies, own {owned}.");
                    }
                }
            }

            return report;
        }
    }
}
