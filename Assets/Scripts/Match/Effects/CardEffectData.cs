using System;
using UnityEngine;

namespace TCG.Match.Effects
{
    [Serializable]
    public class CardEffectEntry
    {
        public CardEffectType   effectType;
        public EffectTargetType targetType;
        [Tooltip("Damage dealt, HP healed, stat buff amount, number of cards drawn, etc.")]
        public int              magnitude = 1;
    }

    /// <summary>
    /// ScriptableObject that holds the list of effects a card applies when played.
    /// Assign to <see cref="TCG.Items.CardData.effectData"/> in the Inspector.
    /// Create via: Assets → Create → TCG/Match/Card Effect Data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEffect", menuName = "TCG/Match/Card Effect Data")]
    public class CardEffectData : ScriptableObject
    {
        [Tooltip("Effects are resolved in list order.")]
        public CardEffectEntry[] effects = Array.Empty<CardEffectEntry>();
    }
}
