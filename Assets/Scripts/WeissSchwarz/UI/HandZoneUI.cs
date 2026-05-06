using System.Collections.Generic;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of a player's hand.
    /// プレイヤーの手札のUI表示を管理するクラスです。
    /// ゲームの論理的な手札ゾーンの状態をUIに反映させます。
    /// </summary>
    public class HandZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("手札に表示するカードUIのプレハブ。CardUIコンポーネントを持つ必要があります。")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [Tooltip("カードUIを配置するコンテナTransform。")]
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [Tooltip("カード間の水平方向の間隔。")]
        [SerializeField] private float cardSpacing = 100f;
        [SerializeField] private bool useChildTransformsAsLayoutSlots = true;
        [SerializeField] private bool hideLayoutTemplateObjects = true;

        private readonly List<CardUI> _cardUIs = new List<CardUI>();
        private readonly List<RectTransform> _layoutSlots = new List<RectTransform>();
        private bool _isMulliganSelectionMode;

        private void Awake()
        {
            CacheLayoutSlotsFromChildren();
        }

        // --- Mulligan Selection ---
        /// <summary>
        /// マリガン選択モードを開始します。
        /// このモードでは、プレイヤーは手札からマリガンするカードを選択できるようになります。
        /// </summary>
        public void EnterMulliganSelectionMode()
        {
            Debug.Log("HandZoneUI: Entering mulligan selection mode.");
            _isMulliganSelectionMode = true;
            foreach (var cardUI in _cardUIs)
            {
                cardUI.SetSelectionMode(true);
                cardUI.SetSelected(false);
            }
        }

        /// <summary>
        /// マリガン選択モードを終了し、カード選択状態をクリアします。
        /// </summary>
        public void ExitMulliganSelectionMode()
        {
            _isMulliganSelectionMode = false;
            foreach (var cardUI in _cardUIs)
            {
                cardUI.SetSelectionMode(false);
                cardUI.SetSelected(false);
            }
        }

        /// <summary>
        /// マリガン選択モードでプレイヤーが選択したカードのリストを取得します。
        /// </summary>
        /// <returns>マリガン対象として選択されたWeissCardのリスト。</returns>
        public List<WeissCard> GetSelectedCardsForMulligan()
        {
            Debug.Log("HandZoneUI: Getting selected cards for mulligan.");
            var selectedCards = new List<WeissCard>();
            if (!_isMulliganSelectionMode)
            {
                return selectedCards;
            }

            foreach (var cardUI in _cardUIs)
            {
                if (cardUI != null && cardUI.IsSelected && cardUI.Card != null)
                {
                    selectedCards.Add(cardUI.Card);
                }
            }

            return selectedCards;
        }

        /// <summary>
        /// 提供された手札ゾーンの状態に合わせてUI表示を更新します。
        /// 既存のカードUIオブジェクトをクリアし、新しい手札のカードに対応するUIオブジェクトを生成・配置します。
        /// </summary>
        /// <param name="handZone">ゲームロジックからの手札ゾーンデータ。</param>
        public void UpdateZone(HandZone handZone)
        {
            // 既存のUIオブジェクトを全て破棄
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear();

            if (handZone == null || cardPrefab == null || cardContainer == null)
            {
                return;
            }

            if (useChildTransformsAsLayoutSlots && _layoutSlots.Count == 0)
            {
                CacheLayoutSlotsFromChildren();
            }

            for (int i = 0; i < handZone.Cards.Count; i++)
            {
                var card = handZone.Cards[i] as WeissCard;;
                // カードプレハブをカードコンテナの子としてインスタンス化
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI == null)
                {
                    Debug.LogError("HandZoneUI: card prefab is missing CardUI component.");
                    Destroy(cardObject);
                    continue;
                }

                cardUI.SetCard(card);
                _cardUIs.Add(cardUI);
                ApplyLayout(cardObject.GetComponent<RectTransform>(), i);
            }
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
                var rect = child as RectTransform;
                if (rect != null)
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
    }
}
