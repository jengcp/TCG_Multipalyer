using UnityEngine;

namespace TCG.Cutscene
{
    /// <summary>
    /// Identifies a speaking character in a dialogue beat.
    /// Create via: Assets → Create → TCG → Cutscene → Character Profile.
    /// </summary>
    [CreateAssetMenu(menuName = "TCG/Cutscene/Character Profile", fileName = "CharProfile_New")]
    public class CharacterProfile : ScriptableObject
    {
        [Tooltip("Name shown in the dialogue name-plate.")]
        public string characterName = "???";

        [Tooltip("Portrait sprite shown beside the dialogue box. Leave null for a text-only narrator entry.")]
        public Sprite portrait;

        [Tooltip("Color of the name-plate text.")]
        public Color  nameColor = Color.white;
    }
}
