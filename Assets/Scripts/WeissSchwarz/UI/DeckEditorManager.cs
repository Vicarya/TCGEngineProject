using System.Collections.Generic;
using UnityEngine;
using TCG.Weiss.Core;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the Deck Editor scene, including loading card data and displaying the card list.
    /// </summary>
    public class DeckEditorManager : MonoBehaviour
    {
        public static DeckEditorManager Instance { get; private set; }

        [Header("UI Prefabs & Parents")]
        [SerializeField] private GameObject cardListItemPrefab; // Prefab for a single card in the list
        [SerializeField] private GameObject cardDetailViewPrefab; // Prefab for the card detail view
        [SerializeField] private Transform cardListContentParent; // Parent for card list items
        [SerializeField] private Transform mainCanvas; // Canvas to instantiate UI elements on

        private CardDetailView _cardDetailViewInstance;
        private List<WeissCardData> _allCardData = new List<WeissCardData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Instantiate the Card Detail View from a prefab and hide it initially.
            if (cardDetailViewPrefab != null && mainCanvas != null)
            {
                GameObject detailViewObject = Instantiate(cardDetailViewPrefab, mainCanvas);
                _cardDetailViewInstance = detailViewObject.GetComponent<CardDetailView>();
                _cardDetailViewInstance?.Hide(); // Ensure card detail view starts hidden
            }
            else
            {
                Debug.LogError("CardDetailViewPrefab or MainCanvas is not assigned in DeckEditorManager.");
            }
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

            if (cardListItemPrefab == null)
            {
                Debug.LogError("CardListItemPrefab is not assigned.");
                return;
            }

            foreach (var cardData in _allCardData)
            {
                GameObject newItemObject = Instantiate(cardListItemPrefab, cardListContentParent);
                CardListItem newItem = newItemObject.GetComponent<CardListItem>();
                if (newItem != null)
                {
                    newItem.SetCardData(cardData);
                }
            }
        }

        /// <summary>
        /// Shows the CardDetailView for a specific card.
        /// </summary>
        /// <param name="cardData">The card data to display.</param>
        public void ShowCardDetail(WeissCardData cardData)
        {
            // Create a dummy WeissCard instance for display in CardDetailView
            WeissCard dummyCard = new WeissCard(cardData, null); // Player is null for display purposes
            _cardDetailViewInstance?.Show(dummyCard);
        }
    }
}
