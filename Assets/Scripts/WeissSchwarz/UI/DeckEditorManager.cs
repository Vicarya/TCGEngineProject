using System.Collections.Generic;
using UnityEngine;
using TCG.Weiss;
using TMPro;
using System.Linq;
using System;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// デッキエディタシーンを管理するMonoBehaviourクラス。
    /// カードデータのロード、カードリストの表示、デッキ構築の処理などを行います。
    /// </summary>
    public class DeckEditorManager : MonoBehaviour
    {
        /// <summary>
        /// DeckEditorManagerのシングルトンインスタンス。
        /// </summary>
        public static DeckEditorManager Instance { get; private set; }

        /// <summary>
        /// デッキの最大枚数。
        /// </summary>
        private const int MAX_DECK_SIZE = 50;
        /// <summary>
        /// 1種類のカードのデッキ内最大枚数。
        /// </summary>
        private const int MAX_COPIES_PER_CARD = 4;

        /// <summary>
        /// 検索入力フィールド。
        /// </summary>
        [Header("検索UI")]
        [SerializeField] private TMP_InputField searchInputField;

        [Header("カードリストUI")]
        /// <summary>
        /// カードリスト項目のPrefab。
        /// </summary>
        [SerializeField] private GameObject cardListItemPrefab;
        /// <summary>
        /// カードリストのContent要素の親Transform。
        /// </summary>
        [SerializeField] private Transform cardListContentParent;

        [Header("カード詳細UI")]
        /// <summary>
        /// カード詳細ビューのPrefab。
        /// </summary>
        [SerializeField] private GameObject cardDetailViewPrefab;
        /// <summary>
        /// カード詳細ビューをインスタンス化するメインCanvas。
        /// </summary>
        [SerializeField] private Transform mainCanvas;

        [Header("デッキ構築UI")]
        /// <summary>
        /// デッキリスト内のアイテム用Prefab。
        /// </summary>
        [SerializeField] private GameObject deckCardListItemPrefab; // デッキリスト内のアイテム用Prefab
        /// <summary>
        /// デッキリストアイテムの親Transform。
        /// </summary>
        [SerializeField] private Transform deckListContentParent; // デッキリストアイテムの親
        /// <summary>
        /// 現在のデッキ枚数を表示するTextMeshProUGUI ("X / 50"形式)。
        /// </summary>
        [SerializeField] private TextMeshProUGUI deckCountText; // "X / 50"を表示するテキスト

        /// <summary>
        /// 生成されたカード詳細ビューのインスタンス。
        /// </summary>
        private CardDetailView _cardDetailViewInstance;
        /// <summary>
        /// ロードされたすべてのカードデータのリスト。
        /// </summary>
        private List<WeissCardData> _allCardData = new List<WeissCardData>();
        /// <summary>
        /// 現在構築中のデッキ（カードIDと枚数のペア）。
        /// </summary>
        private Dictionary<string, int> _currentDeck = new Dictionary<string, int>();
        /// <summary>
        /// カードIDをキーとするカードデータのマップ。
        /// </summary>
        private Dictionary<string, WeissCardData> _cardDataMap = new Dictionary<string, WeissCardData>();

        /// <summary>
        /// オブジェクトの初期化時に呼び出され、シングルトンインスタンスの設定とカード詳細ビューの準備を行います。
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // カード詳細ビューを生成し、非表示状態で初期化
            if (cardDetailViewPrefab != null && mainCanvas != null)
            {
                GameObject detailViewObject = Instantiate(cardDetailViewPrefab, mainCanvas);
                _cardDetailViewInstance = detailViewObject.GetComponent<CardDetailView>();
                _cardDetailViewInstance?.Hide();
            }
            else
            {
                Debug.LogError("DeckEditorManager: CardDetailViewPrefab または MainCanvas が割り当てられていません。", this);
            }
        }

        /// <summary>
        /// 最初のフレーム更新前に呼び出され、検索入力フィールドのイベントリスナーを設定します。
        /// </summary>
        private void Start()
        {
            if (searchInputField != null)
            {
                searchInputField.onValueChanged.AddListener(FilterAndDisplayCards);
            }
        }

        /// <summary>
        /// オブジェクトが有効になったときに呼び出され、AppManagerからのデータ初期化イベントを購読します。
        /// </summary>
        private void OnEnable()
        {
            AppManager.OnDataInitialized += HandleDataInitialized;
        }

        /// <summary>
        /// オブジェクトが無効になったときに呼び出され、AppManagerからのデータ初期化イベントの購読を解除します。
        /// </summary>
        private void OnDisable()
        {
            AppManager.OnDataInitialized -= HandleDataInitialized;
        }

        /// <summary>
        /// AppManagerによるデータ初期化が完了した際に呼び出されます。
        /// SQLiteデータベースからすべてのカードデータをロードし、初期表示を更新します。
        /// </summary>
        private void HandleDataInitialized()
        {
            _allCardData = Data.CardDataImporter.GetAllCardData();
            // カードデータをマップに格納し、card_noで高速参照できるようにする
            foreach(var card in _allCardData)
            {
                if(!_cardDataMap.ContainsKey(card.card_no))
                {
                    _cardDataMap.Add(card.card_no, card);
                }
            }
            Debug.Log($"SQLiteデータベースから{_allCardData.Count}枚のカードをロードしました。");
            // 初期状態としてすべてのカードを表示
            FilterAndDisplayCards(string.Empty);
            // デッキUIを更新（最初は空デッキ）
            UpdateDeckUI();
        }

        /// <summary>
        /// 検索クエリに基づいてカードリストをフィルタリングし、UIに表示します。
        /// </summary>
        /// <param name="query">カード名をフィルタリングするための検索文字列。</param>
        private void FilterAndDisplayCards(string query)
        {
            List<WeissCardData> cardsToDisplay;

            if (string.IsNullOrEmpty(query))
            {
                // クエリが空の場合はすべてのカードを表示
                cardsToDisplay = _allCardData;
            }
            else
            {
                // クエリに部分一致するカードをフィルタリング
                cardsToDisplay = _allCardData
                    .Where(card => card.name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            DisplayCardList(cardsToDisplay);
        }

        /// <summary>
        /// 指定されたカードデータのリストをカードリストUIに表示します。
        /// 既存のリスト項目はすべて破棄され、新しく生成されます。
        /// </summary>
        /// <param name="cardsToDisplay">表示するWeissCardDataのリスト。</param>
        private void DisplayCardList(List<WeissCardData> cardsToDisplay)
        {
            // 既存のリスト項目をクリア
            foreach (Transform child in cardListContentParent) Destroy(child.gameObject);
            if (cardListItemPrefab == null)
            {
                Debug.LogError("CardListItemPrefabが割り当てられていません。");
                return;
            }

            // 各カードデータに対応するリスト項目を生成し、UIに設定
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

        /// <summary>
        /// 指定されたカードをデッキに追加します。
        /// デッキの最大枚数や同一カードの最大枚数を超過しないかチェックします。
        /// </summary>
        /// <param name="cardData">デッキに追加するカードのデータ。</param>
        public void AddCardToDeck(WeissCardData cardData)
        {
            if (cardData == null) return;

            int currentDeckSize = _currentDeck.Values.Sum();
            if (currentDeckSize >= MAX_DECK_SIZE)
            {
                Debug.LogWarning("カードを追加できません: デッキが最大枚数 (50枚) に達しています。");
                return;
            }

            _currentDeck.TryGetValue(cardData.card_no, out int currentCopies);
            if (currentCopies >= MAX_COPIES_PER_CARD)
            {
                Debug.LogWarning($"カードを追加できません: {cardData.name} は既に最大枚数 ({MAX_COPIES_PER_CARD}枚) デッキに入っています。");
                return;
            }

            _currentDeck[cardData.card_no] = currentCopies + 1;
            UpdateDeckUI(); // デッキUIを更新
            _cardDetailViewInstance?.UpdateCardCount(cardData); // カード詳細ビューの枚数表示も更新
        }

        /// <summary>
        /// 指定されたカードをデッキから1枚削除します。
        /// </summary>
        /// <param name="cardData">デッキから削除するカードのデータ。</param>
        public void RemoveCardFromDeck(WeissCardData cardData)
        {
            if (cardData == null || !_currentDeck.ContainsKey(cardData.card_no)) return;

            _currentDeck[cardData.card_no]--; // 枚数を減らす

            if (_currentDeck[cardData.card_no] <= 0)
            {
                _currentDeck.Remove(cardData.card_no); // 枚数が0以下になったらエントリを削除
            }
            UpdateDeckUI(); // デッキUIを更新
            _cardDetailViewInstance?.UpdateCardCount(cardData); // カード詳細ビューの枚数表示も更新
        }

        /// <summary>
        /// 現在構築中のデッキの内容に基づいて、デッキリストUIとデッキ枚数表示を更新します。
        /// </summary>
        private void UpdateDeckUI()
        {
            // 既存のデッキリスト項目をクリア
            foreach (Transform child in deckListContentParent) Destroy(child.gameObject);
            if (deckCardListItemPrefab == null)
            {
                Debug.LogError("DeckCardListItemPrefabが割り当てられていません。");
                return;
            }

            // デッキ内のカードをカード番号でソートして表示
            var sortedDeck = _currentDeck.OrderBy(kvp => _cardDataMap[kvp.Key].card_no);

            // 各デッキエントリに対応するリスト項目を生成し、UIに設定
            foreach (var deckEntry in sortedDeck)
            {
                WeissCardData cardData = _cardDataMap[deckEntry.Key];
                int count = deckEntry.Value;

                GameObject newItemObject = Instantiate(deckCardListItemPrefab, deckListContentParent);
                DeckCardListItem newItem = newItemObject.GetComponent<DeckCardListItem>();
                if (newItem != null)
                {
                    // 削除アクションをDeckEditorManagerのRemoveCardFromDeckにバインド
                    newItem.Setup(cardData, count, RemoveCardFromDeck);
                }
            }
            
            // デッキ枚数テキストを更新
            if(deckCountText != null)
            {
                deckCountText.text = $"{_currentDeck.Values.Sum()} / {MAX_DECK_SIZE}";
            }

            // カード詳細ビューが開いている場合はその枚数表示も更新
            _cardDetailViewInstance?.UpdateCardCount();
        }

        /// <summary>
        /// 指定されたカードデータの詳細ビューを表示します。
        /// </summary>
        /// <param name="cardData">詳細を表示するカードデータ。</param>
        public void ShowCardDetail(WeissCardData cardData)
        {
            // WeissCardDataから一時的なWeissCardインスタンスを作成して詳細表示
            WeissCard dummyCard = new WeissCard(cardData, null);
            _cardDetailViewInstance?.Show(dummyCard, GetCardCountInDeck(cardData));
        }

        /// <summary>
        /// 指定されたカードが現在デッキに何枚入っているかを取得します。
        /// </summary>
        /// <param name="cardData">枚数を取得するカードデータ。</param>
        /// <returns>デッキ内のカード枚数。</returns>
        public int GetCardCountInDeck(WeissCardData cardData)
        {
            if (cardData == null) return 0;
            return _currentDeck.TryGetValue(cardData.card_no, out int count) ? count : 0;
        }
    }
}
