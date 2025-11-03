using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Climax Zone.
    /// </summary>
    public class ClimaxZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CardUI climaxSlotUI;

        /// <summary>
        /// Updates the Climax Zone display.
        /// </summary>
        /// <param name="climaxZone">The climax zone from the game logic.</param>
        public void UpdateZone(ClimaxZone climaxZone)
        {
            if (climaxSlotUI == null) return;

            if (climaxZone != null && climaxZone.Cards.Any())
            {
                // Climax zone should only have one card.
                climaxSlotUI.SetCard(climaxZone.Cards.FirstOrDefault() as WeissCard);
            }
            else
            {
                // If zone is null or empty, show no card.
                climaxSlotUI.SetCard(null);
            }
        }
    }
}
