using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TCG.Match;

namespace TCG.UI.Match
{
    /// <summary>
    /// One battlefield slot visual.
    ///
    /// Own row clicks → <see cref="LocalPlayerController.NotifyAttackSlotSelected"/>
    /// Opponent row clicks → <see cref="LocalPlayerController.NotifyDefendSlotSelected"/>
    ///
    /// Set <see cref="IsOpponentRow"/> before use so the click handler routes correctly.
    /// </summary>
    public class BattlefieldSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CardInPlayUI         cardInPlayUI;
        [SerializeField] private GameObject           emptySlotVisual;

        /// <summary>True when this slot belongs to the opponent's row.</summary>
        public bool IsOpponentRow { get; set; }

        private BattlefieldSlot       _slot;
        private LocalPlayerController _controller;

        // ── Setup ──────────────────────────────────────────────────────────────────

        public void Bind(BattlefieldSlot slot, LocalPlayerController controller, bool isOpponentRow)
        {
            _slot        = slot;
            _controller  = controller;
            IsOpponentRow = isOpponentRow;

            Refresh();
        }

        public void Refresh()
        {
            bool hasCard = _slot != null && _slot.HasCard;

            if (emptySlotVisual != null)
                emptySlotVisual.SetActive(!hasCard);

            if (cardInPlayUI != null)
            {
                cardInPlayUI.gameObject.SetActive(hasCard);
                if (hasCard)
                    cardInPlayUI.Bind(_slot.Occupant);
                else
                    cardInPlayUI.Unbind();
            }
        }

        // ── IPointerClickHandler ───────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_controller == null || _slot == null) return;

            if (IsOpponentRow)
                _controller.NotifyDefendSlotSelected(_slot);
            else
                _controller.NotifyAttackSlotSelected(_slot);
        }
    }
}
