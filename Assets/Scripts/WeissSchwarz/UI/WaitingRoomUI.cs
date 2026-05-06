using System.Collections.Generic;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// プレイヤーの控え室のUI表示を管理するクラスです。
    /// ゲームの論理的な控え室（DiscardPile）の状態をUIに反映させます。
    /// </summary>
    public class WaitingRoomUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("控え室に表示するカードUIのプレハブ。")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [Tooltip("カードUIを配置するコンテナTransform。")]
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [Tooltip("UI上にスタックして表示する最大枚数。")]
        [SerializeField] private int maxCardsToShow = 10;
        [Tooltip("子オブジェクトのTransformをレイアウトスロットとして使用するかどうか。")]
        [SerializeField] private bool useChildTransformsAsLayoutSlots = true;
        [Tooltip("レイアウトに使用したテンプレートオブジェクトを非表示にするかどうか。")]
        [SerializeField] private bool hideLayoutTemplateObjects = true;

        private readonly List<CardUI> _cardUIs = new List<CardUI>();
        private readonly List<RectTransform> _layoutSlots = new List<RectTransform>();

        private void Awake()
        {
            CacheLayoutSlotsFromChildren();
        }

        /// <summary>
        /// 提供された控え室ゾーンの状態に合わせてUI表示を更新します。
        /// 既存のカードUIオブジェクトをクリアし、現在のカードに対応するUIオブジェクトを生成・配置します。
        /// </summary>
        /// <param name="waitingRoomZone">ゲームロジックからの控え室データ。</param>
        public void UpdateZone(WaitingRoomZone waitingRoomZone)
        {
            // 既存のUIオブジェクトを全て破棄
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear();

            if (waitingRoomZone == null || cardPrefab == null || cardContainer == null)
            {
                return;
            }

            if (useChildTransformsAsLayoutSlots && _layoutSlots.Count == 0)
            {
                CacheLayoutSlotsFromChildren();
            }

            int showCount = Mathf.Min(maxCardsToShow, waitingRoomZone.Cards.Count);
            for (int i = 0; i < showCount; i++)
            {
                var card = waitingRoomZone.Cards[i];
                // カードプレハブをカードコンテナの子としてインスタンス化
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI == null)
                {
                    Debug.LogError("WaitingRoomUI: card prefab is missing CardUI component.");
                    Destroy(cardObject);
                    continue;
                }

                card.IsFaceUp = true;
                cardUI.SetCard(card);
                _cardUIs.Add(cardUI);
                ApplyStackLayout(cardObject.GetComponent<RectTransform>());
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

        private void ApplyStackLayout(RectTransform cardRect)
        {
            if (cardRect == null)
            {
                return;
            }

            if (useChildTransformsAsLayoutSlots && _layoutSlots.Count >= 1 && _layoutSlots[0] != null)
            {
                var slot = _layoutSlots[0];
                CopyTransform(cardRect, slot);
                if (hideLayoutTemplateObjects && slot.gameObject.activeSelf)
                {
                    slot.gameObject.SetActive(false);
                }
                return;
            }

            cardRect.anchoredPosition = Vector2.zero;
        }

        private static void CopyTransform(RectTransform target, RectTransform source)
        {
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.sizeDelta = source.sizeDelta;
            target.anchoredPosition = source.anchoredPosition;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
        }
    }
}