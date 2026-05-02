using UnityEngine;
using System.Collections.Generic;
using TCG.Core;

namespace TCG.Characters
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "TCG/Character")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string characterName;
        [TextArea] public string lore;
        public Sprite portrait;
        public Sprite fullArt;

        [Header("Stats")]
        public int maxHealth = 20;

        [Header("Energy")]
        public int startingEnergy = 0;
        public int maxEnergy = 10;
        public int energyPerTurn = 2;   // energy gained at the start of each owner's turn

        [Header("Passive Keywords")]
        public List<CharacterKeyword> keywords = new List<CharacterKeyword>();

        [Header("Abilities")]
        public List<CharacterAbilityData> abilities = new List<CharacterAbilityData>();

        public bool HasKeyword(CharacterKeyword kw) => keywords.Contains(kw);
    }
}
