using UnityEngine;
using TCG.Cutscene;

namespace TCG.Narrative
{
    /// <summary>
    /// Describes one story beat in the overarching game narrative.
    ///
    /// Create via: Assets → Create → TCG → Narrative → Narrative Event.
    ///
    /// Each event has:
    /// <list type="bullet">
    ///   <item>A <see cref="trigger"/> that defines when it fires automatically.</item>
    ///   <item>An optional <see cref="cutscene"/> that plays when it fires.</item>
    ///   <item>A <see cref="logTitle"/> / <see cref="logBody"/> / <see cref="logIllustration"/>
    ///       used in the Story Log UI so players can revisit lore.</item>
    /// </list>
    ///
    /// Events are fired in the order they appear in <see cref="NarrativeConfig.events"/>.
    /// Only the first unseen matching event fires per trigger check.
    /// </summary>
    [CreateAssetMenu(menuName = "TCG/Narrative/Narrative Event", fileName = "NarrativeEvent_New")]
    public class NarrativeEventData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier used for save-tracking (e.g. 'prologue', 'ch1_aftermath').")]
        public string eventId;

        [Header("Story Log")]
        [Tooltip("Heading displayed in the Story Log panel.")]
        public string logTitle;

        [TextArea(3, 8)]
        [Tooltip("Body text displayed in the Story Log panel. Can contain lore, letters, journal entries, etc.")]
        public string logBody;

        [Tooltip("Illustration shown alongside the log entry. Optional.")]
        public Sprite logIllustration;

        [Tooltip("When true this event appears in the Story Log after it fires.")]
        public bool addToLog = true;

        [Header("Trigger")]
        [Tooltip("Condition that causes this event to fire automatically.")]
        public NarrativeTrigger trigger;

        [Header("Cutscene")]
        [Tooltip("Cutscene played when this event fires. Leave null for log-only events.")]
        public CutsceneData cutscene;
    }
}
