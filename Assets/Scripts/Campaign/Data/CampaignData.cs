using System.Collections.Generic;
using UnityEngine;

namespace TCG.Campaign
{
    /// <summary>
    /// Root ScriptableObject that holds the ordered list of all campaign chapters.
    /// Assign one of these to CampaignManager in the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "Campaign", menuName = "TCG/Campaign/Campaign")]
    public class CampaignData : ScriptableObject
    {
        public string campaignName;
        public List<CampaignChapterData> chapters = new();
    }
}
