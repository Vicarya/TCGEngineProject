using UnityEngine;
using TCG.Core;
using System;
using System.Collections.Generic; // Added for List<string>

namespace TCG.Weiss {
    [System.Serializable]
    public class WeissCardData : CardData
    {
        public int Level;               // レベル
        public int Cost;                // コスト
        public int Power;               // パワー
        public int Soul;                // ソウル (Direct field)
        public string Side;             // サイド (Direct field)
        public string Color;            // 色（Weissでは属性を色で表現）(Direct field)
        public string CardType;         // キャラ/イベント/クライマックス
        public string TriggerIcon;      // トリガーアイコン（キャラカードのみ）
        public string FlavorText;       // フレーバーテキスト (Direct field)
        public List<string> Abilities;  // 能力 (Direct field, deserialized from JSON)
        public List<string> Traits;     // 特徴 (Direct field, deserialized from JSON)
    }
}