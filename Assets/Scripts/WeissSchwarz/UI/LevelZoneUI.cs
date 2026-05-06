using System.Collections.Generic;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// プレイヤーのレベル置場のUI表示を管理するクラスです。
    /// ゲームの論理的なレベルゾーンの状態をUIに反映させます。
    /// </summary>
    public class LevelZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("レベル置場に表示するカードUIのプレハブ。")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [Tooltip("カードUIを配置するコンテナTransform。")]
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [Tooltip("カード間の水平方向の間隔（フォールバック用）。")]
        [SerializeField] private float cardSpacing = 100f;
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
        /// 提供されたレベルゾーンの状態に合わせてUI表示を更新します。
        /// 既存のカードUIオブジェクトをクリアし、現在のカードに対応するUIオブジェクトを生成・配置します。
        /// </summary>
        /// <param name="levelZone">ゲームロジックからのレベルゾーンデータ。</param>
        public void UpdateZone(LevelZone levelZone)
        {
            // 既存のUIオブジェクトを全て破棄
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear();

            if (levelZone == null || cardPrefab == null || cardContainer == null)
            {
                return;
            }

            if (useChildTransformsAsLayoutSlots && _layoutSlots.Count == 0)
            {
                CacheLayoutSlotsFromChildren();
            }

            for (int i = 0; i < levelZone.LevelCards.Count; i++)
            {
                var levelCardInfo = levelZone.LevelCards[i];
                var card = levelCardInfo.Card;

                // カードプレハブをカードコンテナの子としてインスタンス化
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI == null)
                {
                    Debug.LogError("LevelZoneUI: card prefab is missing CardUI component.");
                    Destroy(cardObject);
                    continue;
                }

                card.IsFaceUp = levelCardInfo.FaceUp;
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
