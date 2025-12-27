using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// プレイヤーのクライマックス置場のUI表示を管理するMonoBehaviourクラス。
    /// ゲームロジックのClimaxZoneの状態を視覚的に表現します。
    /// </summary>
    public class ClimaxZoneUI : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private CardUI climaxSlotUI; // クライマックス置場に置かれるカードを表示するためのCardUI

        /// <summary>
        /// ゲームロジックのClimaxZoneの状態に基づいて、UIのクライマックス置場の表示を更新します。
        /// </summary>
        /// <param name="climaxZone">ゲームロジックからのClimaxZoneオブジェクト。</param>
        public void UpdateZone(ClimaxZone climaxZone)
        {
            if (climaxSlotUI == null) return;

            if (climaxZone != null && climaxZone.Cards.Any())
            {
                // クライマックス置場には通常1枚しかカードが置かれないため、最初のカードを表示
                climaxSlotUI.SetCard(climaxZone.Cards.FirstOrDefault() as WeissCard);
            }
            else
            {
                // ゾーンがnullか空の場合は、カードUIを非表示にする
                climaxSlotUI.SetCard(null);
            }
        }
    }
}
