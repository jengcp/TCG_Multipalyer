namespace TCG.Match
{
    /// <summary>
    /// One position on a player's side of the battlefield.
    /// Holds at most one CardInstance. Mutation is internal to the Match namespace.
    /// </summary>
    public class BattlefieldSlot
    {
        public int          SlotIndex { get; }
        public CardInstance Occupant  { get; private set; }

        public bool HasCard => Occupant != null;
        public bool IsEmpty => Occupant == null;

        public BattlefieldSlot(int index)
        {
            SlotIndex = index;
        }

        /// <summary>Places a card in this slot and updates the card's status.</summary>
        public void PlaceCard(CardInstance card)
        {
            Occupant = card;
            card?.SetStatus(CardInstanceStatus.OnBattlefield);
        }

        /// <summary>Removes and returns the occupant without updating card status (caller must do that).</summary>
        public CardInstance RemoveCard()
        {
            var card = Occupant;
            Occupant = null;
            return card;
        }

        public override string ToString() =>
            IsEmpty ? $"[Slot {SlotIndex}] Empty" : $"[Slot {SlotIndex}] {Occupant}";
    }
}
