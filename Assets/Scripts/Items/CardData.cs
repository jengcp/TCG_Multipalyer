using UnityEngine;

namespace TCG.Items
{
    public enum CardElement { Neutral, Fire, Water, Earth, Air, Light, Dark }
    public enum CardClass  { Creature, Spell, Trap, Equipment }

    /// <summary>
    /// ScriptableObject for a single collectible card.
    /// Extends ItemData with TCG-specific attributes.
    /// </summary>
    [CreateAssetMenu(fileName = "New Card", menuName = "TCG/Items/Card")]
    public class CardData : ItemData
    {
        [Header("Card Stats")]
        public CardClass cardClass;
        public CardElement element;
        public int manaCost;
        public int attackPower;
        public int defensePower;
        public int healthPoints;

        [Header("Card Text")]
        [TextArea(2, 6)]
        public string effectText;
        [TextArea(1, 3)]
        public string flavorText;

        [Header("Visuals")]
        public Sprite cardArtwork;
        public Sprite cardFrame;

        [Header("Collection")]
        [Tooltip("Maximum copies of this card allowed in a single deck.")]
        public int maxCopiesInDeck = 3;
        [Tooltip("Set or expansion this card belongs to.")]
        public string cardSet;

        private void Awake()
        {
            itemType = ItemType.Card;
            isStackable = true;
            maxStack = 99;
        }
    }
}
