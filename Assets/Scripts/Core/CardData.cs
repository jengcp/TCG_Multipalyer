using UnityEngine;
using System.Collections.Generic;
using TCG.Core;

namespace TCG.Core
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "TCG/Card")]
    public class CardData : ScriptableObject
    {
        [Header("Identity")]
        public string cardName;
        [TextArea] public string flavorText;
        public Sprite artwork;

        [Header("Classification")]
        public CardType cardType;
        public CardRarity rarity;
        public CardElement element;

        [Header("Cost")]
        public int manaCost;

        [Header("Stats (Creatures only)")]
        public int baseAttack;
        public int baseDefense;
        public int baseHealth;

        [Header("Effects")]
        public List<CardEffectData> onPlayEffects = new List<CardEffectData>();
        public List<CardEffectData> onDeathEffects = new List<CardEffectData>();
        public List<CardEffectData> activatedAbilities = new List<CardEffectData>();

        [Header("Keywords")]
        public bool hasFirstStrike;
        public bool hasLifelink;
        public bool hasVigilance;
        public bool hasFlying;
        public bool hasTaunt;

        public bool IsCreature => cardType == CardType.Creature;
        public bool IsSpell => cardType == CardType.Spell;
        public bool IsArtifact => cardType == CardType.Artifact;
        public bool IsTrap => cardType == CardType.Trap;
    }
}
