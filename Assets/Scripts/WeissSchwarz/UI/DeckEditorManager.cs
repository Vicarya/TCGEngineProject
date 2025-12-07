using System.Collections.Generic;
using UnityEngine;
using TCG.Weiss;
using TMPro;
using System.Linq;
using System;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the Deck Editor scene, including loading card data, displaying the card list, and handling deck construction.
    /// </summary>
    public class DeckEditorManager : MonoBehaviour
    {
        public static DeckEditorManager Instance { get; private set; }

        private const int MAX_DECK_SIZE = 50;
        private const int MAX_COPIES_PER_CARD = 4;

        [Header("Search UI")]
        [SerializeField] private TMP_InputField searchInputField;

        [Header("Card List UI")]
        [SerializeField] private GameObject cardListItemPrefab;
        [SerializeField] private Transform cardListContentParent;

        [Header("Card Detail UI")]
        [SerializeField] private GameObject cardDetailViewPrefab;
        [SerializeField] private Transform mainCanvas;

        [Header("Deck Construction UI")]
        [SerializeField] private GameObject deckCardListItemPrefab; // Prefab for an item in the deck list
        [SerializeField] private Transform deckListContentParent; // Parent for deck list items
        [SerializeField] private TextMeshProUGUI deckCountText; // Text to display "X / 50"

        private CardDetailView _cardDetailViewInstance;
        private List<WeissCardData> _allCardData = new List<WeissCardData>();
        private Dictionary<string, int> _currentDeck = new Dictionary<string, int>();
        private Dictionary<string, WeissCardData> _cardDataMap = new Dictionary<string, WeissCardData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (cardDetailViewPrefab != null && mainCanvas != null)
            {
                GameObject detailViewObject = Instantiate(cardDetailViewPrefab, mainCanvas);
                _cardDetailViewInstance = detailViewObject.GetComponent<CardDetailView>();
                _cardDetailViewInstance?.Hide();
            }
            else
            {
                Debug.LogError("CardDetailViewPrefab or MainCanvas is not assigned in DeckEditorManager.");
            }
        }

        private void Start()
        {
            if (searchInputField != null)
            {
                searchInputField.onValueChanged.AddListener(FilterAndDisplayCards);
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
            _allCardData = Data.CardDataImporter.GetAllCardData();
            foreach(var card in _allCardData)
            {
                if(!_cardDataMap.ContainsKey(card.card_no))
                {
                    _cardDataMap.Add(card.card_no, card);
                }
            }
            Debug.Log($"Loaded {_allCardData.Count} cards from SQLite database.");
            // Initially display all cards
            FilterAndDisplayCards(string.Empty);
            UpdateDeckUI();
        }

        private void FilterAndDisplayCards(string query)
        {
            List<WeissCardData> cardsToDisplay;

            if (string.IsNullOrEmpty(query))
            {
                cardsToDisplay = _allCardData;
            }
            else
            {
                cardsToDisplay = _allCardData
                    .Where(card => card.name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            DisplayCardList(cardsToDisplay);
        }

        private void DisplayCardList(List<WeissCardData> cardsToDisplay)
        {
            foreach (Transform child in cardListContentParent) Destroy(child.gameObject);
            if (cardListItemPrefab == null)
            {
                Debug.LogError("CardListItemPrefab is not assigned.");
                return;
            }

            foreach (var cardData in cardsToDisplay)
            {
                GameObject newItemObject = Instantiate(cardListItemPrefab, cardListContentParent);
                CardListItem newItem = newItemObject.GetComponent<CardListItem>();
                if (newItem != null)
                {
                    newItem.SetCardData(cardData);
                }
            }
        }

        public void AddCardToDeck(WeissCardData cardData)
        {
            if (cardData == null) return;

            int currentDeckSize = _currentDeck.Values.Sum();
            if (currentDeckSize >= MAX_DECK_SIZE)
            {
                Debug.LogWarning("Cannot add card: Deck is full (50 cards).");
                return;
            }

            _currentDeck.TryGetValue(cardData.card_no, out int currentCopies);
            if (currentCopies >= MAX_COPIES_PER_CARD)
            {
                Debug.LogWarning($"Cannot add card: Maximum copies ({MAX_COPIES_PER_CARD}) of {cardData.name} already in deck.");
                return;
            }

            _currentDeck[cardData.card_no] = currentCopies + 1;
            UpdateDeckUI();
            _cardDetailViewInstance?.UpdateCardCount(cardData);
        }

        public void RemoveCardFromDeck(WeissCardData cardData)
        {
            if (cardData == null || !_currentDeck.ContainsKey(cardData.card_no)) return;

            _currentDeck[cardData.card_no]--;

            if (_currentDeck[cardData.card_no] <= 0)
            {
                _currentDeck.Remove(cardData.card_no);
            }
            UpdateDeckUI();
            _cardDetailViewInstance?.UpdateCardCount(cardData);
        }

        private void UpdateDeckUI()
        {
            foreach (Transform child in deckListContentParent) Destroy(child.gameObject);
            if (deckCardListItemPrefab == null)
            {
                return;
            }

            var sortedDeck = _currentDeck.OrderBy(kvp => _cardDataMap[kvp.Key].card_no);

            foreach (var deckEntry in sortedDeck)
            {
                WeissCardData cardData = _cardDataMap[deckEntry.Key];
                int count = deckEntry.Value;

                GameObject newItemObject = Instantiate(deckCardListItemPrefab, deckListContentParent);
                DeckCardListItem newItem = newItemObject.GetComponent<DeckCardListItem>();
                if (newItem != null)
                {
                    newItem.Setup(cardData, count, RemoveCardFromDeck);
                }
            }
            
            if(deckCountText != null)
            {
                deckCountText.text = $"{_currentDeck.Values.Sum()} / {MAX_DECK_SIZE}";
            }

            // If detail view is open, update its count as well
            _cardDetailViewInstance?.UpdateCardCount();
        }

        public void ShowCardDetail(WeissCardData cardData)
        {
            WeissCard dummyCard = new WeissCard(cardData, null);
            _cardDetailViewInstance?.Show(dummyCard, GetCardCountInDeck(cardData));
        }

        public int GetCardCountInDeck(WeissCardData cardData)
        {
            if (cardData == null) return 0;
            return _currentDeck.TryGetValue(cardData.card_no, out int count) ? count : 0;
        }
    }
}
