using UnityEngine;

namespace TCG.Characters
{
    /// <summary>
    /// ScriptableObject describing a collectible character (cosmetic deck leader / avatar).
    /// Characters are unlocked for free (starter) or purchased with Gemstones.
    /// </summary>
    [CreateAssetMenu(fileName = "New Character", menuName = "TCG/Characters/Character")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string characterId;
        public string characterName;
        [TextArea(2, 4)]
        public string lore;

        [Header("Art")]
        public Sprite portrait;       // small thumbnail (shop grid, deck builder)
        public Sprite fullArt;        // large image shown in the detail view

        [Header("Unlock")]
        [Tooltip("Gemstone cost. Ignored for starter characters.")]
        public int gemCost;
        [Tooltip("If true, the player owns this character from the start (no purchase needed).")]
        public bool isStarterCharacter;
    }
}
