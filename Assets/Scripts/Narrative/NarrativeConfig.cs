using UnityEngine;

namespace TCG.Narrative
{
    /// <summary>
    /// Designer-owned asset that holds every <see cref="NarrativeEventData"/> in the game,
    /// in the order they should be checked and displayed.
    ///
    /// Create via: Assets → Create → TCG → Narrative → Narrative Config.
    /// Assign the single instance to <see cref="NarrativeManager"/> in the Inspector.
    ///
    /// Events are evaluated top-to-bottom; the first unseen matching event fires for each trigger.
    /// </summary>
    [CreateAssetMenu(menuName = "TCG/Narrative/Narrative Config", fileName = "NarrativeConfig")]
    public class NarrativeConfig : ScriptableObject
    {
        [Tooltip("All narrative events in the order they should fire. " +
                 "Events earlier in the list take priority when multiple triggers match simultaneously.")]
        public NarrativeEventData[] events = System.Array.Empty<NarrativeEventData>();
    }
}
