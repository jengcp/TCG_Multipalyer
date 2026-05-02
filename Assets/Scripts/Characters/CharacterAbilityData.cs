using UnityEngine;
using TCG.Core;

namespace TCG.Characters
{
    [CreateAssetMenu(fileName = "NewAbility", menuName = "TCG/Character Ability")]
    public class CharacterAbilityData : ScriptableObject
    {
        [Header("Identity")]
        public string abilityName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Cost & Cooldown")]
        public int energyCost;
        public int cooldownTurns;       // 0 = no cooldown, N = N turns before reuse

        [Header("Effect")]
        public CharacterEffectType effectType;
        public TargetType targetType;
        public int value;               // damage / heal / buff / card count amount

        [Header("Token Summoning (SummonToken only)")]
        public TCG.Core.CardData tokenData; // filled when effectType == SummonToken

        [Header("Keyword Grant (GiveCreatureKeyword only)")]
        public TCG.Core.StatusEffect keywordToGrant;

        public override string ToString()
        {
            return $"{abilityName} [{energyCost}E, {cooldownTurns}cd]: {effectType}({value}) → {targetType}";
        }
    }
}
