using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Waiting Room (Discard Pile).
    /// </summary>
    public class WaitingRoomUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 20f;
        [SerializeField] private int maxCardsToShow = 10; // To prevent performance issues

        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// 提供された控え室ゾーンの状態に合わせてUI表示を更新します。
        /// 既存のカードUIオブジェクトをクリアし、新しい控え室のカードに対応するUIオブジェクトを生成・配置します。
        /// </summary>
        /// <param name="waitingRoomZone">ゲームロジックからの控え室ゾーンデータ。</param>
        public void UpdateZone(WaitingRoomZone waitingRoomZone)
        {
            // 既存のUIオブジェクトを全て破棄
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear(); // リストもクリア

            // 控え室ゾーンがnullの場合は何もしない
            if (waitingRoomZone == null) return;

            // 控え室のカードは多数になる可能性があるため、表示上のパフォーマンスを考慮し、
            // 最大表示枚数（maxCardsToShow）に制限して表示します。
            var cardsToShow = waitingRoomZone.Cards.Take(maxCardsToShow);
            int i = 0;
            foreach(var card in cardsToShow)
            {
                // カードプレハブをカードコンテナの子としてインスタンス化
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // 控え室のカードは常に表向きです。
                    card.IsFaceUp = true;
                    cardUI.SetCard(card); // カードデータをCardUIに設定
                    _cardUIs.Add(cardUI); // 生成したCardUIをリストに追加

                    // 基本的な水平レイアウトを適用
                    var rectTransform = cardObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        // カードの間隔を考慮して位置を設定
                        rectTransform.anchoredPosition = new Vector2(i * cardSpacing, 0);
                    }
                }
                else
                {
                    // CardUIコンポーネントが見つからない場合はエラーをログに記録し、オブジェクトを破棄
                    Debug.LogError("Card prefab is missing CardUI component!");
                    Destroy(cardObject);
                }
                i++;
            }
        }
    }
}
