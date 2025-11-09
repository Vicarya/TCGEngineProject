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

        // Compatibility properties for older code that expects snake_case / Japanese field names
        public string card_no { get => CardCode; set => CardCode = value; }
        public string name { get => Name; set => Name = value; }
        public string detail_page_url { get; set; }
        public string image_url { get => ImagePath; set => ImagePath = value; }

        // Japanese aliases (C# allows Unicode identifiers)
        public string サイド { get => Side; set => Side = value; }
        public string 種類 { get => CardType; set => CardType = value; }

        // Numeric fields exposed as strings for compatibility with importer/UI
        public string レベル { get => Level.ToString(); set => Level = int.TryParse(value, out var v) ? v : 0; }
        public string 色 { get => Color; set => Color = value; }
        public string パワー { get => Power.ToString(); set => Power = int.TryParse(value, out var v) ? v : 0; }
        public string ソウル { get => Soul.ToString(); set => Soul = int.TryParse(value, out var v) ? v : 0; }
        public string コスト { get => Cost.ToString(); set => Cost = int.TryParse(value, out var v) ? v : 0; }
        public string レアリティ { get => Rarity; set => Rarity = value; }
        public string トリガー { get => TriggerIcon; set => TriggerIcon = value; }
        public List<string> 特徴 { get => Traits; set => Traits = value; }
        public string flavor_text { get => FlavorText; set => FlavorText = value; }
        public List<string> abilities { get => Abilities; set => Abilities = value; }
    }
}