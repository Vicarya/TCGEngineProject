using UnityEngine;
using System.Collections.Generic;

namespace TCG.Core {
    /// <summary>
    /// カードの不変の（静的な）データを表現するための抽象基底クラス。
    /// このクラスは、カード名、カードコード、レアリティなど、ゲーム中に変化しない情報を保持します。
    /// ゲーム固有のデータ項目は、このクラスを継承して追加します。
    /// </summary>
    [System.Serializable]
    public abstract class CardData
    {
        /// <summary>
        /// カードを一位に識別するためのGUID。
        /// </summary>
        public string CardGuid;

        /// <summary>
        /// カードの製品番号（例：SAO/S100-001）。
        /// </summary>
        public string CardCode;

        /// <summary>
        /// カードの名前。
        /// </summary>
        public string Name;

        /// <summary>
        /// カードが属する作品ID（例：SAO）。
        /// </summary>
        public string WorkId;

        /// <summary>
        /// カードのレアリティ（例：RR, C）。
        /// </summary>
        public string Rarity;

        /// <summary>
        /// カードのイラストレーター名。
        /// </summary>
        public string Illustration;

        /// <summary>
        /// カード画像のファイルパス。
        /// </summary>
        public string ImagePath;

        /// <summary>
        /// ゲーム固有の追加データを格納するためのメタデータ辞書。
        /// 例えば、色、レベル、パワー、ソウルなどの情報をここに格納できます。
        /// </summary>
        public Dictionary<string, object> Metadata = new();
    }
}