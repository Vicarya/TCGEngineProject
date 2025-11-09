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
        public static GameView Instance { get; private set; }

        [Header("Player UI Zone Managers")]
        [SerializeField] private HandZoneUI handZoneUI;
        [SerializeField] private StageZoneUI stageZoneUI;
        [SerializeField] private ClimaxZoneUI climaxZoneUI;
        [SerializeField] private LevelZoneUI levelZoneUI;
        [SerializeField] private StockZoneUI stockZoneUI;
        [SerializeField] private ClockZoneUI clockZoneUI;
        [SerializeField] private WaitingRoomUI waitingRoomUI;
        [SerializeField] private DeckZoneUI deckZoneUI;
        [SerializeField] private MemoryZoneUI memoryZoneUI;

        [Header("Card Detail View")]
        [SerializeField] private CardDetailView cardDetailView;

        [Header("Interaction Buttons")]
        [SerializeField] private GameObject mulliganConfirmButton;

        private UIGamePlayerController _activeController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            mulliganConfirmButton?.SetActive(false);
            cardDetailView?.Hide(); // Ensure card detail view starts hidden
        }

        /// <summary>
        /// Displays the detailed information for a given card.
        /// </summary>
        /// <param name="card">The WeissCard to display details for.</param>
        public void ShowCardDetail(WeissCard card)
        {
            cardDetailView?.Show(card);
        }

        /// <summary>
        /// Hides the card detail view.
        /// </summary>
        public void HideCardDetail()
        {
            cardDetailView?.Hide();
        }

        public void BeginMulliganSelection(UIGamePlayerController controller)
        {
            _activeController = controller;
            handZoneUI.EnterMulliganSelectionMode();
            mulliganConfirmButton?.SetActive(true);
            Debug.Log("GameView: Mulligan selection started. Confirm button is now visible.");
        }

        public void ConfirmMulliganClicked()
        {
            if (_activeController == null) return;

            var selectedCards = handZoneUI.GetSelectedCardsForMulligan();
            _activeController.ConfirmMulligan(selectedCards);

            // Clean up UI
            mulliganConfirmButton?.SetActive(false);
            _activeController = null;
            Debug.Log("GameView: Mulligan confirmed. UI has been reset.");
        }

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

            // Update Climax Zone
            if (climaxZoneUI != null)
            {
                var climaxZone = player.GetZone<IClimaxZone<WeissCard>>() as ClimaxZone;
                climaxZoneUI.UpdateZone(climaxZone);
            }

            if (stockZoneUI != null)
            {
                var stockZone = player.GetZone<IStockZone<WeissCard>>() as StockZone;
                stockZoneUI.UpdateZone(stockZone);
            }

            // Update Clock Zone
            if (clockZoneUI != null)
            {
                var clockZone = player.GetZone<IClockZone<WeissCard>>() as ClockZone;
                clockZoneUI.UpdateZone(clockZone);
            }

            if (waitingRoomUI != null)
            {
                var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>() as WaitingRoomZone;
                waitingRoomUI.UpdateZone(waitingRoom);
            }

            // Update Deck Zone
            if (deckZoneUI != null)
            {
                var deckZone = player.GetZone<IDeckZone<WeissCard>>() as DeckZone;
                deckZoneUI.UpdateZone(deckZone);
            }
            // if (clockZoneUI != null) { ... }
        }
    }
}
