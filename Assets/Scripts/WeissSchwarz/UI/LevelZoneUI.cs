using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Level Zone.
    /// </summary>
    public class LevelZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 100f;

        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// 提供されたレベルゾーンの状態に合わせてUI表示を更新します。
        /// 既存のカードUIオブジェクトをクリアし、新しいレベルゾーンのカードに対応するUIオブジェクトを生成・配置します。
        /// </summary>
        /// <param name="levelZone">ゲームロジックからのレベルゾーンデータ。</param>
        public void UpdateZone(LevelZone levelZone)
        {
            // 既存のUIオブジェクトを全て破棄
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear(); // リストもクリア

            // レベルゾーンがnullの場合は何もしない
            if (levelZone == null) return;

            // レベルゾーン内の各カードに対して新しいUIオブジェクトを生成
            for (int i = 0; i < levelZone.LevelCards.Count; i++)
            {
                var levelCardInfo = levelZone.LevelCards[i];
                var card = levelCardInfo.Card;

                // カードプレハブをカードコンテナの子としてインスタンス化
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // IMPORTANT: Set the face-up state before setting the card data
                    // レベルカードの表示状態（表向きか裏向きか）を設定してからカードデータを設定します。
                    // これにより、UIが正確なカードの向きを反映するようにします。
                    card.IsFaceUp = levelCardInfo.FaceUp;
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
