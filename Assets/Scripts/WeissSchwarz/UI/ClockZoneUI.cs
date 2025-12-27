using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// クロック置場のUI表示を管理するMonoBehaviourクラス。
    /// ゲームロジックのClockZoneの状態を視覚的に表現します。
    /// </summary>
    public class ClockZoneUI : MonoBehaviour
    {
        [Header("UI参照")]
        /// <summary>
        /// カードUIコンポーネントを持つカードPrefab。
        /// </summary>
        [SerializeField] private GameObject cardPrefab; // CardUIコンポーネントを持つべきPrefab
        /// <summary>
        /// 生成されたカードUIを配置するコンテナTransform。
        /// </summary>
        [SerializeField] private Transform cardContainer;

        [Header("レイアウト設定")]
        /// <summary>
        /// カード間の間隔。
        /// </summary>
        [SerializeField] private float cardSpacing = 80f;

        /// <summary>
        /// 現在表示されている各カードUIのリスト。
        /// </summary>
        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// クロック置場のUI表示をゲームロジックの状態に基づいて更新します。
        /// 既存のカードUIはクリアされ、新しいカードUIが生成されて配置されます。
        /// </summary>
        /// <param name="clockZone">ゲームロジックからのClockZoneオブジェクト。</param>
        public void UpdateZone(ClockZone clockZone)
        {
            // 既存のUIオブジェクトをクリア
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject); // GameObjectを破棄
            }
            _cardUIs.Clear(); // リストもクリア

            if (clockZone == null) return;

            // クロックゾーン内の各カードに対してUIを生成・配置
            for (int i = 0; i < clockZone.Cards.Count; i++)
            {
                var card = clockZone.Cards[i];
                // カードPrefabから新しいGameObjectを生成し、コンテナの子にする
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // クロックのカードは常に表向き
                    card.IsFaceUp = true;
                    cardUI.SetCard(card); // CardUIにカードデータを設定
                    _cardUIs.Add(cardUI); // 生成されたCardUIをリストに追加

                    // 水平レイアウトを適用（カードの間隔を調整）
                    var rectTransform = cardObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        // カードを横に並べるように位置を調整
                        rectTransform.anchoredPosition = new Vector2(i * cardSpacing, 0);
                    }
                }
                else
                {
                    Debug.LogError("カードPrefabにCardUIコンポーネントがありません！", cardObject);
                    Destroy(cardObject); // コンポーネントがない場合は破棄
                }
            }
        }
    }
}
