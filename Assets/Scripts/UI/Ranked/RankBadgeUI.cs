using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Ranked;

namespace TCG.UI.Ranked
{
    /// <summary>
    /// Reusable rank display: tier icon, division pips, and an optional RP bar + label.
    /// Bind via <see cref="Bind(RankTier, RankDivision, int)"/>.
    ///
    /// Inspector setup:
    ///   • <see cref="tierIcon"/>      — Image that shows the tier emblem sprite
    ///   • <see cref="divisionPips"/>  — 3 small Images (DivIII=1 pip, DivII=2 pips, DivI=3 pips); hidden for Master
    ///   • <see cref="rpBar"/>         — Optional Slider (0–99 range); hidden for Master's open-ended RP
    ///   • <see cref="rpText"/>        — Optional TMP_Text showing numeric RP
    ///   • <see cref="tierSprites"/>   — 6-element array indexed by (int)RankTier
    /// </summary>
    public class RankBadgeUI : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Image     tierIcon;
        [SerializeField] private Image[]   divisionPips = new Image[3];
        [SerializeField] private Slider    rpBar;
        [SerializeField] private TMP_Text  rpText;

        [Header("Tier Sprites")]
        [Tooltip("One sprite per RankTier, in order: Bronze, Silver, Gold, Platinum, Diamond, Master.")]
        [SerializeField] private Sprite[] tierSprites = new Sprite[6];

        [Header("Pip Colors")]
        [SerializeField] private Color pipActiveColor   = Color.white;
        [SerializeField] private Color pipInactiveColor = new Color(1f, 1f, 1f, 0.25f);

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Updates all visuals to reflect the supplied rank state.</summary>
        public void Bind(RankTier tier, RankDivision division, int rp)
        {
            SetTierIcon(tier);
            SetDivisionPips(tier, division);
            SetRpBar(tier, rp);
        }

        // ── Internal ──────────────────────────────────────────────────────────────

        private void SetTierIcon(RankTier tier)
        {
            if (tierIcon == null) return;
            int index = (int)tier;
            tierIcon.sprite  = (tierSprites != null && index < tierSprites.Length) ? tierSprites[index] : null;
            tierIcon.enabled = tierIcon.sprite != null;
        }

        private void SetDivisionPips(RankTier tier, RankDivision division)
        {
            if (divisionPips == null) return;

            bool isMaster    = tier == RankTier.Master;
            // Number of lit pips: DivIII=1, DivII=2, DivI=3, None=0
            int litPips = isMaster ? 0 : (int)division + 1;

            for (int i = 0; i < divisionPips.Length; i++)
            {
                if (divisionPips[i] == null) continue;
                divisionPips[i].gameObject.SetActive(!isMaster);
                divisionPips[i].color = i < litPips ? pipActiveColor : pipInactiveColor;
            }
        }

        private void SetRpBar(RankTier tier, int rp)
        {
            bool isMaster = tier == RankTier.Master;

            if (rpBar != null)
            {
                rpBar.gameObject.SetActive(!isMaster);
                if (!isMaster)
                {
                    rpBar.minValue = 0;
                    rpBar.maxValue = 99;
                    rpBar.value    = Mathf.Clamp(rp, 0, 99);
                }
            }

            if (rpText != null)
                rpText.text = isMaster ? $"{rp} LP" : $"{rp} / 99 RP";
        }
    }
}
