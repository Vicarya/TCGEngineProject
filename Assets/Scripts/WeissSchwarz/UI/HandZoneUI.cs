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

        private List<CardUI> _cardUIs = new List<CardUI>();
        private bool _isMulliganSelectionMode;

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
        // --------------------------

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
            _cardUIs.Clear(); // リストもクリア

            // 手札ゾーンがnullの場合は何もしない
            if (handZone == null) return;

            // 手札内の各カードに対して新しいUIオブジェクトを生成
            for (int i = 0; i < handZone.Cards.Count; i++)
            {
                var card = handZone.Cards[i];
                // カードプレハブをカードコンテナの子としてインスタンス化
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    cardUI.SetCard(card as WeissCard); // カードデータをCardUIに設定
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
