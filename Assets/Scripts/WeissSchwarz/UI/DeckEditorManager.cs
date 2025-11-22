using System.Collections.Generic;
using UnityEngine;
using TCG.Core; // For Card
using TCG.Weiss.Core;
using System.Threading.Tasks; // For async operations
using Newtonsoft.Json; // For JSON deserialization

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the Deck Editor scene, including loading card data and displaying the card list.
    /// </summary>
    public class DeckEditorManager : MonoBehaviour
    {
        public static DeckEditorManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Transform cardListContentParent; // Parent for card list items
        [SerializeField] private CardListItem cardListItemPrefab; // Prefab for a single card in the list
        [SerializeField] private CardDetailView cardDetailView; // Reference to the CardDetailView

        private List<WeissCardData> _allCardData = new List<WeissCardData>();

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

            cardDetailView?.Hide(); // Ensure card detail view starts hidden
        }

        private void OnEnable()
        {
            AppManager.OnDataInitialized += HandleDataInitialized;
        }

        private void OnDisable()
        {
            AppManager.OnDataInitialized -= HandleDataInitialized;
        }

        private void HandleDataInitialized()
        {
            // Load card data from SQLite database
            _allCardData = Data.CardDataImporter.GetAllCardData();
            Debug.Log($"Loaded {_allCardData.Count} cards from SQLite database.");
            DisplayCardList();
        }

        /// <summary>
        /// Displays the loaded card data in a scrollable list.
        /// </summary>
        private void DisplayCardList()
        {
            // Clear existing list items
            foreach (Transform child in cardListContentParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var cardData in _allCardData)
            {
                CardListItem newItem = Instantiate(cardListItemPrefab, cardListContentParent);
                newItem.SetCardData(cardData);
            }
        }

        /// <summary>
        /// Shows the CardDetailView for a specific card.
        /// </summary>
        /// <param name="cardData">The card data to display.</param>
        public void ShowCardDetail(WeissCardData cardData)
        {
            // Create a dummy WeissCard instance for display in CardDetailView
            // In a real scenario, you might have a pool of WeissCard objects or create one on demand.
            WeissCard dummyCard = new WeissCard(cardData, null); // Player is null for display purposes
            cardDetailView?.Show(dummyCard);
        }
    }
}
