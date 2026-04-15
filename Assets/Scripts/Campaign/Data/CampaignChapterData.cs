using System.Collections.Generic;
using UnityEngine;

namespace TCG.Campaign
{
    /// <summary>
    /// One chapter of the campaign — a thematic group of hex-grid stages.
    /// </summary>
    [CreateAssetMenu(fileName = "New Chapter", menuName = "TCG/Campaign/Chapter")]
    public class CampaignChapterData : ScriptableObject
    {
        [Header("Identity")]
        public string chapterId;
        public string chapterName;
        [TextArea(1, 2)]
        public string chapterDescription;
        public Sprite chapterBackground;

        [Header("Stages")]
        [Tooltip("All stages belonging to this chapter.")]
        public List<CampaignStageData> stages = new();

        [Header("Unlock")]
        [Tooltip("Chapter that must be started before this one appears. Leave null for chapter 1.")]
        public CampaignChapterData prerequisiteChapter;
    }
}
