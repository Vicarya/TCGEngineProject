using System.Linq;
using TCG.Weiss;
using TMPro;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// 山札置場のUI表示を管理するMonoBehaviourクラス。
    /// </summary>
    public class DeckZoneUI : MonoBehaviour
    {
        [Header("UI参照")]
        /// <summary>
        /// 山札のカードの山（一番上のカード）を表すCardUI。
        /// </summary>
        [SerializeField] private CardUI deckCardUI; // Represents the pile of cards
        /// <summary>
        /// 山札の枚数を表示するTextMeshProUGUI。
        /// </summary>
        [SerializeField] private TextMeshProUGUI deckCountText;

        /// <summary>
        /// 山札置場の表示を更新します。
        /// 山札が残っている場合は一番上のカードを裏向き表示にし、枚数テキストを現在枚数に同期します。
        /// </summary>
        /// <param name="deckZone">ゲームロジック側の山札ゾーン。</param>
        public void UpdateZone(DeckZone deckZone)
        {
            if (deckZone == null)
            {
                if(deckCardUI != null) deckCardUI.gameObject.SetActive(false);
                if(deckCountText != null) deckCountText.text = "0";
                return;
            }

            if (deckCountText != null)
            {
                deckCountText.text = deckZone.Cards.Count.ToString();
            }

            if (deckCardUI != null)
            {
                bool hasCards = deckZone.Cards.Any();
                deckCardUI.gameObject.SetActive(hasCards);

                if (hasCards)
                {
                    // 山札の代表表示として、一番上のカードを必ず裏向きで表示する。
                    var topCard = deckZone.Cards.First();
                    topCard.IsFaceUp = false;
                    deckCardUI.SetCard(topCard);
                }
            }
        }

        /// <summary>
        /// 山札カード表示のワールド座標を取得します。
        /// 初期配札アニメーションの開始位置として使用します。
        /// </summary>
        public bool TryGetDeckCardWorldPosition(out Vector3 position)
        {
            if (deckCardUI != null)
            {
                position = deckCardUI.transform.position;
                return true;
            }

            position = Vector3.zero;
            return false;
        }
    }
}
