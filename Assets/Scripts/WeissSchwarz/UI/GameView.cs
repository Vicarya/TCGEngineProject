using TCG.Core;
using TCG.Weiss; // For IHandZone, IStageZone, etc.
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Acts as the main controller for updating the entire game UI for a player.
    /// It retrieves game state from the core logic and passes it to the specialized UI zone managers.
    /// </summary>
    public class GameView : MonoBehaviour
    {
        [Header("Player UI Zone Managers")]
        [SerializeField] private HandZoneUI handZoneUI;
        [SerializeField] private StageZoneUI stageZoneUI;
        // TODO: Add references for other zones like Clock, Level, Stock, etc.
        // [SerializeField] private ClockZoneUI clockZoneUI;
        // [SerializeField] private LevelZoneUI levelZoneUI;
        // [SerializeField] private StockZoneUI stockZoneUI;

        /// <summary>
        /// Updates all managed UI zones to reflect the state of the provided player.
        /// </summary>
        /// <param name="player">The player whose state should be displayed.</param>
        public void UpdateView(Player player)
        {
            if (player == null)
            {
                Debug.LogError("Cannot update view for a null player.");
                return;
            }

            // Update Hand Zone
            if (handZoneUI != null)
            {
                var handZone = player.GetZone<IHandZone<WeissCard>>() as HandZone;
                handZoneUI.UpdateZone(handZone);
            }

            // Update Stage Zone
            if (stageZoneUI != null)
            {
                var stageZone = player.GetZone<IStageZone<WeissCard>>() as StageZone;
                stageZoneUI.UpdateZone(stageZone);
            }

            // TODO: Update other zones
            // if (clockZoneUI != null) { ... }
        }
    }
}
