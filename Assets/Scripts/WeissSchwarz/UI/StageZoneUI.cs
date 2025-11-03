using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the stage, including all 5 character slots.
    /// </summary>
    public class StageZoneUI : MonoBehaviour
    {
        [Header("Stage Slot UI References")]
        [SerializeField] private CardUI frontLeftSlotUI;
        [SerializeField] private CardUI frontCenterSlotUI;
        [SerializeField] private CardUI frontRightSlotUI;
        [SerializeField] private CardUI backLeftSlotUI;
        [SerializeField] private CardUI backRightSlotUI;

        /// <summary>
        /// Updates the stage display to match the provided StageZone state.
        /// </summary>
        /// <param name="stageZone">The stage zone from the game logic.</param>
        public void UpdateZone(StageZone stageZone)
        {
            if (stageZone == null)
            {
                // Hide all slots if the zone is null
                frontLeftSlotUI.SetCard(null);
                frontCenterSlotUI.SetCard(null);
                frontRightSlotUI.SetCard(null);
                backLeftSlotUI.SetCard(null);
                backRightSlotUI.SetCard(null);
                return;
            }

            // Update each slot with the card from the corresponding logical slot
            frontLeftSlotUI.SetCard(stageZone.FrontLeft.Current);
            frontCenterSlotUI.SetCard(stageZone.FrontCenter.Current);
            frontRightSlotUI.SetCard(stageZone.FrontRight.Current);
            backLeftSlotUI.SetCard(stageZone.BackLeft.Current);
            backRightSlotUI.SetCard(stageZone.BackRight.Current);
        }
    }
}
