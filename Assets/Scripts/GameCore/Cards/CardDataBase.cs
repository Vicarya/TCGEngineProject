using UnityEngine;
using System.Collections.Generic;

namespace TCG.Core {
    [CreateAssetMenu(fileName = "CardData", menuName = "TCG/CardData")]
    public abstract class CardDataBase
    {
        public string CardGuid;         // ユニークID
        public string CardCode;         // カード番号
        public string Name;             // カード名
        public string Set;              // 収録セット
        public string Rarity;           // レアリティ
        public string Atrribute;        // 属性
        public string CardType;         // カードタイプ（クリーチャー、スペルなど）
        public string[] Description;    // 説明文、効果テキスト、フレーバーテキストなど
        public string Illustration;     // イラストレーター
        public string ImagePath;        // 画像パス
        // その他、ゲーム固有のプロパティを追加可能
        
        public Dictionary<string, object> Metadata = new(); // 拡張用メタデータ
    }
}