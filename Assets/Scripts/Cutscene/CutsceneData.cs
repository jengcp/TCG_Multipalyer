using System;
using UnityEngine;

namespace TCG.Cutscene
{
    /// <summary>
    /// An ordered sequence of <see cref="CutsceneBeat"/>s that form one cutscene or story segment.
    /// Create via: Assets → Create → TCG → Cutscene → Cutscene.
    ///
    /// Assign to <see cref="TCG.Campaign.CampaignStageData.preStageCutscene"/> or
    /// <see cref="TCG.Campaign.CampaignStageData.postStageCutscene"/>, or play manually via
    /// <see cref="CutsceneManager.Play"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "TCG/Cutscene/Cutscene", fileName = "Cutscene_New")]
    public class CutsceneData : ScriptableObject
    {
        [Tooltip("Unique identifier — used for save-tracking (e.g. 'ch1_stage1_intro').")]
        public string cutsceneId;

        [Tooltip("Human-readable name shown in the editor.")]
        public string displayName;

        [Tooltip("If true the player can tap the Skip button to jump straight to the end.")]
        public bool skippable = true;

        [Tooltip("Ordered list of beats that make up this cutscene.")]
        public CutsceneBeat[] beats = Array.Empty<CutsceneBeat>();
    }
}
