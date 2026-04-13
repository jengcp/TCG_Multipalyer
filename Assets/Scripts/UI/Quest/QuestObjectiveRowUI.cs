using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Quest;

namespace TCG.UI.Quest
{
    /// <summary>
    /// Displays a single objective's description and progress bar inside a quest entry.
    /// </summary>
    public class QuestObjectiveRowUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider          progressBar;
        [SerializeField] private Image           doneCheckmark;

        public void Populate(ObjectiveProgress obj)
        {
            if (descriptionText != null)
                descriptionText.text = obj.Data.description;

            if (progressText != null)
                progressText.text = $"{obj.Current}/{obj.Target}";

            if (progressBar != null)
                progressBar.value = obj.Ratio;

            if (doneCheckmark != null)
                doneCheckmark.gameObject.SetActive(obj.IsDone);
        }
    }
}
