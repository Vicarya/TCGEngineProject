using System.Collections.Generic;
using System.Threading.Tasks;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// プレイヤーの手札表示を管理するUIコンポーネント。
    /// 通常表示に加えて、カード選択要求中は選択可能カード・選択済みカード・必要枚数を保持します。
    /// </summary>
    public class HandZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 100f;
        [SerializeField] private bool useChildTransformsAsLayoutSlots = true;
        [SerializeField] private bool hideLayoutTemplateObjects = true;

        private readonly List<CardUI> _cardUIs = new List<CardUI>();
        private readonly List<RectTransform> _layoutSlots = new List<RectTransform>();
        private HandSelectionSession _activeSelectionSession;
        private TaskCompletionSource<List<WeissCard>> _selectionCompletionSource;

        private sealed class HandSelectionSession
        {
            public HashSet<WeissCard> SelectableCards { get; } = new HashSet<WeissCard>();
            public List<WeissCard> SelectedCards { get; } = new List<WeissCard>();
            public int MinSelection { get; set; }
            public int MaxSelection { get; set; }
            public bool AllowCancel { get; set; }
        }

        private void Awake()
        {
            CacheLayoutSlotsFromChildren();
        }

        /// <summary>
        /// 手札カードの選択セッションを開始し、確定またはキャンセルされるまで完了しないTaskを返します。
        /// selectableCardsに含まれないカードは半透明かつクリック不可にして、効果やフェイズごとの選択条件をUIへ反映します。
        /// </summary>
        public Task<List<WeissCard>> BeginCardSelectionAsync(
            IReadOnlyList<WeissCard> selectableCards,
            int minSelection,
            int maxSelection,
            bool allowCancel)
        {
            CancelActiveSelection();

            _activeSelectionSession = new HandSelectionSession
            {
                MinSelection = Mathf.Max(0, minSelection),
                MaxSelection = Mathf.Max(1, maxSelection),
                AllowCancel = allowCancel
            };

            foreach (var card in selectableCards ?? new List<WeissCard>())
            {
                if (card != null)
                {
                    _activeSelectionSession.SelectableCards.Add(card);
                }
            }

            _selectionCompletionSource = new TaskCompletionSource<List<WeissCard>>();
            RefreshCardSelectionState();
            return _selectionCompletionSource.Task;
        }

        /// <summary>
        /// 現在の選択枚数が要求範囲内なら選択結果を確定します。
        /// 枚数不足や上限超過の場合はTaskを完了させず、モーダルを開いたままにします。
        /// </summary>
        public bool TryConfirmSelection()
        {
            if (_activeSelectionSession == null || _selectionCompletionSource == null)
            {
                return false;
            }

            int count = _activeSelectionSession.SelectedCards.Count;
            if (count < _activeSelectionSession.MinSelection || count > _activeSelectionSession.MaxSelection)
            {
                Debug.Log("HandZoneUI: Selection count does not satisfy the current request.");
                return false;
            }

            CompleteSelection(new List<WeissCard>(_activeSelectionSession.SelectedCards));
            return true;
        }

        /// <summary>
        /// キャンセルが許可された選択セッションを0枚選択として完了します。
        /// 必須選択ではfalseを返し、呼び出し元のモーダルを閉じないようにします。
        /// </summary>
        public bool TryCancelSelection()
        {
            if (_activeSelectionSession == null || !_activeSelectionSession.AllowCancel)
            {
                return false;
            }

            CompleteSelection(new List<WeissCard>());
            return true;
        }

        /// <summary>
        /// 進行中の選択セッションを外部都合で終了します。
        /// モーダルの無効化や別入力への切り替え時に、待機中Taskを残さないために使用します。
        /// </summary>
        public void AbortSelection()
        {
            if (_selectionCompletionSource != null && !_selectionCompletionSource.Task.IsCompleted)
            {
                CompleteSelection(new List<WeissCard>());
            }
        }

        public List<WeissCard> GetCurrentSelection()
        {
            if (_activeSelectionSession == null)
            {
                return new List<WeissCard>();
            }

            return new List<WeissCard>(_activeSelectionSession.SelectedCards);
        }

        /// <summary>
        /// 指定カードに対応する表示中のCardUIを取得します。
        /// 初期配札アニメーションなど、ロジック上のカードと画面上の座標を対応付ける処理で使用します。
        /// </summary>
        public bool TryGetCardUI(WeissCard card, out CardUI cardUI)
        {
            cardUI = null;
            if (card == null)
            {
                return false;
            }

            foreach (var current in _cardUIs)
            {
                if (current != null && current.Card == card)
                {
                    cardUI = current;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 初期配札時に一瞬カードを見せるための中間ワールド座標を返します。
        /// レイアウトスロットがあればその下側、なければ手札コンテナまたは自身の位置を基準にします。
        /// </summary>
        public Vector3 GetDealRevealWorldPosition()
        {
            if (_layoutSlots.Count > 0 && _layoutSlots[0] != null)
            {
                var slot = _layoutSlots[0];
                return slot.TransformPoint(new Vector3(0f, -slot.rect.height * 1.8f, 0f));
            }

            if (cardContainer is RectTransform rectTransform)
            {
                return rectTransform.TransformPoint(new Vector3(0f, -220f, 0f));
            }

            return transform.position + Vector3.down * 220f;
        }

        public void UpdateZone(HandZone handZone)
        {
            if (handZone == null || cardPrefab == null || cardContainer == null)
            {
                ClearCardUIs();
                return;
            }

            if (useChildTransformsAsLayoutSlots && _layoutSlots.Count == 0)
            {
                CacheLayoutSlotsFromChildren();
            }

            var reusableCardUIs = new Dictionary<WeissCard, CardUI>();
            foreach (var existingCardUI in _cardUIs)
            {
                if (existingCardUI != null && existingCardUI.Card != null && !reusableCardUIs.ContainsKey(existingCardUI.Card))
                {
                    reusableCardUIs.Add(existingCardUI.Card, existingCardUI);
                }
            }

            var nextCardUIs = new List<CardUI>(handZone.Cards.Count);
            for (int i = 0; i < handZone.Cards.Count; i++)
            {
                var card = handZone.Cards[i] as WeissCard;
                if (card == null)
                {
                    continue;
                }

                CardUI cardUI;
                if (reusableCardUIs.TryGetValue(card, out var reusableCardUI) && reusableCardUI != null)
                {
                    cardUI = reusableCardUI;
                    reusableCardUIs.Remove(card);
                }
                else
                {
                    cardUI = CreateCardUI();
                    if (cardUI == null)
                    {
                        continue;
                    }
                }

                cardUI.SetCard(card);
                cardUI.transform.SetParent(cardContainer, false);
                cardUI.transform.SetSiblingIndex(i);
                ApplyLayout(cardUI.GetComponent<RectTransform>(), i);
                ApplySelectionState(cardUI);
                nextCardUIs.Add(cardUI);
            }

            foreach (var unusedCardUI in reusableCardUIs.Values)
            {
                ReleaseCardUI(unusedCardUI);
            }

            _cardUIs.Clear();
            _cardUIs.AddRange(nextCardUIs);
        }

        /// <summary>
        /// 選択セッション中のカードクリックを処理します。
        /// 単一選択では新しいカードを選ぶたびに既存選択を解除し、複数選択では上限枚数を超えないようにします。
        /// </summary>
        private void HandleCardClicked(CardUI cardUI)
        {
            if (_activeSelectionSession == null || cardUI == null || cardUI.Card == null)
            {
                return;
            }

            if (!_activeSelectionSession.SelectableCards.Contains(cardUI.Card))
            {
                return;
            }

            if (_activeSelectionSession.SelectedCards.Contains(cardUI.Card))
            {
                _activeSelectionSession.SelectedCards.Remove(cardUI.Card);
            }
            else
            {
                if (_activeSelectionSession.MaxSelection == 1)
                {
                    _activeSelectionSession.SelectedCards.Clear();
                }
                else if (_activeSelectionSession.SelectedCards.Count >= _activeSelectionSession.MaxSelection)
                {
                    return;
                }

                _activeSelectionSession.SelectedCards.Add(cardUI.Card);
            }

            RefreshCardSelectionState();
        }

        private void RefreshCardSelectionState()
        {
            foreach (var cardUI in _cardUIs)
            {
                ApplySelectionState(cardUI);
            }
        }

        private void ApplySelectionState(CardUI cardUI)
        {
            if (cardUI == null)
            {
                return;
            }

            if (_activeSelectionSession == null || cardUI.Card == null)
            {
                cardUI.SetSelectionMode(false);
                cardUI.SetSelectable(true);
                cardUI.SetSelected(false);
                return;
            }

            bool isSelectable = _activeSelectionSession.SelectableCards.Contains(cardUI.Card);
            bool isSelected = _activeSelectionSession.SelectedCards.Contains(cardUI.Card);

            cardUI.SetSelectionMode(true);
            cardUI.SetSelectable(isSelectable);
            cardUI.SetSelected(isSelected);
        }

        /// <summary>
        /// 選択セッションを終了し、CardUIを通常表示へ戻して待機中Taskへ結果を返します。
        /// </summary>
        private void CompleteSelection(List<WeissCard> selectedCards)
        {
            _activeSelectionSession = null;
            RefreshCardSelectionState();

            if (_selectionCompletionSource != null && !_selectionCompletionSource.Task.IsCompleted)
            {
                _selectionCompletionSource.SetResult(selectedCards ?? new List<WeissCard>());
            }

            _selectionCompletionSource = null;
        }

        private void CancelActiveSelection()
        {
            AbortSelection();
        }

        private void CacheLayoutSlotsFromChildren()
        {
            _layoutSlots.Clear();
            if (!useChildTransformsAsLayoutSlots || cardContainer == null)
            {
                return;
            }

            foreach (Transform child in cardContainer)
            {
                if (child is RectTransform rect)
                {
                    _layoutSlots.Add(rect);
                }
            }
        }

        private void ApplyLayout(RectTransform cardRect, int index)
        {
            if (cardRect == null)
            {
                return;
            }

            if (useChildTransformsAsLayoutSlots && index < _layoutSlots.Count && _layoutSlots[index] != null)
            {
                var slot = _layoutSlots[index];
                cardRect.anchorMin = slot.anchorMin;
                cardRect.anchorMax = slot.anchorMax;
                cardRect.pivot = slot.pivot;
                cardRect.sizeDelta = slot.sizeDelta;
                cardRect.anchoredPosition = slot.anchoredPosition;
                cardRect.localRotation = slot.localRotation;
                cardRect.localScale = slot.localScale;

                if (hideLayoutTemplateObjects && slot.gameObject.activeSelf)
                {
                    slot.gameObject.SetActive(false);
                }

                return;
            }

            if (useChildTransformsAsLayoutSlots && _layoutSlots.Count >= 2 && _layoutSlots[0] != null && _layoutSlots[1] != null)
            {
                var slot0 = _layoutSlots[0];
                var slot1 = _layoutSlots[1];
                var delta = slot1.anchoredPosition - slot0.anchoredPosition;

                cardRect.anchorMin = slot1.anchorMin;
                cardRect.anchorMax = slot1.anchorMax;
                cardRect.pivot = slot1.pivot;
                cardRect.sizeDelta = slot1.sizeDelta;
                cardRect.localRotation = slot1.localRotation;
                cardRect.localScale = slot1.localScale;
                cardRect.anchoredPosition = slot0.anchoredPosition + (delta * index);
                return;
            }

            cardRect.anchoredPosition = new Vector2(index * cardSpacing, 0f);
        }

        private CardUI CreateCardUI()
        {
            var cardObject = Instantiate(cardPrefab, cardContainer);
            var cardUI = cardObject.GetComponent<CardUI>();

            if (cardUI == null)
            {
                Debug.LogError("HandZoneUI: card prefab is missing CardUI component.");
                Destroy(cardObject);
                return null;
            }

            cardUI.CardClicked += HandleCardClicked;
            return cardUI;
        }

        private void ClearCardUIs()
        {
            foreach (var cardUI in _cardUIs)
            {
                ReleaseCardUI(cardUI);
            }

            _cardUIs.Clear();
        }

        private void ReleaseCardUI(CardUI cardUI)
        {
            if (cardUI == null)
            {
                return;
            }

            cardUI.CardClicked -= HandleCardClicked;
            Destroy(cardUI.gameObject);
        }
    }
}
