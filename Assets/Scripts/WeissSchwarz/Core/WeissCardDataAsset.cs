using UnityEngine;

namespace TCG.Weiss {
    /// <summary>
    /// WeissCardDataをUnityのScriptableObjectとしてラップするクラス。
    /// これにより、カードデータをUnityエディタ上でアセットファイル（.asset）として
    /// 直接作成、編集、管理できるようになります。
    /// </summary>
    [CreateAssetMenu(menuName = "TCG/Weiss Card Data Asset", fileName = "WeissCard_")]
    public class WeissCardDataAsset : ScriptableObject
    {
        /// <summary>
        /// このアセットが保持する、実際のカードデータ。
        /// </summary>
        public WeissCardData Data = new WeissCardData();
    }
}
