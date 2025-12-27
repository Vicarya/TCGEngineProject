using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Stock Zone.
    /// </summary>
    public class StockZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private Vector2 cardOffset = new Vector2(10, -5); // How much each card in the stack is offset

        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// 提供されたストックゾーンの状態に合わせてUI表示を更新します。
        /// 既存のカードUIオブジェクトをクリアし、新しいストックゾーンのカードに対応するUIオブジェクトを生成・配置します。
        /// </summary>
        /// <param name="stockZone">ゲームロジックからのストックゾーンデータ。</param>
        public void UpdateZone(StockZone stockZone)
        {
            // 既存のUIオブジェクトを全て破棄
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear(); // リストもクリア

            // ストックゾーンがnullの場合は何もしない
            if (stockZone == null) return;

            // ストックはLIFO（後入れ先出し）ですが、UIではカードの枚数を視覚的に示すため、
            // 生成順に重ねて表示します。
            for (int i = 0; i < stockZone.Cards.Count; i++)
            {
                var card = stockZone.Cards[i];
                // カードプレハブをカードコンテナの子としてインスタンス化
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // ストックに置かれているカードは常に裏向きです。
                    card.IsFaceUp = false;
                    cardUI.SetCard(card); // カードデータをCardUIに設定
                    _cardUIs.Add(cardUI); // 生成したCardUIをリストに追加

                    // 重ねて表示するためのレイアウトを適用
                    var rectTransform = cardObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        // 各カードを少しずつオフセットして、重ねて表示されるように位置を調整
                        rectTransform.anchoredPosition = i * cardOffset;
                    }
                }
                else
                {
                    // CardUIコンポーネントが見つからない場合はエラーをログに記録し、オブジェクトを破棄
                    Debug.LogError("Card prefab is missing CardUI component!");
                    Destroy(cardObject);
                }
            }
        }
    }
}
