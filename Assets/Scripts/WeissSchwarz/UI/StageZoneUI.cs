using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the stage, including all 5 character slots.
    /// </summary>
    public class StageZoneUI : MonoBehaviour
    {
        [Header("Stage Slot UI References")]
        [SerializeField] private CardUI frontLeftSlotUI;
        [SerializeField] private CardUI frontCenterSlotUI;
        [SerializeField] private CardUI frontRightSlotUI;
        [SerializeField] private CardUI backLeftSlotUI;
        [SerializeField] private CardUI backRightSlotUI;

        /// <summary>
        /// 提供されたステージゾーンの状態に合わせてUI表示を更新します。
        /// 各スロットのUIを、対応する論理的なステージスロットのカードで更新します。
        /// </summary>
        /// <param name="stageZone">ゲームロジックからのステージゾーンデータ。</param>
        public void UpdateZone(StageZone stageZone)
        {
            if (stageZone == null)
            {
                // ステージゾーンがnullの場合、全てのスロットUIを非表示または空の状態に設定します。
                frontLeftSlotUI.SetCard(null);
                frontCenterSlotUI.SetCard(null);
                frontRightSlotUI.SetCard(null);
                backLeftSlotUI.SetCard(null);
                backRightSlotUI.SetCard(null);
                return;
            }

            // 各ステージスロットに対応するカードをUIに設定します。
            // `Current`プロパティは、そのスロットに現在配置されているカードを返します。
            frontLeftSlotUI.SetCard(stageZone.FrontLeft.Current);
            frontCenterSlotUI.SetCard(stageZone.FrontCenter.Current);
            frontRightSlotUI.SetCard(stageZone.FrontRight.Current);
            backLeftSlotUI.SetCard(stageZone.BackLeft.Current);
            backRightSlotUI.SetCard(stageZone.BackRight.Current);
        }
    }
}
