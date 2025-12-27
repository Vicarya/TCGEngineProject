using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Memory Zone.
    /// </summary>
    public class MemoryZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 30f;

        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// 提供されたメモリーゾーンの状態に合わせてUI表示を更新します。
        /// 既存のカードUIオブジェクトをクリアし、新しいメモリーゾーンのカードに対応するUIオブジェクトを生成・配置します。
        /// </summary>
        /// <param name="memoryZone">ゲームロジックからのメモリーゾーンデータ。</param>
        public void UpdateZone(MemoryZone memoryZone)
        {
            // 既存のUIオブジェクトを全て破棄
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear(); // リストもクリア

            // メモリーゾーンがnullの場合は何もしない
            if (memoryZone == null) return;

            // メモリーゾーン内の各カードに対して新しいUIオブジェクトを生成
            for (int i = 0; i < memoryZone.MemoryCards.Count; i++)
            {
                var memoryCardInfo = memoryZone.MemoryCards[i];
                var card = memoryCardInfo.Card;

                // カードプレハブをカードコンテナの子としてインスタンス化
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // Set the face-up state before setting the card data
                    // メモリーカードの表示状態（表向きか裏向きか）を設定してからカードデータを設定します。
                    // これにより、UIが正確なカードの向きを反映するようにします。
                    card.IsFaceUp = memoryCardInfo.FaceUp;
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
            }
        }
    }
}
