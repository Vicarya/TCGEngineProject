using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Weiss.Data.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    public class DeckEditorManager : MonoBehaviour
    {
        public static DeckEditorManager Instance { get; private set; }

        private const int MAX_DECK_SIZE = 50;
        private const int MAX_COPIES_PER_CARD = 4;

        [Header("UI")]
        [SerializeField] private Transform cardGridContentParent;
        [SerializeField] private Transform paginationParent;
        [SerializeField] private int itemsPerPage = 18;

        [Header("Filters")]
        [SerializeField] private TMP_InputField searchInputField;
        [SerializeField] private TMP_Dropdown colorDropdown;
        [SerializeField] private TMP_Dropdown cardTypeDropdown;
        [SerializeField] private TMP_InputField levelInputField;
        [SerializeField] private TMP_InputField costInputField;
        [SerializeField] private TMP_InputField traitInputField;
        [SerializeField] private TMP_Dropdown workIdDropdown;

        [Header("Detail View")]
        [SerializeField] private GameObject cardDetailViewPrefab;
        [SerializeField] private Transform mainCanvas;

        [Header("Deck UI")]
        [SerializeField] private GameObject deckCardListItemPrefab;
        [SerializeField] private Transform deckListContentParent;
        [SerializeField] private TextMeshProUGUI deckCountText;

        private CardDetailView _cardDetailViewInstance;
        private PaginationUI _paginationUI;

        private List<WeissCardData> _allCardData = new List<WeissCardData>();
        private Dictionary<string, WeissCardData> _cardDataMap = new Dictionary<string, WeissCardData>();
        private List<WeissCardData> _filteredCardData = new List<WeissCardData>();
        private Dictionary<string, int> _currentDeck = new Dictionary<string, int>();

        private int _currentPage = 1;
        private int _totalPages;
        private bool _isUiReady;
        private bool _hasLoadedData;

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
        }

        private void Start()
        {
            CreatePaginationControls();

            if (_paginationUI == null || _paginationUI.NextButton == null || _paginationUI.PrevButton == null)
            {
                Debug.LogError("DeckEditorManager: Pagination UI failed to initialize.");
                return;
            }

            InitializeDropdowns();
            ResetFilterUi();
            RegisterUiListeners();

            _isUiReady = true;
            RefreshCardViewsIfReady();
        }

        private void OnEnable()
        {
            AppManager.OnDataInitialized += HandleDataInitialized;
        }

        private void OnDisable()
        {
            AppManager.OnDataInitialized -= HandleDataInitialized;

            if (searchInputField != null) searchInputField.onValueChanged.RemoveAllListeners();
            if (colorDropdown != null) colorDropdown.onValueChanged.RemoveAllListeners();
            if (cardTypeDropdown != null) cardTypeDropdown.onValueChanged.RemoveAllListeners();
            if (levelInputField != null) levelInputField.onValueChanged.RemoveAllListeners();
            if (costInputField != null) costInputField.onValueChanged.RemoveAllListeners();
            if (traitInputField != null) traitInputField.onValueChanged.RemoveAllListeners();
            if (workIdDropdown != null) workIdDropdown.onValueChanged.RemoveAllListeners();

            if (_paginationUI != null)
            {
                _paginationUI.NextButton.onClick.RemoveAllListeners();
                _paginationUI.PrevButton.onClick.RemoveAllListeners();
            }
        }

        private void CreatePaginationControls()
        {
            if (paginationParent == null)
            {
                Debug.LogError("Pagination Parent is not assigned in the inspector.");
                return;
            }

            GameObject panel = new GameObject("PaginationPanel", typeof(RectTransform), typeof(LayoutElement));
            panel.transform.SetParent(paginationParent, false);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            LayoutElement layoutElement = panel.GetComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1f;
            layoutElement.flexibleHeight = 1f;
            layoutElement.minWidth = 1f;
            layoutElement.minHeight = 1f;

            _paginationUI = panel.AddComponent<PaginationUI>();
        }

        private void HandleDataInitialized()
        {
            _allCardData = Data.CardDataImporter.GetAllCardData();
            _cardDataMap = _allCardData.ToDictionary(card => card.card_no, card => card);
            _hasLoadedData = true;

            if (_isUiReady)
            {
                ResetFilterUi();
            }

            RefreshCardViewsIfReady();
        }

        private void RegisterUiListeners()
        {
            searchInputField?.onValueChanged.AddListener(_ => UpdateCardFilter());
            colorDropdown?.onValueChanged.AddListener(_ => UpdateCardFilter());
            cardTypeDropdown?.onValueChanged.AddListener(_ => UpdateCardFilter());
            levelInputField?.onValueChanged.AddListener(_ => UpdateCardFilter());
            costInputField?.onValueChanged.AddListener(_ => UpdateCardFilter());
            traitInputField?.onValueChanged.AddListener(_ => UpdateCardFilter());
            workIdDropdown?.onValueChanged.AddListener(_ => UpdateCardFilter());

            _paginationUI.NextButton.onClick.AddListener(GoToNextPage);
            _paginationUI.PrevButton.onClick.AddListener(GoToPreviousPage);
        }

        private void InitializeDropdowns()
        {
            if (colorDropdown != null)
            {
                colorDropdown.ClearOptions();
                colorDropdown.AddOptions(new List<string> { "All", "Yellow", "Green", "Red", "Blue" });
                colorDropdown.value = 0;
                colorDropdown.RefreshShownValue();
            }

            if (cardTypeDropdown != null)
            {
                cardTypeDropdown.ClearOptions();
                cardTypeDropdown.AddOptions(new List<string> { "All", "Character", "Event", "Climax" });
                cardTypeDropdown.value = 0;
                cardTypeDropdown.RefreshShownValue();
            }

            if (workIdDropdown != null)
            {
                workIdDropdown.ClearOptions();
                List<string> options = new List<string> { "All" };
                options.AddRange(WorkIdData.AllWorkIds.Select(w => $"{w.Name} ({w.Id})"));
                workIdDropdown.AddOptions(options);
                workIdDropdown.value = 0;
                workIdDropdown.RefreshShownValue();
            }
        }

        private void ResetFilterUi()
        {
            if (searchInputField != null) searchInputField.SetTextWithoutNotify(string.Empty);
            if (levelInputField != null) levelInputField.SetTextWithoutNotify(string.Empty);
            if (costInputField != null) costInputField.SetTextWithoutNotify(string.Empty);
            if (traitInputField != null) traitInputField.SetTextWithoutNotify(string.Empty);

            if (colorDropdown != null)
            {
                colorDropdown.SetValueWithoutNotify(0);
                colorDropdown.RefreshShownValue();
            }

            if (cardTypeDropdown != null)
            {
                cardTypeDropdown.SetValueWithoutNotify(0);
                cardTypeDropdown.RefreshShownValue();
            }

            if (workIdDropdown != null)
            {
                workIdDropdown.SetValueWithoutNotify(0);
                workIdDropdown.RefreshShownValue();
            }
        }

        private void RefreshCardViewsIfReady()
        {
            if (!_isUiReady || !_hasLoadedData)
            {
                return;
            }

            UpdateCardFilter();
            UpdateDeckUI();
        }

        private void UpdateCardFilter()
        {
            WeissCardQuery query = new WeissCardQuery();

            string searchText = searchInputField != null ? searchInputField.text?.Trim() : null;
            query.HasName(searchText);

            string color = (colorDropdown != null && colorDropdown.value > 0) ? colorDropdown.options[colorDropdown.value].text : null;
            query.HasColor(color);

            string cardType = (cardTypeDropdown != null && cardTypeDropdown.value > 0) ? cardTypeDropdown.options[cardTypeDropdown.value].text : null;
            query.IsCardType(cardType);

            int? level = int.TryParse(levelInputField != null ? levelInputField.text : null, out int levelValue) ? levelValue : (int?)null;
            query.HasLevel(level);

            int? cost = int.TryParse(costInputField != null ? costInputField.text : null, out int costValue) ? costValue : (int?)null;
            query.HasCost(cost);

            string trait = traitInputField != null ? traitInputField.text?.Trim() : null;
            query.HasTrait(trait);

            if (workIdDropdown != null && workIdDropdown.value > 0)
            {
                query.HasWorkId(WorkIdData.AllWorkIds[workIdDropdown.value - 1].Id);
            }

            _filteredCardData = query.Apply(_allCardData).ToList();

            _currentPage = 1;
            _totalPages = (int)Math.Ceiling((double)_filteredCardData.Count / itemsPerPage);
            if (_totalPages == 0)
            {
                _totalPages = 1;
            }

            Debug.Log($"DeckEditorManager: loaded={_allCardData.Count}, filtered={_filteredCardData.Count}, pages={_totalPages}");

            DisplayPage(_currentPage);
            UpdatePaginationUI();
        }

        private void DisplayPage(int page)
        {
            foreach (Transform child in cardGridContentParent)
            {
                Destroy(child.gameObject);
            }

            int startIndex = (page - 1) * itemsPerPage;
            int endIndex = Math.Min(startIndex + itemsPerPage, _filteredCardData.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                WeissCardData cardData = _filteredCardData[i];
                GameObject cardObject = new GameObject(cardData.card_no, typeof(RectTransform));
                cardObject.transform.SetParent(cardGridContentParent, false);
                CardGridItem gridItem = cardObject.AddComponent<CardGridItem>();
                gridItem.Initialize(cardData);
            }
        }

        private void UpdatePaginationUI()
        {
            if (_paginationUI == null)
            {
                return;
            }

            _paginationUI.PrevButton.interactable = _currentPage > 1;
            _paginationUI.NextButton.interactable = _currentPage < _totalPages;
            UpdatePageButtons();
        }

        private void UpdatePageButtons()
        {
            List<int> pages = BuildPageButtonPages();

            for (int i = 0; i < _paginationUI.PageButtons.Count; i++)
            {
                Button button = _paginationUI.PageButtons[i];
                button.onClick.RemoveAllListeners();

                if (i >= pages.Count)
                {
                    _paginationUI.SetPageButtonState(i, string.Empty, false, false, false);
                    continue;
                }

                int page = pages[i];
                bool isCurrent = page == _currentPage;
                _paginationUI.SetPageButtonState(i, page.ToString(), true, !isCurrent, isCurrent);

                if (!isCurrent)
                {
                    int capturedPage = page;
                    button.onClick.AddListener(() => GoToPage(capturedPage));
                }
            }
        }

        private List<int> BuildPageButtonPages()
        {
            HashSet<int> pages = new HashSet<int>();
            if (_totalPages <= 0)
            {
                pages.Add(1);
            }
            else
            {
                pages.Add(1);
                pages.Add(_totalPages);
                for (int page = _currentPage - 2; page <= _currentPage + 2; page++)
                {
                    if (page >= 1 && page <= _totalPages)
                    {
                        pages.Add(page);
                    }
                }
            }

            List<int> ordered = pages.OrderBy(page => page).ToList();
            if (ordered.Count <= 7)
            {
                return ordered;
            }

            List<int> centered = ordered
                .OrderBy(page => Mathf.Abs(page - _currentPage))
                .ThenBy(page => page)
                .Take(7)
                .OrderBy(page => page)
                .ToList();

            if (!centered.Contains(1))
            {
                centered[0] = 1;
            }
            if (!centered.Contains(_totalPages))
            {
                centered[centered.Count - 1] = _totalPages;
            }

            return centered.Distinct().OrderBy(page => page).ToList();
        }

        private void GoToPage(int page)
        {
            if (page < 1 || page > _totalPages || page == _currentPage)
            {
                return;
            }

            _currentPage = page;
            DisplayPage(_currentPage);
            UpdatePaginationUI();
        }

        public void GoToNextPage()
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                DisplayPage(_currentPage);
                UpdatePaginationUI();
            }
        }

        public void GoToPreviousPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                DisplayPage(_currentPage);
                UpdatePaginationUI();
            }
        }

        public void AddCardToDeck(WeissCardData cardData)
        {
            if (cardData == null)
            {
                return;
            }

            int currentDeckSize = _currentDeck.Values.Sum();
            if (currentDeckSize >= MAX_DECK_SIZE)
            {
                return;
            }

            _currentDeck.TryGetValue(cardData.card_no, out int currentCopies);
            if (currentCopies >= MAX_COPIES_PER_CARD)
            {
                return;
            }

            _currentDeck[cardData.card_no] = currentCopies + 1;
            UpdateDeckUI();
            _cardDetailViewInstance?.UpdateCardCount(cardData);
        }

        public void RemoveCardFromDeck(WeissCardData cardData)
        {
            if (cardData == null || !_currentDeck.ContainsKey(cardData.card_no))
            {
                return;
            }

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
            foreach (Transform child in deckListContentParent)
            {
                Destroy(child.gameObject);
            }

            if (deckCardListItemPrefab == null)
            {
                return;
            }

            IOrderedEnumerable<KeyValuePair<string, int>> sortedDeck = _currentDeck.OrderBy(kvp => _cardDataMap[kvp.Key].card_no);
            foreach (KeyValuePair<string, int> deckEntry in sortedDeck)
            {
                GameObject newItemObject = Instantiate(deckCardListItemPrefab, deckListContentParent);
                DeckCardListItem newItem = newItemObject.GetComponent<DeckCardListItem>();
                if (newItem != null)
                {
                    newItem.Setup(_cardDataMap[deckEntry.Key], deckEntry.Value);
                }
            }

            if (deckCountText != null)
            {
                deckCountText.text = $"{_currentDeck.Values.Sum()} / {MAX_DECK_SIZE}";
            }

            _cardDetailViewInstance?.UpdateCardCount();
        }

        public void ShowCardDetail(WeissCardData cardData)
        {
            WeissCard dummyCard = new WeissCard(cardData, null);
            _cardDetailViewInstance?.Show(dummyCard, GetCardCountInDeck(cardData));
        }

        public int GetCardCountInDeck(WeissCardData cardData)
        {
            if (cardData == null)
            {
                return 0;
            }

            return _currentDeck.TryGetValue(cardData.card_no, out int count) ? count : 0;
        }
    }
}
