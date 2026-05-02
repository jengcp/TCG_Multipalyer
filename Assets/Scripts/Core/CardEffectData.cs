using UnityEngine;
using TCG.Core;

namespace TCG.Core
{
    [CreateAssetMenu(fileName = "NewCardEffect", menuName = "TCG/Card Effect")]
    public class CardEffectData : ScriptableObject
    {
        [Header("Effect Identity")]
        public string effectName;
        [TextArea] public string description;

        [Header("Effect Parameters")]
        public EffectType effectType;
        public TargetType targetType;
        public int value;

        [Header("Conditions")]
        public bool requiresTarget;
        public int manaCostModifier;

        public override string ToString()
        {
            return $"{effectName}: {effectType} ({value}) on {targetType}";
        }
    }
}
