using UnityEngine;
using TCG.Core;
using System;

namespace TCG.Weiss {
    [CreateAssetMenu(fileName = "WeissCardData", menuName = "TCG/WeissCardData")]
    public class WeissCardData : CardData
    {
        public int Level;               // レベル
        public int Cost;                // コスト
        public int Power;               // パワー
        public int Soul;                // ソウル
        public string Color
            => Atrribute;               // 色（Weissでは属性を色で表現）   
        public string CardType;         // キャラ/イベント/クライマックス
        public string TriggerIcon;      // トリガーアイコン（キャラカードのみ）
        public string Trait1 
            => Description[0];          // 特徴1（キャラカードのみ）
        public string Trait2
            => Description[1];          // 特徴2（キャラカードのみ）
        public string FlavorText
            => Description.Length > 2 ? Description[2] : "";    // フレーバーテキスト（キャラカードのみ）
    }
}